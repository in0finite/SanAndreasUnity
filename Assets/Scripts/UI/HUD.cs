using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI {
	
	public class HUD : MonoBehaviour {

		public float crosshairSize = 16;
		public ScaleMode crosshairScaleMode = ScaleMode.StretchToFill;

		public ScreenCorner hudScreenCorner = ScreenCorner.TopRight;
		public Vector2 hudSize = new Vector2 (100, 100);
		public Vector2 hudPadding = new Vector2 (10, 10);


		void Start () {
			
		}

		void Update () {
			
		}

		void OnGUI () {

			if (null == Player.Instance)
				return;

			// draw crosshair
			if (Player.Instance.IsAiming) {
				DrawCrosshair( new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Vector2.one * this.crosshairSize, this.crosshairScaleMode );
			}

			// draw hud
			DrawHud( this.hudScreenCorner, this.hudSize, this.hudPadding );

		}

		public static void DrawCrosshair (Vector2 screenPos, Vector2 size, ScaleMode scaleMode) {
			
			if (null == Weapon.CrosshairTexture)
				return;

			// crosshair texture is actually only a 4th part of the whole crosshair
			// so we have to draw it 4 times

			var oldMatrix = GUI.matrix;

			// upper left
			Rect rect = new Rect( screenPos - size * 0.5f, size * 0.5f );
			GUIUtility.RotateAroundPivot (90, rect.center);
			GUI.DrawTexture( rect, Weapon.CrosshairTexture, scaleMode );

			// upper right
			rect = new Rect( new Vector2(screenPos.x, screenPos.y - size.y * 0.5f), size * 0.5f );
			GUI.matrix = oldMatrix;
			GUIUtility.RotateAroundPivot (180, rect.center);
			GUI.DrawTexture( rect, Weapon.CrosshairTexture, scaleMode );

			// bottom right
			rect = new Rect( new Vector2(screenPos.x, screenPos.y), size * 0.5f );
			GUI.matrix = oldMatrix;
			GUIUtility.RotateAroundPivot (270, rect.center);
			GUI.DrawTexture( rect, Weapon.CrosshairTexture, scaleMode );

			// bottom left
			rect = new Rect( new Vector2(screenPos.x - size.x * 0.5f, screenPos.y), size * 0.5f );
			GUI.matrix = oldMatrix;
			GUIUtility.RotateAroundPivot (360, rect.center);
			GUI.DrawTexture( rect, Weapon.CrosshairTexture, scaleMode );


			GUI.matrix = oldMatrix;

		}

		public static void DrawHud (ScreenCorner screenCorner, Vector2 size, Vector2 padding)
		{

			var rect = GUIUtils.GetCornerRect (screenCorner, size, padding);

			// draw icon for current weapon

			Texture2D tex;
			if (Player.Instance.CurrentWeapon != null)
			{
				tex = Player.Instance.CurrentWeapon.HudTexture;
			}
			else
			{
				tex = Weapon.FistTexture;
			}

			if (tex != null) {
				Rect texRect = rect;
				texRect.width *= 0.4f;
				texRect.height *= 0.5f;

				var savedMatrix = GUI.matrix;
				// we have to flip texture around Y axis
				GUIUtility.ScaleAroundPivot (new Vector2 (1.0f, -1.0f), texRect.center);

			//	GUI.DrawTexture( texRect, tex, ScaleMode.StretchToFill, true, 0.0f, Color.black, 3f, 5f );
				GUI.DrawTexture (texRect, tex);

				GUI.matrix = savedMatrix;
			}


		}

	}

}
