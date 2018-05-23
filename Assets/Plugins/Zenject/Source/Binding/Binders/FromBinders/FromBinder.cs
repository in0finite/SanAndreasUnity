using System;
using System.Collections.Generic;
using ModestTree;
using System.Linq;

#if !NOT_UNITY3D

using UnityEngine;

#endif

namespace Zenject
{
    public abstract class FromBinder : ScopeArgConditionCopyNonLazyBinder
    {
        public FromBinder(
            BindInfo bindInfo,
            BindFinalizerWrapper finalizerWrapper)
            : base(bindInfo)
        {
            FinalizerWrapper = finalizerWrapper;
        }

        protected BindFinalizerWrapper FinalizerWrapper
        {
            get;
            private set;
        }

        protected IBindingFinalizer SubFinalizer
        {
            set { FinalizerWrapper.SubFinalizer = value; }
        }

        protected IEnumerable<Type> AllParentTypes
        {
            get { return BindInfo.ContractTypes.Concat(BindInfo.ToTypes); }
        }

        protected IEnumerable<Type> ConcreteTypes
        {
            get
            {
                if (BindInfo.ToChoice == ToChoices.Self)
                {
                    return BindInfo.ContractTypes;
                }

                Assert.IsNotEmpty(BindInfo.ToTypes);
                return BindInfo.ToTypes;
            }
        }

