using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zenject
{
    public class InjectContext
    {
        public InjectContext()
        {
            SourceType = InjectSources.Any;
            MemberName = "";
        }

        public InjectContext(DiContainer container, Type memberType)
            : this()
        {
            Container = container;
            MemberType = memberType;
        }

        public InjectContext(DiContainer container, Type memberType, object identifier)
            : this(container, memberType)
        {
            Identifier = identifier;
        }

        public InjectContext(DiContainer container, Type memberType, object identifier, bool optional)
            : this(container, memberType, identifier)
        {
            Optional = optional;
        }

        // The type of the object which is having its members injected
        // NOTE: This is null for root calls to Resolve<> or Instantiate<>
        public Type ObjectType
        {
            get;
            set;
        }

        // Parent context that triggered the creation of ObjectType
        // This can be used for very complex conditions using parent info such as identifiers, types, etc.
        // Note that ParentContext.MemberType is not necessarily the same as ObjectType,
        // since the ObjectType could be a derived type from ParentContext.MemberType
        public InjectContext ParentContext
        {
            get;
            set;
        }

        // The instance which is having its members injected
        // Note that this is null when injecting into the constructor
        public object ObjectInstance
        {
            get;
            set;
        }

        // Identifier - most of the time this is null
        // It will match 'foo' in this example:
        //      ... In an installer somewhere:
        //          Container.Bind<Foo>("foo").AsSingle();
        //      ...
        //      ... In a constructor:
        //          public Foo([Inject(Id = "foo") Foo foo)
        public object Identifier
        {
            get;
            set;
        }

        // ConcreteIdentifier - most of the time this is null
        // It will match 'foo' in this example:
        //      ... In an installer somewhere:
        //          Container.Bind<Foo>().ToSingle("foo");
        //          Container.Bind<ITickable>().ToSingle<Foo>("foo");
        //      ...
        // This allows you to create When() conditionals like this:
        //      ...
        //          Container.BindInstance("some text").When(c => c.ConcreteIdentifier == "foo");
        public object ConcreteIdentifier
        {
            get;
            set;
        }

        // The constructor parameter name, or field name, or property name
        public string MemberName
        {
            get;
            set;
        }

        // The type of the constructor parameter, field or property
        public Type MemberType
        {
            get;
            set;
        }

        // When optional, null is a valid value to be returned
        public bool Optional
        {
            get;
            set;
        }

        // When set to true, this will only look up dependencies in the local container and will not
        // search in parent containers
        public InjectSources SourceType
        {
            get;
            set;
        }

        // When optional, this is used to provide the value
        public object FallBackValue
        {
            get;
            set;
        }

        // The container used for this injection
        public DiContainer Container
        {
            get;
            set;
        }

        public IEnumerable<InjectContext> ParentContexts
        {
            get
            {
                if (ParentContext == null)
                {
                    yield break;
                }

                yield return ParentContext;

                foreach (var context in ParentContext.ParentContexts)
                {
                    yield return context;
                }
            }
        }

        public IEnumerable<InjectContext> ParentContextsAndSelf
        {
            get
            {
                yield return this;

                foreach (var context in ParentContexts)
                {
                    yield return context;
                }
            }
        }

        // This will return the types of all the objects that are being injected
        // So if you have class Foo which has constructor parameter of type IBar,
        // and IBar resolves to Bar, this will be equal to (Bar, Foo)
        public IEnumerable<Type> AllObjectTypes
        {
            get
            {
                foreach (var context in ParentContextsAndSelf)
                {
                    if (context.ObjectType != null)
                    {
                        yield return context.ObjectType;
                    }
                }
            }
        }

        public BindingId GetBindingId()
        {
            return new BindingId(MemberType, Identifier);
        }

        public InjectContext CreateSubContext(Type memberType)
        {
            return CreateSubContext(memberType, null);
        }

        public InjectContext CreateSubContext(Type memberType, object identifier)
        {
            var subContext = new InjectContext();

            subContext.ParentContext = this;
            subContext.Identifier = identifier;
            subContext.MemberType = memberType;

            // Clear these
            subContext.ConcreteIdentifier = null;
            subContext.MemberName = "";
            subContext.FallBackValue = null;

            // Inherit these ones by default
            subContext.ObjectType = this.ObjectType;
            subContext.ObjectInstance = this.ObjectInstance;
            subContext.Optional = this.Optional;
            subContext.SourceType = this.SourceType;
            subContext.Container = this.Container;

            return subContext;
        }

        public InjectContext Clone()
        {
            var clone = new InjectContext();

            clone.ObjectType = this.ObjectType;
            clone.ParentContext = this.ParentContext;
            clone.ObjectInstance = this.ObjectInstance;
            clone.Identifier = this.Identifier;
            clone.ConcreteIdentifier = this.ConcreteIdentifier;
            clone.MemberType = this.MemberType;
            clone.MemberName = this.MemberName;
            clone.Optional = this.Optional;
            clone.SourceType = this.SourceType;
            clone.FallBackValue = this.FallBackValue;
            clone.Container = this.Container;

            return clone;
        }

        // This is very useful to print out for debugging purposes
        public string GetObjectGraphString()
        {
            var result = new StringBuilder();

            foreach (var context in ParentContextsAndSelf.Reverse())
            {
                if (context.ObjectType == null)
                {
                    continue;
                }

                result.AppendLine(context.ObjectType.Name());
            }

            return result.ToString();
        }
    }
}