using System;

#if !NOT_UNITY3D

using JetBrains.Annotations;

#endif

namespace Zenject
{
    [AttributeUsage(AttributeTargets.Parameter
        | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
#if !NOT_UNITY3D
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
#endif
    public class InjectOptionalAttribute : InjectAttributeBase
    {
        public InjectOptionalAttribute()
        {
            Optional = true;
        }
    }
}