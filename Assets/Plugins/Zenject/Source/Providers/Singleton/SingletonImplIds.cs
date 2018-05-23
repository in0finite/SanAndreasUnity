using ModestTree;
using System;

namespace Zenject
{
    public static class SingletonImplIds
    {
        public class ToMethod
        {
            private readonly Delegate _method;

            public ToMethod(Delegate method)
            {
                _method = method;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + _method.GetHashCode();
                    return hash;
                }
            }

            public override bool Equals(object otherObj)
            {
                var other = otherObj as ToMethod;

                if (other == null)
                {
                    return false;
                }

                return _method.Target == other._method.Target && _method.Method() == other._method.Method();
            }
        }

        public class ToGetter
        {
            private readonly Delegate _method;
            private readonly object _identifier;

            public ToGetter(object identifier, Delegate method)
            {
                _method = method;
                _identifier = identifier;
            }

            public override int GetHashCode()
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = 17;
                    hash = hash * 29 + _method.GetHashCode();
                    hash = hash * 29 + (_identifier == null ? 0 : _identifier.GetHashCode());
                    return hash;
                }
            }

            public override bool Equals(object otherObj)
            {
                var other = otherObj as ToGetter;

                if (other == null)
                {
                    return false;
                }

                return object.Equals(_identifier, other._identifier)
                    && _method.Target == other._method.Target
                    && _method.Method() == other._method.Method();
            }
        }
    }
}