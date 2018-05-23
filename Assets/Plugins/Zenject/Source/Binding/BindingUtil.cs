using System;
using System.Collections.Generic;
using ModestTree;
using Zenject.Internal;
using System.Linq;
using TypeExtensions = ModestTree.TypeExtensions;

#if !NOT_UNITY3D

using UnityEngine;

#if UNITY_EDITOR
#endif

#endif

namespace Zenject
{
    internal static class BindingUtil
    {
#if !NOT_UNITY3D

        public static void AssertIsValidPrefab(UnityEngine.Object prefab)
        {
            Assert.That(!ZenUtilInternal.IsNull(prefab), "Received null prefab during bind command");

#if UNITY_EDITOR
            // Unfortunately we can't do this check because asset bundles return PrefabType.None here
            // as discussed here: https://github.com/modesttree/Zenject/issues/269#issuecomment-323419408
            //Assert.That(PrefabUtility.GetPrefabType(prefab) == PrefabType.Prefab,
            //"Expected prefab but found game object with name '{0}' during bind command", prefab.name);
#endif
        }

        public static void AssertIsValidGameObject(GameObject gameObject)
        {
            Assert.That(!ZenUtilInternal.IsNull(gameObject), "Received null game object during bind command");

#if UNITY_EDITOR
            // Unfortunately we can't do this check because asset bundles return PrefabType.None here
            // as discussed here: https://github.com/modesttree/Zenject/issues/269#issuecomment-323419408
            //Assert.That(PrefabUtility.GetPrefabType(gameObject) != PrefabType.Prefab,
            //"Expected game object but found prefab instead with name '{0}' during bind command", gameObject.name);
#endif
        }

        public static void AssertIsNotComponent(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsNotComponent(type);
            }
        }

        public static void AssertIsNotComponent<T>()
        {
            AssertIsNotComponent(typeof(T));
        }

        public static void AssertIsNotComponent(Type type)
        {
            Assert.That(!type.DerivesFrom(typeof(Component)),
                "Invalid type given during bind command.  Expected type '{0}' to NOT derive from UnityEngine.Component", type);
        }

        public static void AssertDerivesFromUnityObject(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertDerivesFromUnityObject(type);
            }
        }

        public static void AssertDerivesFromUnityObject<T>()
        {
            AssertDerivesFromUnityObject(typeof(T));
        }

        public static void AssertDerivesFromUnityObject(Type type)
        {
            Assert.That(type.DerivesFrom<UnityEngine.Object>(),
                "Invalid type given during bind command.  Expected type '{0}' to derive from UnityEngine.Object", type);
        }

        public static void AssertTypesAreNotComponents(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsNotComponent(type);
            }
        }

        public static void AssertIsValidResourcePath(string resourcePath)
        {
            Assert.That(!string.IsNullOrEmpty(resourcePath), "Null or empty resource path provided");

            // We'd like to validate the path here but unfortunately there doesn't appear to be
            // a way to do this besides loading it
        }

        public static void AssertIsInterfaceOrScriptableObject(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsInterfaceOrScriptableObject(type);
            }
        }

        public static void AssertIsInterfaceOrScriptableObject<T>()
        {
            AssertIsInterfaceOrScriptableObject(typeof(T));
        }

        public static void AssertIsInterfaceOrScriptableObject(Type type)
        {
            Assert.That(type.DerivesFrom(typeof(ScriptableObject)) || type.IsInterface(),
                "Invalid type given during bind command.  Expected type '{0}' to either derive from UnityEngine.ScriptableObject or be an interface", type);
        }

        public static void AssertIsInterfaceOrComponent(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsInterfaceOrComponent(type);
            }
        }

        public static void AssertIsInterfaceOrComponent<T>()
        {
            AssertIsInterfaceOrComponent(typeof(T));
        }

        public static void AssertIsInterfaceOrComponent(Type type)
        {
            Assert.That(type.DerivesFrom(typeof(Component)) || type.IsInterface(),
                "Invalid type given during bind command.  Expected type '{0}' to either derive from UnityEngine.Component or be an interface", type);
        }

        public static void AssertIsComponent(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsComponent(type);
            }
        }

        public static void AssertIsComponent<T>()
        {
            AssertIsComponent(typeof(T));
        }

        public static void AssertIsComponent(Type type)
        {
            Assert.That(type.DerivesFrom(typeof(Component)),
                "Invalid type given during bind command.  Expected type '{0}' to derive from UnityEngine.Component", type);
        }

