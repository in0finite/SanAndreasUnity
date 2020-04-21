using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;
using SanAndreasUnity.Importing.Conversion;

namespace SanAndreasUnity.UI {
	
	public class HUD : MonoBehaviour {

		public static HUD Instance { get; private set; }

		public float crosshairSize = 16;
		public ScaleMode crosshairScaleMode = ScaleMode.StretchToFill;

		public ScreenCorner hudScreenCorner = ScreenCorner.TopRight;
		public Vector2 hudSize = new Vector2 (100, 100);
		public Vector2 hudPadding = new Vector2 (10, 10);

		public Color healthColor = Color.red;
		public Color healthBackgroundColor = (Color.red + Color.black) * 0.5f;

		public bool drawRedDotOnScreenCenter = false;

		public Canvas canvas;
		public RawImage weaponImage;
		public Text weaponAmmoText;
		public RawImage healthBackgroundImage;
		public RawImage healthForegroundImage;
		public RawImage crosshairImage;

		public static Texture2D LeftArrowTexture { get; set; }
		public static Texture2D RightArrowTexture { get; set; }
		public static Texture2D UpArrowTexture { get; set; }
		public static Texture2D DownArrowTexture { get; set; }



		void Awake () {
			Instance = this;

			Loader.onLoadSpecialTextures += LoadTextures;
		}

		void LoadTextures()
		{
			// load arrow textures
			var pcbtnsTxd = TextureDictionary.Load("pcbtns");
			LeftArrowTexture = pcbtnsTxd.GetDiffuse("left").Texture;
			RightArrowTexture = pcbtnsTxd.GetDiffuse("right").Texture;
			UpArrowTexture = pcbtnsTxd.GetDiffuse("up").Texture;
			DownArrowTexture = pcbtnsTxd.GetDiffuse("down").Texture;

			LoadCrosshairTexture();

		}

		void LoadCrosshairTexture()
		{

			Texture2D originalTex = TextureDictionary.Load("hud")
				.GetDiffuse("siteM16", new TextureLoadParams { makeNoLongerReadable = false })
				.Texture;

			// construct crosshair texture

			int originalWidth = originalTex.width;
			int originalHeight = originalTex.height;

			Texture2D tex = new Texture2D(originalWidth * 2, originalHeight * 2);

			// bottom left
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(i, j, originalTex.GetPixel(i, j));
				}
			}

			// bottom right - flip around X axis
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(originalWidth - i + originalWidth, j, originalTex.GetPixel(i, j));
				}
			}

			// top left - flip Y axis
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(i, originalHeight - j + originalHeight, originalTex.GetPixel(i, j));
				}
			}

			// top right - flip both X and Y axes
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(originalWidth - i + originalWidth, originalHeight - j + originalHeight, originalTex.GetPixel(i, j));
				}
			}

			//tex.Apply(false, true);
			tex.Apply();

			Weapon.CrosshairTexture = tex;
			this.crosshairImage.enabled = true;
			this.crosshairImage.texture = tex;

		}

		void Update()
		{
			if (!Loader.HasLoaded)
			{
				this.canvas.enabled = false;
				return;
			}

			var ped = Ped.Instance;

			if (null == ped)
			{
				this.canvas.enabled = false;
				return;
			}

			this.canvas.enabled = true;

			this.crosshairImage.enabled = ped.IsAiming;

			var weapon = ped.CurrentWeapon;

			Texture2D weaponTextureToDisplay = weapon != null ? weapon.HudTexture : Weapon.FistTexture;
			if (this.weaponImage.texture != weaponTextureToDisplay)
				this.weaponImage.texture = weaponTextureToDisplay;

			//System.Text.StringBuilder sb = new System.Text.StringBuilder();

			string ammoText = weapon != null ? weapon.AmmoOutsideOfClip + "-" + weapon.AmmoInClip : string.Empty;
			if (this.weaponAmmoText.text != ammoText)
				this.weaponAmmoText.text = ammoText;

			float healthPerc = Mathf.Clamp01( ped.Health / ped.MaxHealth );
			this.healthForegroundImage.rectTransform.sizeDelta = new Vector2(this.healthBackgroundImage.rectTransform.sizeDelta.x * healthPerc, this.healthForegroundImage.rectTransform.sizeDelta.y);

		}

		void OnGUI () {

			if (!Loader.HasLoaded)
				return;

			var ped = Ped.Instance;

			if (null == ped)
				return;

			// draw crosshair
			if (ped.IsAiming) {
				//DrawCrosshair( new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), Vector2.one * this.crosshairSize, this.crosshairScaleMode );
			}

			// draw hud
			DrawHud( this.hudScreenCorner, this.hudSize, this.hudPadding, this.healthColor, this.healthBackgroundColor );

			// draw dot in the middle of screen
			if (this.drawRedDotOnScreenCenter)
				GUIUtils.DrawRect (GUIUtils.GetCenteredRect (new Vector2 (2f, 2f)), Color.red);

			// let current state draw it's own hud
			if (ped.CurrentState != null)
				ped.CurrentState.OnDrawHUD();

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

			Ped ped = Ped.Instance;

			// draw icon for current weapon

			Weapon weapon = Ped.Instance.CurrentWeapon;

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
