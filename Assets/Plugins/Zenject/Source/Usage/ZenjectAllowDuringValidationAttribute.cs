using System;

namespace Zenject
{
    // Add this to the classes that you want to allow being created during validation
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class ZenjectAllowDuringValidationAttribute : Attribute
    {
    }
}