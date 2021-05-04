using SanAndreasUnity.Importing.RenderWareStream;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class LightSource : MonoBehaviour
    {
        public TwoDEffect.Light LightInfo { get; private set; }

        public static LightSource Create(
            Transform parent,
            TwoDEffect.Light lightInfo)
        {
            return null;
        }
    }
}
