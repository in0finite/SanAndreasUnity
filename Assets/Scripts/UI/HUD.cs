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

		public Color healthColor = Color.red;
		public Color healthBackgroundColor = (Color.red + Color.black) * 0.5f;


		void Start () {
			
		}

		void Update () {
			
		}

		void OnGUI () {

			if (!Loader.HasLoaded)
				return;

			if (null == Player.Instance)
				return;

			// draw crosshair
			if (Player.Instance.IsAiming) {
				DrawCrosshair( new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Vector2.one * this.crosshairSize, this.crosshairScaleMode );
			}

			// draw hud
			DrawHud( this.hudScreenCorner, this.hudSize, this.hudPadding, this.healthColor, this.healthBackgroundColor );

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

		public static void DrawHud (ScreenCorner screenCorner, Vector2 size, Vector2 padding, Color healthColor,
			Color healthBackgroundColor)
		{

			var rect = GUIUtils.GetCornerRect (screenCorner, size, padding);

			Player ped = Player.Instance;

			// draw icon for current weapon

			Weapon weapon = Player.Instance.CurrentWeapon;

			Rect texRect = rect;
			texRect.width *= 0.4f;
			texRect.height *= 0.5f;

			Texture2D tex;
			if (weapon != null)
			{
				tex = weapon.HudTexture;
			}
			else
			{
				tex = Weapon.FistTexture;
			}

			if (tex != null) {
				
				var savedMatrix = GUI.matrix;
				// we have to flip texture around Y axis
				GUIUtility.ScaleAroundPivot (new Vector2 (1.0f, -1.0f), texRect.center);

			//	GUI.DrawTexture( texRect, tex, ScaleMode.StretchToFill, true, 0.0f, Color.black, 3f, 5f );
				GUI.DrawTexture (texRect, tex);

				GUI.matrix = savedMatrix;
			}

			// ammo

			if (weapon != null && weapon.IsGun)
			{
				string str = string.Format ("<b>{0}-{1}</b>", weapon.AmmoOutsideOfClip, weapon.AmmoInClip);

				// draw it at the bottom of weapon icon
				Vector2 desiredSize = GUIUtils.CalcScreenSizeForText (str, GUIUtils.CenteredLabelStyle);
				Rect ammoRect = new Rect (new Vector2 (texRect.position.x, texRect.yMax - desiredSize.y / 2.0f), new Vector2 (texRect.width, desiredSize.y));

				GUI.Label (ammoRect, str, GUIUtils.CenteredLabelStyle);
			}


			// health bar

			float barHeight = 8f; //rect.height / 10f;
			float barWidth = rect.width * 0.5f;
			Rect healthBarRect = new Rect (rect.width * 0.5f, texRect.yMax - barHeight, barWidth, barHeight);
			DrawBar( healthBarRect, ped.Health / ped.MaxHealth, healthColor, healthBackgroundColor );

		}

		public static void DrawBar (Rect rect, float fillPerc, Color fillColor, Color backgroundColor)
		{
			
			float borderWidth = 2f; //rect.height / 8f;

			GUIUtils.DrawBar (rect, fillPerc, fillColor, backgroundColor, borderWidth);
		}

	}

}
