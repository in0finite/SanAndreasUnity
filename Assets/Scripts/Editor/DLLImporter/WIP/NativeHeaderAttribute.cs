/*using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.ReturnValue | AttributeTargets.Parameter, AllowMultiple = true)]
[VisibleToOtherModules]
internal class NativeHeaderAttribute : Attribute, IBindingsHeaderProviderAttribute
{
    public string Header { get; set; }

    public NativeHeaderAttribute()
    {
    }

    public NativeHeaderAttribute(string header)
    {
        if (header == null) throw new ArgumentNullException("header");
        if (header == "") throw new ArgumentException("header cannot be empty", "header");

        Header = header;
    }
}*/