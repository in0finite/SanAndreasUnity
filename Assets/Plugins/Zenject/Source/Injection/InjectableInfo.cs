using System;

namespace Zenject
{
    // An injectable is a field or property with [Inject] attribute
    // Or a constructor parameter
    public class InjectableInfo
    {
        public readonly bool Optional;
        public readonly object Identifier;

        public readonly InjectSources SourceType;

        // The field name or property name from source code
        public readonly string MemberName;

        // The field type or property type from source code
        public readonly Type MemberType;

        public readonly Type ObjectType;

        // Null for constructor declared dependencies
        public readonly Action<object, object> Setter;

        public readonly object DefaultValue;

        public InjectableInfo(
            bool optional, object identifier, string memberName,
            Type memberType, Type objectType, Action<object, object> setter, object defaultValue, InjectSources sourceType)
        {
            Optional = optional;
            Setter = setter;
            ObjectType = objectType;
            MemberType = memberType;
            MemberName = memberName;
            Identifier = identifier;
            DefaultValue = defaultValue;
            SourceType = sourceType;
        }

        public InjectContext CreateInjectContext(
            DiContainer container, InjectContext currentContext, object targetInstance, object concreteIdentifier)
        {
            var context = new InjectContext();

            context.MemberType = MemberType;
            context.Container = container;
            context.ObjectType = ObjectType;
            context.ParentContext = currentContext;
            context.ObjectInstance = targetInstance;
            context.Identifier = Identifier;
            context.ConcreteIdentifier = concreteIdentifier;
            context.MemberName = MemberName;
            context.Optional = Optional;
            context.SourceType = SourceType;
            context.FallBackValue = DefaultValue;

            return context;
        }
    }
}