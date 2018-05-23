using System;
using System.Collections.Generic;

namespace Zenject
{
    // The given InjectContext values here should always be non-null
    public interface IProvider
    {
        Type GetInstanceType(InjectContext context);

        // This returns an IEnumerable so that we can support circular references
        // The first yield statement from every implementation of this method
        // should return the actual instance
        // And then after that, the rest of the method should handle the actual injection
        // This way, providers that call CreateInstance() can store the instance immediately,
        // and then return that if something gets created during injection that refers back
        // to the newly created instance
        IEnumerator<List<object>> GetAllInstancesWithInjectSplit(
            InjectContext context, List<TypeValuePair> args);
    }
}