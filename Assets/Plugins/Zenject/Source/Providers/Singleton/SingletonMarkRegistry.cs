using ModestTree;
using System;
using System.Collections.Generic;

namespace Zenject
{
    public class SingletonMarkRegistry
    {
        private readonly Dictionary<SingletonId, SingletonTypes> _singletonTypes = new Dictionary<SingletonId, SingletonTypes>();

        public SingletonTypes? TryGetSingletonType<T>()
        {
            return TryGetSingletonType(typeof(T));
        }

        public SingletonTypes? TryGetSingletonType(Type type)
        {
            return TryGetSingletonType(type, null);
        }

        public SingletonTypes? TryGetSingletonType(Type type, object concreteIdentifier)
        {
            return TryGetSingletonType(new SingletonId(type, concreteIdentifier));
        }

        public SingletonTypes? TryGetSingletonType(SingletonId id)
        {
            SingletonTypes type;

            if (_singletonTypes.TryGetValue(id, out type))
            {
                return type;
            }

            return null;
        }

        public void MarkSingleton(
            Type type, object concreteIdentifier, SingletonTypes singletonType)
        {
            MarkSingleton(new SingletonId(type, concreteIdentifier), singletonType);
        }

        public void MarkSingleton(SingletonId id, SingletonTypes type)
        {
            SingletonTypes existingType;

            if (_singletonTypes.TryGetValue(id, out existingType))
            {
                if (existingType != type)
                {
                    throw Assert.CreateException(
                        "Cannot use both '{0}' and '{1}' for the same type/concreteIdentifier!", existingType, type);
                }
            }
            else
            {
                _singletonTypes.Add(id, type);
            }
        }
    }
}