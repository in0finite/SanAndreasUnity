using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.UI {
	
	public class HUD : MonoBehaviour {

		public float crosshairSize = 16;
		public ScaleMode crosshairScaleMode = ScaleMode.StretchToFill;


		void Start () {
			
		}

		void Update () {
			
		}

		void OnGUI () {

			if (null == Player.Instance || !Player.Instance.WeaponHolder.IsAiming)
				return;

			// draw crosshair
			DrawCrosshair( new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Vector2.one * this.crosshairSize, this.crosshairScaleMode );

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

	}

}
