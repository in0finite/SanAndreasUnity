using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    // Note about script execution order: execute this script before others to make sure config is loaded before their Awake() is called.
    public class ConfigManager : MonoBehaviour
    {
        void Awake()
        {
            //Config.Load();
        }
    }
}
