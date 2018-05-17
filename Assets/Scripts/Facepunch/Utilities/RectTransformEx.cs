using System.Linq;
using UnityEngine.UI;

namespace UnityEngine
{
    public static class RectTransformEx
    {
        private static readonly Vector3[] _sCorners = new Vector3[4];

        public static Vector2 GetWorldSize(this RectTransform trans)
        {
            trans.GetWorldCorners(_sCorners);
            return new Vector2(_sCorners.Max(x => x.x) - _sCorners.Min(x => x.x), _sCorners.Max(x => x.y) - _sCorners.Min(x => x.y));
        }

        public static float GetPreferredTextHeight(this RectTransform trans, Text text)
        {
            if (text == null) return 0;

            var settings = text.GetGenerationSettings(new Vector2(trans.rect.size.x, 0.0f));

            return text.cachedTextGeneratorForLayout.GetPreferredHeight(text.text, settings) / text.pixelsPerUnit;
        }
    }
}