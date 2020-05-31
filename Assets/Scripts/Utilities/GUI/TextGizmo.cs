using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class TextGizmo
    {
        private static TextGizmo tg = null;
        private Dictionary<char, string> texturePathLookup;
        private Camera editorCamera = null;
        private const int CHAR_TEXTURE_HEIGHT = 8; // todo: line breaks
        private const int CHAR_TEXTURE_WIDTH = 6;
        private const string characters = "abcdefghijklmnopqrstuvwxyz0123456789";

        public static void Init()
        {
            tg = new TextGizmo();
        }

        /* singleton constructor */

        private TextGizmo()
        {
            editorCamera = Camera.current;
            texturePathLookup = new Dictionary<char, string>();
            for (int c = 0; c < characters.Length; c++)
            {
                texturePathLookup.Add(characters[c], "TextGizmo/text_" + characters[c] + ".tif");
            }
        }

        /* only call this method from a OnGizmos() method */

        public static void Draw(Vector3 position, string text)
        {
            if (tg == null) Init();

            string lowerText = text.ToLower();
            Vector3 screenPoint = tg.editorCamera.WorldToScreenPoint(position);
            int offset = 20;
            for (int c = 0; c < lowerText.Length; c++)
            {
                if (tg.texturePathLookup.ContainsKey(lowerText[c]))
                {
                    Vector3 worldPoint = tg.editorCamera.ScreenToWorldPoint(new Vector3(screenPoint.x + offset, screenPoint.y, screenPoint.z));

                    Gizmos.DrawIcon(worldPoint, tg.texturePathLookup[lowerText[c]]);

                    offset += CHAR_TEXTURE_WIDTH;
                }
            }
        }
    }
}