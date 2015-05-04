using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Facepunch.Networking;
using ParseSharp;
using ParseSharp.BackusNaur;
using UnityEngine;

namespace Facepunch.ConCommands
{
    using RegexGroup = System.Text.RegularExpressions.Group;
    using UnityComponent = Component;

    public static class ReflectionCommands
    {
        private static readonly Parser _sLocationParser;
        private static readonly Parser _sValueParser;

        static ReflectionCommands()
        {
            const string ebnf = @"
                (* skip-whitespace omit-from-hierarchy *)
                Location Root = Location, End of Input;

                (* skip-whitespace omit-from-hierarchy *)
                Value Root = Value, End of Input;

                Location = ""["", Root, ""]"", {Accessor};

                (* omit-from-hierarchy *) Root     = Networkable Identifier | Type Name;
                (* omit-from-hierarchy *) Accessor = Member Access | Component Access | Invocation;

                Member Access = ""."", Identifier;
                Component Access = ""<"", Type Name, "">"";

                Invocation = ""("", [Value, {"","", Value}], "")"";

                (* omit-from-hierarchy *)
                Value = Null | Boolean | Vector | Float | Double | (""0x"", Hex Integer) | Integer | String | Location;

                Boolean = True | False;
                Vector = ""Vector"", Invocation;

                Float = (Integer | Decimal), ""f"";
                Double = Decimal, [""d""];

                String = ""'"", {(""\\"", Escaped Character) | Single Character}, ""'"";

                (* collapse *) End of Input = ? /$/ ?;
                (* collapse *) Single Character = ? /[^\\']/ ?;
                (* collapse *) Escaped Character = ? /[\\'rnt]/ ?;
                (* collapse *) Integer = ? /-?[0-9]+/ ?;
                (* collapse *) Hex Integer = ? /[0-9A-F]/i ?;
                (* collapse *) Decimal = ? /-?[0-9]+\.[0-9]+/ ?;

                (* collapse *) Networkable Identifier = ? /[0-9]+/ ?;
                (* collapse *) Type Name = ? /[^0-9\]>][^\]>]*/ ?;
                (* collapse *) Component Name = Type Name;
                (* collapse *) Null = ""null"";
                (* collapse *) True = ? /[Tt]rue/ ?;
                (* collapse *) False = ? /[Ff]alse/ ?;
                (* collapse *) Identifier = ? /[A-Za-z_][A-Za-z0-9_]*/ ?;";

            _sLocationParser = ParserGenerator.FromEBnf(ebnf, "Location Root");
            _sValueParser = ParserGenerator.FromEBnf(ebnf, "Value Root");
        }

        #region Member types

        private abstract class Member
        {
            public abstract object GetValue();
            protected abstract Type GetContextType(object value);

            protected virtual BindingFlags GetBindingFlags()
            {
                return BindingFlags.Public | BindingFlags.NonPublic;
            }

            public virtual void SetValue(object val)
            {
                throw new Exception("Cannot set the value of this member");
            }

            private IEnumerable<MemberInfo> GetMembers(Type type)
            {
                return type.GetMembers(GetBindingFlags());
            }

            public Member FindMember(Domain domain, ParseResult result)
            {
                var context = GetValue();
                var type = GetContextType(context);

                switch (result.Parser.Name) {
                    case "Member Access":
                        var name = result[0].Value;

                        foreach (var member in GetMembers(type).Where(x => x.Name == name)) {
                            var field = member as FieldInfo;
                            if (field != null) return new Field(field, context);
                            var prop = member as PropertyInfo;
                            if (prop != null) return new Property(prop, context);
                            var method = member as MethodInfo;
                            if (method != null) return new Method(method, context);
                        }

                        throw new Exception(string.Format("Could not find {2} member '{0}' in type '{1}'",
                            name, type.FullName, context == null ? "static" : "instance"));
                    case "Component Access":
                        var componentName = result[0].Value;
                        return new Component(componentName, context);
                    case "Invocation":
// ReSharper disable once ConvertClosureToMethodGroup
                        var values = result.Select(x => ReadValue(domain, x)).ToArray();
                        return new Invocation(values, this);
                    default:
                        throw new Exception("This should not occur - parser grammar is incorrect");
                }
            }
        }

        private abstract class Instance : Member
        {
            protected override Type GetContextType(object value)
            {
                return value.GetType();
            }

            protected override BindingFlags GetBindingFlags()
            {
                return base.GetBindingFlags() | BindingFlags.Instance;
            }
        }

        private class Static : Member
        {
            private readonly Type _type;

            public Static(Type type)
            {
                _type = type;
            }

            public override object GetValue()
            {
                return null;
            }

            protected override Type GetContextType(object value)
            {
                return _type;
            }

            protected override BindingFlags GetBindingFlags()
            {
                return base.GetBindingFlags() | BindingFlags.Static;
            }
        }

        private class NetworkableId : Instance
        {
            private readonly Domain _domain;
            private readonly uint _ident;

            public NetworkableId(Domain domain, uint ident)
            {
                _domain = domain;
                _ident = ident;
            }

            public override object GetValue()
            {
                Networkable nw;

                switch (_domain) {
                    case Domain.Client:
                        nw = Client.Instance.GetNetworkable(_ident);
                        break;
                    case Domain.Server:
                        nw = Server.Instance.GetNetworkable(_ident);
                        break;
                    default:
                        throw new ArgumentException("Invalid domain");
                }

                if (nw == null) {
                    throw new Exception(string.Format("Invalid networkable identifier '{0}'", _ident));
                }

                return nw;
            }
        }

