using ModestTree;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zenject
{
    public class ConcreteBinderNonGeneric : FromBinderNonGeneric
    {
        public ConcreteBinderNonGeneric(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo, finalizerWrapper)
        {
            ToSelf();
        }

        // Note that this is the default, so not necessary to call
        public FromBinderNonGeneric ToSelf()
        {
            Assert.IsEqual(BindInfo.ToChoice, ToChoices.Self);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.FromNew, null,
                (container, type) => new TransientProvider(
                    type, container, BindInfo.Arguments, BindInfo.ConcreteIdentifier, BindInfo.ContextInfo));

            return this;
        }

        public FromBinderNonGeneric To<TConcrete>()
        {
            return To(typeof(TConcrete));
        }

        public FromBinderNonGeneric To(params Type[] concreteTypes)
        {
            return To((IEnumerable<Type>)concreteTypes);
        }

        public FromBinderNonGeneric To(IEnumerable<Type> concreteTypes)
        {
            BindingUtil.AssertIsDerivedFromTypes(concreteTypes, BindInfo.ContractTypes, BindInfo.InvalidBindResponse);

            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes = concreteTypes.ToList();

            return this;
        }

#if !(UNITY_WSA && ENABLE_DOTNET)

        public FromBinderNonGeneric To(
            Action<ConventionSelectTypesBinder> generator)
        {
            var bindInfo = new ConventionBindInfo();

            // This is nice because it allows us to do things like Bind(all interfaces).To(specific types)
            // instead of having to do Bind(all interfaces).To(specific types that inherit from one of these interfaces)
            BindInfo.InvalidBindResponse = InvalidBindResponses.Skip;

            generator(new ConventionSelectTypesBinder(bindInfo));

            BindInfo.ToChoice = ToChoices.Concrete;
            BindInfo.ToTypes = bindInfo.ResolveTypes();

            return this;
        }

#endif
    }
}