#else
        public static void AssertTypesAreNotComponents(IEnumerable<Type> types)
        {
        }

        public static void AssertIsNotComponent(Type type)
        {
        }

        public static void AssertIsNotComponent<T>()
        {
        }

        public static void AssertIsNotComponent(IEnumerable<Type> types)
        {
        }
#endif

        public static void AssertTypesAreNotAbstract(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsNotAbstract(type);
            }
        }

        public static void AssertIsNotAbstract(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                AssertIsNotAbstract(type);
            }
        }

        public static void AssertIsNotAbstract<T>()
        {
            AssertIsNotAbstract(typeof(T));
        }

        public static void AssertIsNotAbstract(Type type)
        {
            Assert.That(!type.IsAbstract(),
                "Invalid type given during bind command.  Expected type '{0}' to not be abstract.", type);
        }

        public static void AssertIsDerivedFromType(Type concreteType, Type parentType)
        {
#if !(UNITY_WSA && ENABLE_DOTNET)
            // TODO: Is it possible to do this on WSA?

            Assert.That(parentType.IsOpenGenericType() == concreteType.IsOpenGenericType(),
                "Invalid type given during bind command.  Expected type '{0}' and type '{1}' to both either be open generic types or not open generic types", parentType, concreteType);

            if (parentType.IsOpenGenericType())
            {
                Assert.That(concreteType.IsOpenGenericType());
                Assert.That(TypeExtensions.IsAssignableToGenericType(concreteType, parentType),
                    "Invalid type given during bind command.  Expected open generic type '{0}' to derive from open generic type '{1}'", concreteType, parentType);
            }
            else
#endif
            {
                Assert.That(concreteType.DerivesFromOrEqual(parentType),
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'", concreteType, parentType.Name());
            }
        }

        public static void AssertConcreteTypeListIsNotEmpty(IEnumerable<Type> concreteTypes)
        {
            Assert.That(concreteTypes.Count() >= 1,
                "Must supply at least one concrete type to the current binding");
        }

        public static void AssertIsDerivedFromTypes(
            IEnumerable<Type> concreteTypes, IEnumerable<Type> parentTypes, InvalidBindResponses invalidBindResponse)
        {
            if (invalidBindResponse == InvalidBindResponses.Assert)
            {
                AssertIsDerivedFromTypes(concreteTypes, parentTypes);
            }
            else
            {
                Assert.IsEqual(invalidBindResponse, InvalidBindResponses.Skip);
            }
        }

        public static void AssertIsDerivedFromTypes(IEnumerable<Type> concreteTypes, IEnumerable<Type> parentTypes)
        {
            foreach (var concreteType in concreteTypes)
            {
                AssertIsDerivedFromTypes(concreteType, parentTypes);
            }
        }

        public static void AssertIsDerivedFromTypes(Type concreteType, IEnumerable<Type> parentTypes)
        {
            foreach (var parentType in parentTypes)
            {
                AssertIsDerivedFromType(concreteType, parentType);
            }
        }

        public static void AssertInstanceDerivesFromOrEqual(object instance, IEnumerable<Type> parentTypes)
        {
            if (!ZenUtilInternal.IsNull(instance))
            {
                foreach (var baseType in parentTypes)
                {
                    AssertInstanceDerivesFromOrEqual(instance, baseType);
                }
            }
        }

        public static void AssertInstanceDerivesFromOrEqual(object instance, Type baseType)
        {
            if (!ZenUtilInternal.IsNull(instance))
            {
                Assert.That(instance.GetType().DerivesFromOrEqual(baseType),
                    "Invalid type given during bind command.  Expected type '{0}' to derive from type '{1}'", instance.GetType(), baseType.Name());
            }
        }
    }
}