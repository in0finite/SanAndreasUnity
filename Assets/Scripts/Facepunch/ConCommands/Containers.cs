using System;
using System.Collections.Generic;
using Facepunch.Networking;
using JetBrains.Annotations;

namespace Facepunch.ConCommands
{
    [MeansImplicitUse]
    public sealed class ConCommandAttribute : Attribute
    {
        public Domain Domain { get; set; }
        public String[] Prefix { get; set; }
        public String Description { get; set; }

        public ConCommandAttribute(Domain domain, params String[] prefix)
        {
            Domain = domain;
            Prefix = prefix;
            Description = "No description provided";
        }
    }

    public sealed class ConCommandArgs
    {
        public static implicit operator string(ConCommandArgs args)
        {
            return args.ToString();
        }

        public Domain Domain { get; private set; }

        public String[] Prefix { get; private set; }
        public String[] Values { get; private set; }

        public int ValueCount { get { return Values.Length; } }

        private readonly Dictionary<int, Dictionary<Type, Object>> _getCache = new Dictionary<int, Dictionary<Type, Object>>();

        public bool CanGet<TValue>(int index)
            where TValue : IConvertible
        {
            if (!_getCache.ContainsKey(index)) {
                _getCache.Add(index, new Dictionary<Type, object>());
            }

            var type = typeof(TValue);

            if (_getCache[index].ContainsKey(type)) return true;

            try {
                _getCache[index].Add(type, Get<TValue>(index));
                return true;
            } catch {
                return false;
            }
        }

        public TValue Get<TValue>(int index)
            where TValue : IConvertible
        {
            var type = typeof(TValue);

            return !_getCache.ContainsKey(index) || !_getCache[index].ContainsKey(type)
                ? (TValue) Convert.ChangeType(Values[index], type)
                : (TValue) _getCache[index][type];
        }

        public ConCommandArgs(Domain domain, String[] prefix, String[] values)
        {
            Domain = domain;

            Prefix = prefix;
            Values = values;
        }

        public override string ToString()
        {
            return String.Join(" ", Values);
        }
    }

    public sealed class ConCommandResult
    {
        public static implicit operator String(ConCommandResult result)
        {
            return result.Message;
        }

        public bool Success { get; private set; }
        public Exception Exception { get; private set; }
        public String Message { get; private set; }
        public Object Value { get; private set; }

        public ConCommandResult(Exception ex)
        {
            Success = false;
            Exception = ex;

            Value = null;
            Message = ex.Message;
        }

        public ConCommandResult(Object result)
        {
            Success = true;
            Exception = null;

            Value = result;
            Message = result.ToString().TrimEnd();
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
