using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

#if !NOT_UNITY3D

using UnityEngine.SceneManagement;
using UnityEngine;

#endif

namespace Zenject.Internal
{
    public class ZenUtilInternal
    {
        // Due to the way that Unity overrides the Equals operator,
        // normal null checks such as (x == null) do not always work as
        // expected
        // In those cases you can use this function which will also
        // work with non-unity objects
        public static bool IsNull(System.Object obj)
        {
            return obj == null || obj.Equals(null);
        }

#if UNITY_EDITOR

        // This can be useful if you are running code outside unity
        // since in that case you have to make sure to avoid calling anything
        // inside Unity DLLs
        public static bool IsOutsideUnity()
        {
            return AppDomain.CurrentDomain.FriendlyName != "Unity Child Domain";
        }

#endif

        public static bool AreFunctionsEqual(Delegate left, Delegate right)
        {
            return left.Target == right.Target && left.Method() == right.Method();
        }

        // Taken from here:
        // http://stackoverflow.com/questions/28937324/in-c-how-could-i-get-a-classs-inheritance-distance-to-base-class/28937542#28937542
        public static int GetInheritanceDelta(Type derived, Type parent)
        {
            Assert.That(derived.DerivesFromOrEqual(parent));

            if (parent.IsInterface())
            {
                // Not sure if we can calculate this so just return 1
                return 1;
            }

            if (derived == parent)
            {
                return 0;
            }

            int distance = 1;

            Type child = derived;

            while ((child = child.BaseType()) != parent)
            {
                distance++;
            }

            return distance;
        }

#if !NOT_UNITY3D

        public static IEnumerable<SceneContext> GetAllSceneContexts()
        {
            foreach (var scene in UnityUtil.AllLoadedScenes)
            {
                var contexts = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<SceneContext>()).ToList();

                if (contexts.IsEmpty())
                {
                    continue;
                }

                Assert.That(contexts.Count == 1,
                    "Found multiple scene contexts in scene '{0}'", scene.name);

                yield return contexts[0];
            }
        }

        public static void GetInjectableMonoBehaviours(
            Scene scene, List<MonoBehaviour> monoBehaviours)
        {
            foreach (var rootObj in GetRootGameObjects(scene))
            {
                if (rootObj != null)
                {
                    GetInjectableMonoBehaviours(rootObj, monoBehaviours);
                }
            }
        }

        // NOTE: This method will not return components that are within a GameObjectContext
        // It returns monobehaviours in a bottom-up order
        public static void GetInjectableMonoBehaviours(
            GameObject gameObject, List<MonoBehaviour> injectableComponents)
        {
            if (gameObject == null)
            {
                return;
            }

            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();

            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var monoBehaviour = monoBehaviours[i];

                // Can be null for broken component references
                if (monoBehaviour != null
                    && monoBehaviour.GetType().DerivesFromOrEqual<GameObjectContext>())
                {
                    // Need to make sure we don't inject on any MonoBehaviour's that are below a GameObjectContext
                    // Since that is the responsibility of the GameObjectContext
                    // BUT we do want to inject on the GameObjectContext itself
                    injectableComponents.Add(monoBehaviour);
                    return;
                }
            }

            // Recurse first so it adds components bottom up
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var child = gameObject.transform.GetChild(i);

                if (child != null)
                {
                    GetInjectableMonoBehaviours(child.gameObject, injectableComponents);
                }
            }

            for (int i = 0; i < monoBehaviours.Length; i++)
            {
                var monoBehaviour = monoBehaviours[i];

                // Can be null for broken component references
                if (monoBehaviour != null
                    && IsInjectableMonoBehaviourType(monoBehaviour.GetType()))
                {
                    injectableComponents.Add(monoBehaviour);
                }
            }
        }

        public static bool IsInjectableMonoBehaviourType(Type type)
        {
            // Do not inject on installers since these are always injected before they are installed
            return type != null && !type.DerivesFrom<MonoInstaller>()
                // Don't bother performing reflection operations on unity classes since they are guaranteed not to use zenject
                && (type.Namespace == null || !type.Namespace.StartsWith("UnityEngine."));
        }

        public static IEnumerable<GameObject> GetRootGameObjects(Scene scene)
        {
            if (scene.isLoaded)
            {
                return scene.GetRootGameObjects()
                    .Where(x => x.GetComponent<ProjectContext>() == null);
            }

            // Note: We can't use scene.GetRootObjects() here because that apparently fails with an exception
            // about the scene not being loaded yet when executed in Awake
            // We also can't use GameObject.FindObjectsOfType<Transform>() because that does not include inactive game objects
            // So we use Resources.FindObjectsOfTypeAll, even though that may include prefabs.  However, our assumption here
            // is that prefabs do not have their "scene" property set correctly so this should work
            //
            // It's important here that we only inject into root objects that are part of our scene, to properly support
            // multi-scene editing features of Unity 5.x
            //
            // Also, even with older Unity versions, if there is an object that is marked with DontDestroyOnLoad, then it will
            // be injected multiple times when another scene is loaded
            //
            // We also make sure not to inject into the project root objects which are injected by ProjectContext.
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(x => x.transform.parent == null
                    && x.GetComponent<ProjectContext>() == null
                    && x.scene == scene);
        }

#endif
    }
}