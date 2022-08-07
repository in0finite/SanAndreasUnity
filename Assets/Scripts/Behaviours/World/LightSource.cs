using SanAndreasUnity.Importing.RenderWareStream;
using SanAndreasUnity.Utilities;
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
            var go = Instantiate(Cell.Instance.lightSourcePrefab, parent);
            go.transform.localPosition = lightInfo.Position;
            go.transform.localScale = Cell.Instance.lightScaleMultiplier * lightInfo.CoronaSize * Vector3.one;

            var lightSource = go.GetComponentOrThrow<LightSource>();
            lightSource.LightInfo = lightInfo;

            var spriteRenderer = go.GetComponentOrThrow<SpriteRenderer>();
            spriteRenderer.color = lightInfo.Color;

            return lightSource;
        }
    }
}
