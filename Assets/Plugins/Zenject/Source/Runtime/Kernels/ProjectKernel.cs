#if !NOT_UNITY3D

using ModestTree;
using System.Linq;
using UnityEngine;
using Zenject.Internal;

namespace Zenject
{
    public class ProjectKernel : MonoKernel
    {
        // One issue with relying on MonoKernel.OnDestroy to call IDisposable.Dispose
        // is that the order that OnDestroy is called in is difficult to predict
        // One good thing is that it does follow the heirarchy order (so root game objects
        // will have thier OnDestroy called before child objects)
        // However, the order that OnDestroy is called for the root game objects themselves
        // is largely random
        // Within an individual scene, this can be helped somewhat by placing all game objects
        // underneath the SceneContext and then also checking the 'ParentNewObjectsUnderRoot'
        // property to ensure any new game objects will also be parented underneath SceneContext
        // By doing this, we can be guaranteed to have any bound IDisposable's have their
        // Dispose called before any game object is destroyed in the scene
        // However, when using multiple scenes (each with their own SceneContext) the order
        // that these SceneContext game objects are destroyed is random
        // So to address that, we explicitly call GameObject.DestroyImmediate for all
        // SceneContext's in the reverse order that the scenes were loaded in below
        // (this works because OnApplicationQuit is always called before OnDestroy)
        // Note that this only works when stopping the app and not when changing scenes
        // When changing scenes, if you have multiple scenes loaded at once, you will have to
        // manually unload the scenes in the reverse order they were loaded before going to
        // the new scene, if you require a predictable destruction order.  Or you can always use
        // ZenjectSceneLoader which will do this for you
        public void OnApplicationQuit()
        {
            ForceUnloadAllScenes(true);

            Assert.That(!IsDestroyed);
            GameObject.DestroyImmediate(this.gameObject);
            Assert.That(IsDestroyed);
        }

        public void ForceUnloadAllScenes(bool immediate = false)
        {
            // OnApplicationQuit should always be called before OnDestroy
            // (Unless it is destroyed manually)
            Assert.That(!IsDestroyed);

            // Destroy the scene contexts from bottom to top
            // Since this is the reverse order that they were loaded in
            foreach (var sceneContext in ZenUtilInternal.GetAllSceneContexts().Reverse().ToList())
            {
                if (immediate)
                {
                    GameObject.DestroyImmediate(sceneContext.gameObject);
                }
                else
                {
                    GameObject.Destroy(sceneContext.gameObject);
                }
            }
        }
    }
}

#endif