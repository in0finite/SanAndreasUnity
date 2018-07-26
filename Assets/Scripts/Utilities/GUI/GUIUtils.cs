using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public enum ScreenCorner { TopRight, TopLeft, BottomRight, BottomLeft }

    public static class GUIUtils
    {

		private	static	GUIStyle	styleWithBackground = new GUIStyle ();



        public static Rect GetCornerRect(ScreenCorner corner, Vector2 size, Vector2? padding = null)
        {
            return GetCornerRect(corner, size.x, size.y, padding);
        }

        public static Rect GetCornerRect(ScreenCorner corner, float width, float height, Vector2? padding = null)
        {
            float padX = 0,
                  padY = 0;

            if (padding != null)
            {
                padX = padding.Value.x;
                padY = padding.Value.y;
            }

            switch (corner)
            {
                case ScreenCorner.TopLeft:
                    return new Rect(padX, padY, width, height);

                case ScreenCorner.TopRight:
                    return new Rect(Screen.width - (width + padX), padY, width, height);

                case ScreenCorner.BottomLeft:
                    return new Rect(padX, Screen.height - (height + padY), width, height);

                case ScreenCorner.BottomRight:
                    return new Rect(Screen.width - (width + padX), Screen.height - (height + padY), width, height);
            }

            return default(Rect);
        }

		public static Rect GetCenteredRect( Vector2 size ) {

			Vector2 pos = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);
			pos -= size * 0.5f;

			return new Rect (pos, size);
		}

		public static Rect GetCenteredRectPerc( Vector2 sizeInScreenPercentage ) {

			return GetCenteredRect (new Vector2 (Screen.width * sizeInScreenPercentage.x, Screen.height * sizeInScreenPercentage.y));

		}

		public	static	Vector2	CalcScreenSizeForContent( GUIContent content, GUIStyle style ) {

			return style.CalcScreenSize (style.CalcSize (content));
		}

		public	static	Vector2	CalcScreenSizeForText( string text, GUIStyle style ) {

			return CalcScreenSizeForContent (new GUIContent (text), style);
		}

		public	static	bool	ButtonWithCalculatedSize( string text ) {

			Vector2 size = CalcScreenSizeForText (text, GUI.skin.button);

			return GUILayout.Button (text, GUILayout.Width (size.x), GUILayout.Height (size.y));
		}

		public	static	bool	ButtonWithColor( Rect rect, string text, Color color) {

			var oldColor = GUI.backgroundColor;
			GUI.backgroundColor = color;

			bool result = GUI.Button (rect, text);

			GUI.backgroundColor = oldColor;

			return result;
		}

		public static void DrawRect (Rect position, Color color, GUIContent content = null)
		{
			var backgroundColor = GUI.backgroundColor;
			GUI.backgroundColor = color;
			styleWithBackground.normal.background = Texture2D.whiteTexture;
			GUI.Box (position, content ?? GUIContent.none, styleWithBackground);
			GUI.backgroundColor = backgroundColor;
		}

		public	static	void	CenteredLabel(Vector2 pos, string text) {

			Vector2 size = CalcScreenSizeForText (text, GUI.skin.label);

			GUI.Label (new Rect (pos - size * 0.5f, size), text);
		}

		/// <summary> Draws the texture flipped around Y axis. </summary>
		public	static	void	DrawTextureWithYFlipped(Rect rect, Texture2D tex) {

			var savedMatrix = GUI.matrix;

			GUIUtility.ScaleAroundPivot (new Vector2 (1, -1), rect.center);

			GUI.DrawTexture (rect, tex);

			GUI.matrix = savedMatrix;
		}

    }
}