        // This is the default if nothing else is called
        public ScopeArgConditionCopyNonLazyBinder FromNew()
        {
            BindingUtil.AssertTypesAreNotComponents(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            return this;
        }

        public ScopeConditionCopyNonLazyBinder FromResolve()
        {
            return FromResolve(null);
        }

        public ScopeConditionCopyNonLazyBinder FromResolve(object subIdentifier)
        {
            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromResolve, subIdentifier,
                (container, type) => new ResolveProvider(
                    type, container, subIdentifier, false, InjectSources.Any));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

        public SubContainerBinder FromSubContainerResolve()
        {
            return FromSubContainerResolve(null);
        }

        public SubContainerBinder FromSubContainerResolve(object subIdentifier)
        {
            // It's unlikely they will want to create the whole subcontainer with each binding
            // (aka transient) which is the default so require that they specify it
            BindInfo.RequireExplicitScope = true;

            return new SubContainerBinder(
                BindInfo, FinalizerWrapper, subIdentifier);
        }

        public ScopeArgConditionCopyNonLazyBinder FromFactory(Type factoryType)
        {
            Assert.That(factoryType.DerivesFrom<IFactory>());

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromFactory, factoryType,
                (container, type) => new UntypedFactoryProvider(
                    factoryType, container, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

#if !NOT_UNITY3D

        public ScopeArgConditionCopyNonLazyBinder FromNewComponentOn(GameObject gameObject)
        {
            BindingUtil.AssertIsValidGameObject(gameObject);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.FromComponentGameObject, gameObject,
                (container, type) => new AddToExistingGameObjectComponentProvider(
                    gameObject, container, type, BindInfo.ConcreteIdentifier, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ScopeArgConditionCopyNonLazyBinder FromNewComponentOn(Func<InjectContext, GameObject> gameObjectGetter)
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.FromComponentGameObject, gameObjectGetter,
                (container, type) => new AddToExistingGameObjectComponentProviderGetter(
                    gameObjectGetter, container, type, BindInfo.ConcreteIdentifier, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ArgConditionCopyNonLazyBinder FromNewComponentSibling()
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new SingleProviderBindingFinalizer(
                BindInfo, (container, type) => new AddToCurrentGameObjectComponentProvider(
                    container, type, BindInfo.ConcreteIdentifier, BindInfo.Arguments));

            return new ArgConditionCopyNonLazyBinder(BindInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewGameObject()
        {
            return FromNewComponentOnNewGameObject(new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewGameObject(
            GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.FromGameObject, gameObjectInfo,
                (container, type) => new AddToNewGameObjectComponentProvider(
                    container,
                    type,
                    BindInfo.ConcreteIdentifier,
                    BindInfo.Arguments,
                    gameObjectInfo));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(string resourcePath)
        {
            return FromNewComponentOnNewPrefabResource(resourcePath, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(
            string resourcePath, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabResourceBindingFinalizer(
                BindInfo, gameObjectInfo, resourcePath,
                (contractType, instantiator) => new InstantiateOnPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(UnityEngine.Object prefab)
        {
            return FromNewComponentOnNewPrefab(prefab, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsComponent(ConcreteTypes);
            BindingUtil.AssertTypesAreNotAbstract(ConcreteTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabBindingFinalizer(
                BindInfo, gameObjectInfo, prefab,
                (contractType, instantiator) => new InstantiateOnPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefab(UnityEngine.Object prefab)
        {
            return FromComponentInNewPrefab(
                prefab, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefab(
            UnityEngine.Object prefab, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabBindingFinalizer(
                BindInfo, gameObjectInfo, prefab,
                (contractType, instantiator) => new GetFromPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefabResource(string resourcePath)
        {
            return FromComponentInNewPrefabResource(resourcePath, new GameObjectCreationParameters());
        }

        internal NameTransformScopeArgConditionCopyNonLazyBinder FromComponentInNewPrefabResource(
            string resourcePath, GameObjectCreationParameters gameObjectInfo)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrComponent(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new PrefabResourceBindingFinalizer(
                BindInfo, gameObjectInfo, resourcePath,
                (contractType, instantiator) => new GetFromPrefabComponentProvider(contractType, instantiator));

            return new NameTransformScopeArgConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public ScopeArgConditionCopyNonLazyBinder FromNewScriptableObjectResource(string resourcePath)
        {
            return FromScriptableObjectResourceInternal(resourcePath, true);
        }

        public ScopeArgConditionCopyNonLazyBinder FromScriptableObjectResource(string resourcePath)
        {
            return FromScriptableObjectResourceInternal(resourcePath, false);
        }

        private ScopeArgConditionCopyNonLazyBinder FromScriptableObjectResourceInternal(
            string resourcePath, bool createNew)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(AllParentTypes);

            BindInfo.RequireExplicitScope = true;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                createNew ? SingletonTypes.FromNewScriptableObjectResource : SingletonTypes.FromScriptableObjectResource,
                resourcePath.ToLower(),
                (container, type) => new ScriptableObjectResourceProvider(
                    resourcePath, type, container, BindInfo.ConcreteIdentifier, BindInfo.Arguments, createNew));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        public ScopeConditionCopyNonLazyBinder FromResource(string resourcePath)
        {
            BindingUtil.AssertDerivesFromUnityObject(ConcreteTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromResource,
                resourcePath.ToLower(),
                (_, type) => new ResourceProvider(resourcePath, type));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

#endif

        public ScopeArgConditionCopyNonLazyBinder FromMethodUntyped(Func<InjectContext, object> method)
        {
            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromMethod, new SingletonImplIds.ToMethod(method),
                (container, type) => new MethodProviderUntyped(method, container));

            return this;
        }

        protected ScopeArgConditionCopyNonLazyBinder FromMethodBase<TConcrete>(Func<InjectContext, TConcrete> method)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TConcrete), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromMethod, new SingletonImplIds.ToMethod(method),
                (container, type) => new MethodProvider<TConcrete>(method, container));

            return this;
        }

        protected ScopeArgConditionCopyNonLazyBinder FromMethodMultipleBase<TConcrete>(Func<InjectContext, IEnumerable<TConcrete>> method)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TConcrete), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromMethod, new SingletonImplIds.ToMethod(method),
                (container, type) => new MethodProviderMultiple<TConcrete>(method, container));

            return this;
        }

        protected ScopeArgConditionCopyNonLazyBinder FromFactoryBase<TConcrete, TFactory>()
            where TFactory : IFactory<TConcrete>
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TConcrete), AllParentTypes);

            // This is kind of like a look up method like FromMethod so don't enforce specifying scope
            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromFactory, typeof(TFactory),
                (container, type) => new FactoryProvider<TConcrete, TFactory>(container, BindInfo.Arguments));

            return new ScopeArgConditionCopyNonLazyBinder(BindInfo);
        }

        protected ScopeConditionCopyNonLazyBinder FromResolveGetterBase<TObj, TResult>(
            object identifier, Func<TObj, TResult> method)
        {
            BindingUtil.AssertIsDerivedFromTypes(typeof(TResult), AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo,
                SingletonTypes.FromGetter,
                new SingletonImplIds.ToGetter(identifier, method),
                (container, type) => new GetterProvider<TObj, TResult>(identifier, method, container));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }

        protected ScopeConditionCopyNonLazyBinder FromInstanceBase(object instance)
        {
            BindingUtil.AssertInstanceDerivesFromOrEqual(instance, AllParentTypes);

            BindInfo.RequireExplicitScope = false;
            SubFinalizer = new ScopableBindingFinalizer(
                BindInfo, SingletonTypes.FromInstance, instance,
                (container, type) => new InstanceProvider(type, instance, container));

            return new ScopeConditionCopyNonLazyBinder(BindInfo);
        }
    }
}