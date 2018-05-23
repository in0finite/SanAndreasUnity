using System;
using System.Collections.Generic;

#if !NOT_UNITY3D

using UnityEngine;

#endif

namespace Zenject
{
    public class FactoryFromBinderBase<TContract> : ArgConditionCopyNonLazyBinder
    {
        public FactoryFromBinderBase(
            BindInfo bindInfo, FactoryBindInfo factoryBindInfo)
            : base(bindInfo)
        {
            FactoryBindInfo = factoryBindInfo;

            factoryBindInfo.ProviderFunc =
                (container) => new TransientProvider(ContractType, container, BindInfo.Arguments, null, BindInfo.ContextInfo);
        }

        protected FactoryBindInfo FactoryBindInfo
        {
            get; private set;
        }

        protected Func<DiContainer, IProvider> ProviderFunc
        {
            get { return FactoryBindInfo.ProviderFunc; }
            set { FactoryBindInfo.ProviderFunc = value; }
        }

        protected Type ContractType
        {
            get { return typeof(TContract); }
        }

        public IEnumerable<Type> AllParentTypes
        {
            get
            {
                yield return ContractType;

                foreach (var type in BindInfo.ToTypes)
                {
                    yield return type;
                }
            }
        }

        // Note that this isn't necessary to call since it's the default
        public ConditionCopyNonLazyBinder FromNew()
        {
            BindingUtil.AssertIsNotComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            return this;
        }

        public ConditionCopyNonLazyBinder FromResolve()
        {
            return FromResolve(null);
        }

        public ConditionCopyNonLazyBinder FromResolve(object subIdentifier)
        {
            ProviderFunc =
                (container) => new ResolveProvider(
                    ContractType, container,
                    subIdentifier, false, InjectSources.Any);

            return this;
        }

#if !NOT_UNITY3D

        public ConditionCopyNonLazyBinder FromNewComponentOn(GameObject gameObject)
        {
            BindingUtil.AssertIsValidGameObject(gameObject);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            ProviderFunc =
                (container) => new AddToExistingGameObjectComponentProvider(
                    gameObject, container, ContractType,
                    null, new List<TypeValuePair>());

            return this;
        }

        public ConditionCopyNonLazyBinder FromNewComponentOn(
            Func<InjectContext, GameObject> gameObjectGetter)
        {
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            ProviderFunc =
                (container) => new AddToExistingGameObjectComponentProviderGetter(
                    gameObjectGetter, container, ContractType,
                    null, new List<TypeValuePair>());

            return this;
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewGameObject()
        {
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new AddToNewGameObjectComponentProvider(
                    container, ContractType, null,
                    new List<TypeValuePair>(), gameObjectInfo);

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new InstantiateOnPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProvider(prefab)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromComponentInNewPrefab(UnityEngine.Object prefab)
        {
            BindingUtil.AssertIsValidPrefab(prefab);
            BindingUtil.AssertIsInterfaceOrComponent(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new GetFromPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProvider(prefab)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromComponentInNewPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrComponent(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new GetFromPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProviderResource(resourcePath)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public NameTransformConditionCopyNonLazyBinder FromNewComponentOnNewPrefabResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsComponent(ContractType);
            BindingUtil.AssertIsNotAbstract(ContractType);

            var gameObjectInfo = new GameObjectCreationParameters();

            ProviderFunc =
                (container) => new InstantiateOnPrefabComponentProvider(
                    ContractType,
                    new PrefabInstantiator(
                        container, gameObjectInfo,
                        ContractType, new List<TypeValuePair>(), new PrefabProviderResource(resourcePath)));

            return new NameTransformConditionCopyNonLazyBinder(BindInfo, gameObjectInfo);
        }

        public ConditionCopyNonLazyBinder FromNewScriptableObjectResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(ContractType);

            ProviderFunc =
                (container) => new ScriptableObjectResourceProvider(
                    resourcePath, ContractType, container, null, new List<TypeValuePair>(), true);

            return this;
        }

        public ConditionCopyNonLazyBinder FromScriptableObjectResource(string resourcePath)
        {
            BindingUtil.AssertIsValidResourcePath(resourcePath);
            BindingUtil.AssertIsInterfaceOrScriptableObject(ContractType);

            ProviderFunc =
                (container) => new ScriptableObjectResourceProvider(
                    resourcePath, ContractType, container, null, new List<TypeValuePair>(), false);

            return this;
        }

#endif
    }
}