        private class Field : Instance
        {
            private readonly FieldInfo _field;
            private readonly object _ctx;

            public Field(FieldInfo field, object ctx)
            {
                _field = field;
                _ctx = ctx;
            }

            public override object GetValue()
            {
                return _field.GetValue(_ctx);
            }

            public override void SetValue(object val)
            {
                _field.SetValue(_ctx, val);
            }
        }

        private class Property : Instance
        {
            private readonly PropertyInfo _prop;
            private readonly object _ctx;

            public Property(PropertyInfo prop, object ctx)
            {
                _prop = prop;
                _ctx = ctx;
            }

            public override object GetValue()
            {
                return _prop.GetValue(_ctx, null);
            }

            public override void SetValue(object val)
            {
                _prop.SetValue(_ctx, val, null);
            }
        }

        private class Method : Instance
        {
            private readonly MethodInfo _meth;
            private readonly object _ctx;

            public Method(MethodInfo meth, object ctx)
            {
                _meth = meth;
                _ctx = ctx;
            }

            public override object GetValue()
            {
                return this;
            }

            public object Invoke(object[] args)
            {
                try {
                    var value = _meth.Invoke(_ctx, args);
                    if (_meth.ReturnType == typeof (void)) value = "void";
                    return value;
                } catch (TargetInvocationException e) {
                    throw e.InnerException;
                }
            }
        }

        private class Component : Instance
        {
            private readonly string _name;
            private readonly object _ctx;

            public Component(string name, object ctx)
            {
                _name = name;
                _ctx = ctx;
            }

            public override object GetValue()
            {
                if (_ctx == null) {
                    throw new Exception("Cannot retrieve components from null");
                }

                var component = _ctx as UnityComponent;

                if (component != null) {
                    return component.GetComponent(_name);
                }

                var gameObj = _ctx as GameObject;
                
                if (gameObj != null) {
                    return gameObj.GetComponent(_name);
                }

                throw new Exception(string.Format("Cannot retrieve components from type '{0}'", _ctx.GetType()));
            }
        }

        private class Invocation : Instance
        {
            private readonly object[] _args;
            private readonly object _ctx;

            public Invocation(object[] args, object ctx)
            {
                _args = args;
                _ctx = ctx;
            }

            public override object GetValue()
            {
                var method = _ctx as Method;
                if (method == null) {
                    throw new Exception("Can only invoke methods.");
                }

                return method.Invoke(_args);
            }
        }

        #endregion

        private static object ParseVector(Domain domain, ParseResult result)
        {
            var values = result.Select(x => Convert.ToSingle(ReadValue(domain, x))).ToArray();

            switch (result.ChildCount) {
                case 2: return new Vector2(values[0], values[1]);
                case 3: return new Vector3(values[0], values[1], values[2]);
                case 4: return new Vector4(values[0], values[1], values[2], values[3]);
                default: throw new Exception("Failed to parse Vector - bad number of arguments");
            }
        }

        private static char ParseChar(ParseResult result)
        {
            if (result.Parser.Name == "Single Character") return result.Value[0];

            switch (result.Value) {
                case "r": return '\r';
                case "n": return '\n';
                case "t": return '\t';
                default: return result.Value[0];
            }
        }

        private static object ReadValue(Domain domain, ParseResult result)
        {
            switch (result.Parser.Name) {
                case "Null": return null;
                case "Boolean": return bool.Parse(result.Value);
                case "Vector": return ParseVector(domain, result[0]);
                case "Float": return float.Parse(result[0].Value);
                case "Double": return double.Parse(result[0].Value);
                case "Integer": return int.Parse(result.Value);
                case "Hex Integer": return uint.Parse(result.Value, NumberStyles.HexNumber);
                case "String": return new string(result.Select(x => ParseChar(x)).ToArray());
                case "Location": return FindMember(domain, result).GetValue();
                default:
                    throw new Exception("This should not occur - parser grammar is incorrect");
            }
        }

        private static Member FindRootMember(Domain domain, ParseResult result)
        {
            switch (result.Parser.Name) {
                case "Networkable Identifier":
                    return new NetworkableId(domain, uint.Parse(result.Value));
                case "Type Name":
                    return new Static(Type.GetType(result.Value));
                default:
                    throw new Exception("This should not occur - parser grammar is incorrect");
            }
        }

        private static Member FindMember(Domain domain, ParseResult result)
        {
            var member = FindRootMember(domain, result[0]);

            for (var i = 1; i < result.ChildCount; ++i) {
                member = member.FindMember(domain, result[i]);
            }

            return member;
        }

        [ConCommand(Domain.Shared, "get")]
        private static String GetValue(ConCommandArgs args)
        {
            if (args.ValueCount != 1) throw new Exception("Expected 1 argument: a member path");

            var match = _sLocationParser.Parse(args.Values[0]);
            if (!match.Success) {
                throw new Exception(match.Error.ToString());
            }

            var member = FindMember(args.Domain, match[0]);
            return (member.GetValue() ?? "null").ToString();
        }

        [ConCommand(Domain.Shared, "set")]
        private static void SetValue(ConCommandArgs args)
        {
            if (args.ValueCount != 2) throw new Exception("Expected 2 arguments: a member path and a value");

            var location = _sLocationParser.Parse(args.Values[0]);
            if (!location.Success) {
                throw new Exception(location.Error.ToString());
            }

            var value = _sValueParser.Parse(args.Values[1]);
            if (!value.Success) {
                throw new Exception(value.Error.ToString());
            }

            var member = FindMember(args.Domain, location[0]);
            member.SetValue(ReadValue(args.Domain, value[0]));
        }
    }
}
