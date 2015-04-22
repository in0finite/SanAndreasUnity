using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Arcade.Networking;
using Facepunch.Networking;
using UnityEngine;

namespace Facepunch.ConCommands
{
    using RegexGroup = System.Text.RegularExpressions.Group;
    using UnityComponent = Component;

    public static class ReflectionCommands
    {
        private const string NamePattern = "[A-Za-z_][A-Za-z0-9_]*";

        private static readonly Regex _sNetworkableRegex = new Regex(@"^\[\s*(?<id>[0-9]+)\s*\]", RegexOptions.Compiled);
        private static readonly Regex _sStaticRegex = new Regex(@"^\[(?<type>[^\]]+)\]", RegexOptions.Compiled);

        private static readonly Regex _sMemberRegex = new Regex(string.Format(@"\s*\.\s*(?<name>{0})", NamePattern), RegexOptions.Compiled);

        private static readonly Regex _sComponentRegex = new Regex(string.Format(@"\s*<(?<component>[^>]+)>"), RegexOptions.Compiled);

        private const string VectorNumberPattern = @"(\s*(?<number>-?[0-9]+(\.[0-9]+)?)f?\s*,?){2,4}";

        private const string ValuePattern = @"
\s*(
    (?<null>null)|
    (?<bool>[tT]rue|[fF]alse)|
    (\{(?<vector>" + VectorNumberPattern + @")\})|
    (--)*(
        (?<float>-?[0-9]+(\.[0-9]+)?)f|
        (?<double>-?[0-9]+(\.[0-9]+d?|d))
    )|
    0x(?<hex>[0-9a-fA-F]+)|
    (?<int>-?[0-9]+)|(\(\))|
    (""(?<string>(\\\\|\\""|[^""])*)"")|
    ('(?<string>(\\\\|\\'|[^""])*)')
)\s*
            ";

        private static readonly Regex _sValueRegex = new Regex(ValuePattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
        private static readonly Regex _sParametersRegex = new Regex(string.Format(@"\s*\(((?<value>{0})(,(?<value>{0}))*)?\)", ValuePattern), RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        private static readonly Regex _sVectorRegex = new Regex(VectorNumberPattern, RegexOptions.Compiled);

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

            public Member FindMember(string path, ref int startAt)
            {
                var context = GetValue();
                var type = GetContextType(context);

                Match match;
                if (TryMatch(_sMemberRegex, path, ref startAt, out match)) {
                    var name = match.Groups["name"].Value;

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
                }

                if (TryMatch(_sComponentRegex, path, ref startAt, out match)) {
                    var componentName = match.Groups["component"].Value;
                    return new Component(componentName, context);
                }

                if (TryMatch(_sParametersRegex, path, ref startAt, out match)) {
// ReSharper disable once ConvertClosureToMethodGroup
                    var values = match.Groups["value"].Captures.Cast<Capture>().Select(x => ReadValue(x.Value, false)).ToArray();
                    return new Invocation(values, this);
                }

                throw new Exception(string.Format("Expected '.Member' or '<Component>' at character {0} in path", startAt + 1));
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
                        nw = ArcadeClient.Instance.GetNetworkable(_ident);
                        break;
                    case Domain.Server:
                        nw = ArcadeServer.Instance.GetNetworkable(_ident);
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

        private static bool TryMatch(Regex regex, string value, ref int startAt, out Match match)
        {
            match = regex.Match(value, startAt);
            if (match.Success && match.Index == startAt) {
                startAt += match.Length;
                return true;
            }

            match = null;
            return false;
        }

        private static bool TryGroup<TVal>(RegexGroup group, Func<string, TVal> parser, ref object val)
        {
            if (!group.Success) return false;
            val = parser(group.Value);
            return true;
        }

        private static object ParseVector(string str)
        {
            var match = _sVectorRegex.Match(str);
            if (!match.Success) throw new Exception("Failed to parse Vector - this shouldn't happen");

            var numbers = match.Groups["number"].Captures.Cast<Capture>().Select(x => float.Parse(x.Value)).ToArray();

            switch (numbers.Length) {
                case 2:
                    return new Vector2(numbers[0], numbers[1]);
                case 3:
                    return new Vector3(numbers[0], numbers[1], numbers[2]);
                case 4:
                    return new Vector4(numbers[0], numbers[1], numbers[2], numbers[3]);
                default:
                    throw new Exception("Failed to parse Vector - this shouldn't happen");
            }
        }

        private static object ReadValue(string str, bool defaultToString)
        {
            var match = _sValueRegex.Match(str);

            object val = null;

// ReSharper disable RedundantTypeArgumentsOfMethod
            if (TryGroup<object>(match.Groups["vector"], ParseVector, ref val) ||
                TryGroup<bool>(match.Groups["bool"], bool.Parse, ref val) ||
                TryGroup<float>(match.Groups["float"], float.Parse, ref val) ||
                TryGroup<double>(match.Groups["double"], double.Parse, ref val) ||
                TryGroup<int>(match.Groups["hex"], x => int.Parse(x, NumberStyles.HexNumber), ref val) ||
                TryGroup<int>(match.Groups["int"], int.Parse, ref val) ||
                TryGroup<string>(match.Groups["string"], x => x.Replace("\\\\", "\\"), ref val) ||
                TryGroup<object>(match.Groups["null"], x => null, ref val)) {
// ReSharper restore RedundantTypeArgumentsOfMethod
                return val;
            }

            if (defaultToString) {
                return str;
            }

            throw new Exception(string.Format("Failed to parse value '{0}'", str));
        }

        private static Member FindRootMember(Domain domain, string path, ref int startAt)
        {
            Match match;

            if (TryMatch(_sNetworkableRegex, path, ref startAt, out match)) {
                return new NetworkableId(domain, uint.Parse(match.Groups["id"].Value));
            }

            if (TryMatch(_sStaticRegex, path, ref startAt, out match)) {
                return new Static(Type.GetType(match.Groups["type"].Value));
            }

            throw new Exception("Invalid member path root, expected '[networkable id]' or '[type name]'");
        }

        private static Member FindMember(Domain domain, string path)
        {
            var startAt = 0;
            var member = FindRootMember(domain, path, ref startAt);

            while (startAt < path.Length) {
                member = member.FindMember(path, ref startAt);
            }

            return member;
        }

        [ConCommand(Domain.Shared, "get")]
        private static String GetValue(ConCommandArgs args)
        {
            if (args.ValueCount != 1) throw new Exception("Expected 1 argument: a member path");

            var member = FindMember(args.Domain, args.Values[0]);
            return (member.GetValue() ?? "null").ToString();
        }

        [ConCommand(Domain.Shared, "set")]
        private static void SetValue(ConCommandArgs args)
        {
            if (args.ValueCount != 2) throw new Exception("Expected 2 arguments: a member path and a value");

            var member = FindMember(args.Domain, args.Values[0]);
            member.SetValue(ReadValue(args.Values[1], true));
        }
    }
}
