using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.UI {
	
	public class HUD : MonoBehaviour {

		public static HUD Instance { get; private set; }

		public Canvas canvas;
		public RawImage weaponImage;
		public Text weaponAmmoText;
		public RawImage healthBackgroundImage;
		public RawImage healthForegroundImage;
		public RawImage crosshairImage;
		public Text pedStateText;
		public Text pedVelocityText;
		public Text radioStationText;
		public RawImage radioStationImage;

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

			Texture2D tex = new Texture2D(originalWidth * 2, originalHeight * 2, TextureFormat.ARGB32, false, true);

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
					tex.SetPixel(originalWidth - i - 1 + originalWidth, j, originalTex.GetPixel(i, j));
				}
			}

			// top left - flip Y axis
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(i, originalHeight - j - 1 + originalHeight, originalTex.GetPixel(i, j));
				}
			}

			// top right - flip both X and Y axes
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(originalWidth - i - 1 + originalWidth, originalHeight - j - 1 + originalHeight, originalTex.GetPixel(i, j));
				}
			}

			tex.Apply(false, true);

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

			string ammoText = weapon != null ? weapon.AmmoOutsideOfClip + "-" + weapon.AmmoInClip : string.Empty;
			if (this.weaponAmmoText.text != ammoText)
				this.weaponAmmoText.text = ammoText;

			float healthPerc = Mathf.Clamp01( ped.Health / ped.MaxHealth );
			this.healthForegroundImage.rectTransform.sizeDelta = new Vector2(this.healthBackgroundImage.rectTransform.sizeDelta.x * healthPerc, this.healthForegroundImage.rectTransform.sizeDelta.y);

			string pedStateDisplayText = "Current ped state: " + (ped.CurrentState != null ? ped.CurrentState.GetType().Name : "none");
			if (pedStateDisplayText != this.pedStateText.text)
				this.pedStateText.text = pedStateDisplayText;

			this.pedVelocityText.enabled = PedManager.Instance.showPedSpeedometer;
			if (this.pedVelocityText.enabled)
			{
				string pedVelocityDisplayText = string.Format("{0:0.0} km/h", ped.GetComponent<PlayerController>().CurVelocity);
				if (pedVelocityDisplayText != this.pedVelocityText.text)
					this.pedVelocityText.text = pedVelocityDisplayText;
			}

			var vehicle = ped.CurrentVehicle;
			if (vehicle != null)
			{
				string text = vehicle.CurrentRadioStationIndex >= 0 ? RadioStation.StationNames[vehicle.CurrentRadioStationIndex] : "Radio Off";
				if (this.radioStationText.text != text)
					this.radioStationText.text = text;
			}
			else
			{
				if (this.radioStationText.text != "")
					this.radioStationText.text = "";
			}

		}

	}

}
