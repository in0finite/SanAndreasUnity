using UnityEngine.SceneManagement;

namespace SanAndreasUnity.Utilities
{
    public class StartupSingleton<T> : SingletonComponent<T>
        where T : StartupSingleton<T>
    {
        protected override void OnSingletonAwakeValidate()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            if (!activeScene.IsValid() || activeScene.buildIndex != 0)
                throw new System.Exception("Startup singleton can only be initialized in startup scene");
        }
    }
}
