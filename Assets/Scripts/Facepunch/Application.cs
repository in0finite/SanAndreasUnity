using Facepunch.Networking;
using UnityEngine;

namespace Facepunch
{
    public static class Application
    {
        public const int DefaultPort = 14242;
        public const int DefaultRconPort = DefaultPort;

        public static bool IsQuitting { get; private set; }

        public static void Quit()
        {
            IsQuitting = true;

            if (Client.Instance != null && Client.Instance.NetStatus == NetStatus.Running) {
                Client.Instance.Net.Shutdown();
            }

            if (Server.Instance != null && Server.Instance.NetStatus == NetStatus.Running) {
                Server.Instance.Net.Shutdown();
            }

#if UNITY_EDITOR
            // We pause here since "isPlaying = false" doesn't seem to always work during scene start
            UnityEditor.EditorApplication.isPaused = true;
#else
            UnityEngine.Application.Quit();
#endif
        }

        public static string InstallPath
        {
            get
            {
                // The check for !UNITY_EDITOR is required as of b13
#if !UNITY_EDITOR && UNITY_STANDALONE_OSX
                return UnityEngine.Application.dataPath + "/../..";
#else
                return UnityEngine.Application.dataPath + "/..";
#endif
            }
        }

        public static string DataPath
        {
            get
            {
                return UnityEngine.Application.dataPath;
            }
        }

        public static bool CursorLocked
        {
            get
            {
                return Cursor.lockState == CursorLockMode.Locked;
            }

            set
            {
                Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !value;
            }
        }
    }
}
