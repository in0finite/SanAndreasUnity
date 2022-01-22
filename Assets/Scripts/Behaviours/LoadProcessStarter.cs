using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
    public class LoadProcessStarter : MonoBehaviour
    {
        void Start()
        {
            Loader.StartLoading();
        }
    }
}
