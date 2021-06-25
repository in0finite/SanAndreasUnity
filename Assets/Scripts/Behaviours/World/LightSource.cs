using System;
using System.Collections;
using SanAndreasUnity.Importing.RenderWareStream;
using SanAndreasUnity.Utilities;
using UnityEngine;
using TextureDictionary = SanAndreasUnity.Importing.Conversion.TextureDictionary;

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
            go.transform.localScale = Vector3.one * lightInfo.CoronaSize * Cell.Instance.lightScaleMultiplier;

            var lightSource = go.GetComponentOrThrow<LightSource>();
            lightSource.LightInfo = lightInfo;

            var spriteRenderer = go.GetComponentOrThrow<SpriteRenderer>();
            // var texture = TextureDictionary.Load("particle")
            //     .GetDiffuse(lightInfo.CoronaTexName)
            //     .Texture;
            // var sprite = Sprite.Create(
            //     texture,
            //     new Rect(0, 0, texture.width, texture.height),
            //     new Vector2(0.5f, 0.5f));
            // spriteRenderer.sprite = sprite;
            spriteRenderer.color = lightInfo.Color;

            return lightSource;
        }
    }
}
