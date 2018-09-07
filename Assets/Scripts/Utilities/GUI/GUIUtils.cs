using UnityEngine;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{
    public enum ScreenCorner { TopRight, TopLeft, BottomRight, BottomLeft }

    public static class GUIUtils
    {

		private	static	GUIStyle	styleWithBackground = new GUIStyle ();

		private static GUIStyle s_centeredLabelStyle = null;
		public static GUIStyle CenteredLabelStyle {
			get {
				if (null == s_centeredLabelStyle)
					s_centeredLabelStyle = new GUIStyle (GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
				return s_centeredLabelStyle;
			}
		}



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

		public static void DrawBar (Rect rect, float fillPerc, Color fillColor, Color backgroundColor, float borderWidth)
		{
			fillPerc = Mathf.Clamp01 (fillPerc);

			Rect fillRect = rect;
			fillRect.position += Vector2.one * borderWidth;
			fillRect.size -= Vector2.one * borderWidth * 2;

			// first fill with black - that will be the border
			GUIUtils.DrawRect( rect, Color.black );

			// fill with background
			GUIUtils.DrawRect( fillRect, backgroundColor );

			// draw filled part
			fillRect.width *= fillPerc;
			GUIUtils.DrawRect( fillRect, fillColor );

		}

		public static int TabsControl (int currentTabIndex, params string[] tabNames)
		{
			return GUILayout.Toolbar (currentTabIndex, tabNames);
		}

		public static Rect GetRectForBarAsBillboard (Vector3 worldPos, float worldWidth, float worldHeight, Camera cam)
		{

			Vector3 camRight = cam.transform.right;
		//	Vector3 camUp = cam.transform.up;

//			Vector3 upperLeft = worldPos - camRight * worldWidth * 0.5f + camUp * worldHeight * 0.5f;
//			Vector3 upperRight = upperLeft + camRight * worldWidth;
//			Vector3 lowerLeft = upperLeft - camUp * worldHeight;
//			Vector3 lowerRight = lowerLeft + camRight * worldWidth;

			Vector3 leftWorld = worldPos - camRight * worldWidth * 0.5f;
			Vector3 rightWorld = worldPos + camRight * worldWidth * 0.5f;

			Vector3 leftScreen = cam.WorldToScreenPoint (leftWorld);
			Vector3 rightScreen = cam.WorldToScreenPoint (rightWorld);

			if (leftScreen.z < 0 || rightScreen.z < 0)
				return Rect.zero;

			// transform to gui coordinates
			leftScreen.y = Screen.height - leftScreen.y;
			rightScreen.y = Screen.height - rightScreen.y;

			float screenWidth = rightScreen.x - leftScreen.x;
			float screenHeight = screenWidth * worldHeight / worldWidth;

			return new Rect (new Vector2(leftScreen.x, leftScreen.y - screenHeight * 0.5f), new Vector2(screenWidth, screenHeight) );
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

		public static Rect DrawItemsInARowPerc (Rect rect, System.Action<Rect, string> drawItem, string[] items, float[] widthPercs ) {

			Rect itemRect = rect;
			float x = rect.position.x;

			for (int i = 0; i < items.Length; i++) {
				float width = widthPercs [i] * rect.width;

				itemRect.position = new Vector2 (x, itemRect.position.y);
				itemRect.width = width;

				drawItem (itemRect, items [i]);

				x += width;
			}

			rect.position += new Vector2 (x, 0f);
			rect.width -= x;
			return rect;
		}

		public static Rect DrawItemsInARow (Rect rect, System.Action<Rect, string> drawItem, string[] items, float[] widths ) {

			float[] widthPercs = new float[widths.Length];
			for (int i = 0; i < widths.Length; i++) {
				widthPercs [i] = widths [i] / rect.width;
			}

			return DrawItemsInARowPerc (rect, drawItem, items, widthPercs);
		}

		public static Rect GetNextRectInARowPerc (Rect rowRect, ref int currentRectIndex, float spacing, params float[] widthPercs) {

			float x = rowRect.position.x;

			for (int i = 0; i < currentRectIndex; i++) {
				x += widthPercs [i] * rowRect.width;
				x += spacing;
			}

			float width = widthPercs [currentRectIndex] * rowRect.width;
			currentRectIndex++;

			return new Rect( x, rowRect.position.y, width, rowRect.height );
		}

		public static Rect GetNextRectInARow (Rect rowRect, ref int currentRectIndex, float spacing, params float[] widths) {

			float[] widthPercs = new float[widths.Length];
			for (int i = 0; i < widths.Length; i++) {
				widthPercs [i] = widths [i] / rowRect.width;
			}

			return GetNextRectInARowPerc (rowRect, ref currentRectIndex, spacing, widthPercs);
		}

		public static int DrawPagedViewNumbers (Rect rect, int currentPage, int numPages)
		{
			int resultingPage = currentPage;

			float spacing = 1f;
			rect.width = 25f;


			if (GUI.Button (rect, "<")) {
				resultingPage--;
			}
			rect.position += new Vector2(rect.width + spacing, 0f);

			for (int i = 0; i < numPages; i++) {
				var style = currentPage == (i + 1) ? GUI.skin.box : GUI.skin.button;
				if (GUI.Button (rect, (i + 1).ToString (), style))
					resultingPage = i + 1;
				rect.position += new Vector2(rect.width + spacing, 0f);
			}

			if (GUI.Button (rect, ">")) {
				resultingPage++;
			}
			rect.position += new Vector2(rect.width + spacing, 0f);


			resultingPage = Mathf.Clamp( resultingPage, 1, numPages );

			return resultingPage;
		}

		public static int DrawPagedViewNumbers (Rect rect, int currentPage, int totalNumItems, int numItemsPerPage)
		{
			int numPages = Mathf.CeilToInt (totalNumItems / (float) numItemsPerPage);
			return DrawPagedViewNumbers (rect, currentPage, numPages);
		}

    }
}