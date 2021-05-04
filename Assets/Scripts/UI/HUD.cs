using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.World;

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
		public Text dayTimeText;

		[SerializeField] [Range(0.3f, 10f)] float m_radioStationLabelDuration = 3f;

		public static Texture2D LeftArrowTexture { get; set; }
		public static Texture2D RightArrowTexture { get; set; }
		public static Texture2D UpArrowTexture { get; set; }
		public static Texture2D DownArrowTexture { get; set; }

		public float regularCrosshairTextureSizeMultiplier = 1.0f;
		public float rocketCrosshairTextureSizeMultiplier = 1.5f;

		public float regularCrosshairUiSizeMultiplier = 1.0f;
		public float rocketCrosshairUiSizeMultiplier = 1.5f;

		private Vector2 m_defaultCrosshairSize;



		void Awake () {
			Instance = this;

			Loader.onLoadSpecialTextures += LoadTextures;

			m_defaultCrosshairSize = this.crosshairImage.rectTransform.sizeDelta;
		}

		void LoadTextures()
		{
			// load arrow textures
			F.RunExceptionSafe(() =>
			{
				var pcbtnsTxd = TextureDictionary.Load("pcbtns");
				LeftArrowTexture = pcbtnsTxd.GetDiffuse("left").Texture;
				RightArrowTexture = pcbtnsTxd.GetDiffuse("right").Texture;
				UpArrowTexture = pcbtnsTxd.GetDiffuse("up").Texture;
				DownArrowTexture = pcbtnsTxd.GetDiffuse("down").Texture;
			});

			LoadCrosshairTextures();

		}

		void LoadCrosshairTextures()
		{
			Texture2D regularCrosshairTex = ConstructCrosshairTexture("siteM16", this.regularCrosshairTextureSizeMultiplier);
			Texture2D rocketCrosshairTex = ConstructCrosshairTexture("siterocket", this.rocketCrosshairTextureSizeMultiplier);

			Weapon.CrosshairTexture = regularCrosshairTex;
			Weapon.RocketCrosshairTexture = rocketCrosshairTex;

			this.crosshairImage.enabled = true;
			this.crosshairImage.texture = regularCrosshairTex;
		}

		static Texture2D ConstructCrosshairTexture(string textureName, float sizeMultiplier)
		{
			Texture2D originalTex = TextureDictionary.Load("hud")
				.GetDiffuse(textureName, new TextureLoadParams { makeNoLongerReadable = false })
				.Texture;

			return ConstructCrosshairTexture(originalTex, sizeMultiplier);
		}

		static Texture2D ConstructCrosshairTexture(Texture2D originalTex, float sizeMultiplier)
		{
			int originalWidth = originalTex.width;
			int originalHeight = originalTex.height;
			int newWidth = Mathf.RoundToInt(originalWidth * 2 * sizeMultiplier);
			int newHeight = Mathf.RoundToInt(originalHeight * 2 * sizeMultiplier);

			Texture2D tex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false, true);

			if (sizeMultiplier > 1)
			{
				// set all pixels to transparent
				Color[] emptyColors = new Color[newWidth * newHeight];
				tex.SetPixels(emptyColors);
			}

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
					tex.SetPixel(newWidth - i - 1 + newWidth, j, originalTex.GetPixel(i, j));
				}
			}

			// top left - flip Y axis
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(i, newHeight - j - 1 + newHeight, originalTex.GetPixel(i, j));
				}
			}

			// top right - flip both X and Y axes
			for (int i = 0; i < originalWidth; i++)
			{
				for (int j = 0; j < originalHeight; j++)
				{
					tex.SetPixel(newWidth - i - 1 + newWidth, newHeight - j - 1 + newHeight, originalTex.GetPixel(i, j));
				}
			}

			tex.Apply(false, true);

			return tex;
		}

		void Update()
		{
			if (!Loader.HasLoaded)
			{
				this.canvas.enabled = false;
				return;
			}

			this.canvas.enabled = true;

			var ped = Ped.Instance;
			bool showPedUi = ped != null;

			this.UpdateWeaponUi(ped, showPedUi);

			this.UpdateOtherUi(ped, showPedUi);

			this.UpdateRadioStationUi(ped, showPedUi);

		}

		void UpdateWeaponUi(Ped ped, bool enableUi)
		{
			if (!enableUi)
			{
				this.crosshairImage.enabled = false;
				this.weaponImage.enabled = false;
				this.weaponAmmoText.enabled = false;
				return;
			}

			var weapon = ped.CurrentWeapon;

			this.crosshairImage.enabled = ped.IsAiming;
			if (this.crosshairImage.enabled)
			{
				if (weapon != null)
				{
					Texture2D crosshairTextureToDisplay = Weapon.CrosshairTexture;
					float uiSizeMultiplier = this.regularCrosshairUiSizeMultiplier;

					if (weapon.Data.modelId1 == WeaponId.RocketLauncher || weapon.Data.modelId1 == WeaponId.RocketLauncherHS)
					{
						crosshairTextureToDisplay = Weapon.RocketCrosshairTexture;
						uiSizeMultiplier = this.rocketCrosshairUiSizeMultiplier;
					}

					if (this.crosshairImage.texture != crosshairTextureToDisplay)
						this.crosshairImage.texture = crosshairTextureToDisplay;

					Vector2 uiSize = m_defaultCrosshairSize * uiSizeMultiplier;
					if (this.crosshairImage.rectTransform.sizeDelta != uiSize)
						this.crosshairImage.rectTransform.sizeDelta = uiSize;
				}
			}

			this.weaponImage.enabled = true;
			Texture2D weaponTextureToDisplay = weapon != null ? weapon.HudTexture : Weapon.FistTexture;
			if (this.weaponImage.texture != weaponTextureToDisplay)
				this.weaponImage.texture = weaponTextureToDisplay;

			this.weaponAmmoText.enabled = true;
			string ammoText = weapon != null ? weapon.AmmoOutsideOfClip + "-" + weapon.AmmoInClip : string.Empty;
			if (this.weaponAmmoText.text != ammoText)
				this.weaponAmmoText.text = ammoText;

		}

		void UpdateRadioStationUi(Ped ped, bool enableUi)
		{
			if (!enableUi)
			{
				this.radioStationText.enabled = false;
				return;
			}

			string textForRadioStation = "";
			var vehicle = ped.CurrentVehicle;
			if (vehicle != null)
			{
				var seat = ped.CurrentVehicleSeat;

				if (vehicle.TimeSinceRadioStationChanged < m_radioStationLabelDuration
				    || (seat != null && seat.TimeSincePedChanged < m_radioStationLabelDuration))
				{
					textForRadioStation = vehicle.CurrentRadioStationIndex >= 0 ?
						RadioStation.StationNames[vehicle.CurrentRadioStationIndex] : "Radio Off";
				}
			}

			this.radioStationText.enabled = true;

			if (this.radioStationText.text != textForRadioStation)
				this.radioStationText.text = textForRadioStation;
		}

		void UpdateOtherUi(Ped ped, bool enableUi)
		{
			if (!enableUi)
			{
				this.healthBackgroundImage.enabled = false;
				this.healthForegroundImage.enabled = false;
				this.pedStateText.enabled = false;
				this.pedVelocityText.enabled = false;
				this.dayTimeText.enabled = false;
				return;
			}

			this.healthForegroundImage.enabled = true;
			this.healthBackgroundImage.enabled = true;
			float healthPerc = Mathf.Clamp01( ped.Health / ped.MaxHealth );
			this.healthForegroundImage.rectTransform.sizeDelta = new Vector2(this.healthBackgroundImage.rectTransform.sizeDelta.x * healthPerc, this.healthForegroundImage.rectTransform.sizeDelta.y);

			this.pedStateText.enabled = true;
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

			this.dayTimeText.enabled = DayTimeManager.Singleton != null;
			if (this.dayTimeText.enabled)
			{
				string dayTimeDisplayText = DayTimeManager.Singleton.CurrentTimeAsString;
				if (this.dayTimeText.text != dayTimeDisplayText)
					this.dayTimeText.text = dayTimeDisplayText;
			}

		}

	}

}
