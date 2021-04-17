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
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(parent);
            go.transform.localPosition = lightInfo.Position;

            var lightSource = go.AddComponent<LightSource>();
            lightSource.LightInfo = lightInfo;

            return lightSource;
        }
    }
}
