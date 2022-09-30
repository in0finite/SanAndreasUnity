using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Weapons;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Settings {

	public class MiscSettings : MonoBehaviour
	{
		static string s_playerName = Player.DefaultPlayerName;

		OptionsWindow.StringInput m_playerNameInput = new OptionsWindow.StringInput
		{
			description = "Player name",
			displayWidth = 200,
			maxNumCharacters = Player.MaxPlayerNameLength,
			getValue = () => s_playerName,
			setValue = ApplyPlayerName,
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_timeScaleInput = new OptionsWindow.FloatInput( "Time scale", 0f, 4f ) {
			getValue = () => Time.timeScale,
			setValue = (value) => { Time.timeScale = value; },
			persistType = OptionsWindow.InputPersistType.None
		};
		OptionsWindow.FloatInput m_physicsUpdateRate = new OptionsWindow.FloatInput( "Physics update rate", 3f, 100f ) {
			getValue = () => 1.0f / Time.fixedDeltaTime,
			setValue = (value) => { Time.fixedDeltaTime = 1.0f / value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_gravityInput = new OptionsWindow.FloatInput( "Gravity", -10f, 50f ) {
			getValue = () => -Physics.gravity.y,
			setValue = (value) => { Physics.gravity = new Vector3(Physics.gravity.x, -value, Physics.gravity.z); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_displayMinimapInput = new OptionsWindow.BoolInput ("Display minimap") {
			isAvailable = () => MiniMap.Instance != null,
			getValue = () => MiniMap.Instance.gameObject.activeSelf,
			setValue = (value) => { MiniMap.Instance.gameObject.SetActive (value); },
			persistType = OptionsWindow.InputPersistType.AfterLoaderFinishes
		};
		OptionsWindow.BoolInput m_runInBackgroundInput = new OptionsWindow.BoolInput ("Run in background") {
			getValue = () => Application.runInBackground,
			setValue = (value) => { Application.runInBackground = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_drawLineFromGunInput = new OptionsWindow.BoolInput ("Draw line from gun") {
			isAvailable = () => WeaponsManager.Instance != null,
			getValue = () => WeaponsManager.Instance.drawLineFromGun,
			setValue = (value) => { WeaponsManager.Instance.drawLineFromGun = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_displayFpsInput = new OptionsWindow.BoolInput("Display FPS")
		{
			isAvailable = () => FPSDisplay.Instance != null,
			getValue = () => FPSDisplay.Instance.updateFPS,
			setValue = (value) => { DisplayFPS(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		// OptionsWindow.IntInput m_imguiFontSize = new OptionsWindow.IntInput ("Imgui font size", 0, 25) {
		// 	//isAvailable = () => UIManager.Instance != null,
		// 	getValue = () => UIManager.Instance.ImguiFontSize,
		// 	setValue = (value) => { UIManager.Instance.ImguiFontSize = value; GUIUtility.ExitGUI(); },
		// 	persistType = OptionsWindow.InputPersistType.OnStart,
		// };

		OptionsWindow.BoolInput m_enableCamera = new OptionsWindow.BoolInput ("Enable camera") {
			getValue = () => Camera.main != null && Camera.main.enabled,
			setValue = (value) => { Camera cam = F.FindMainCameraEvenIfDisabled(); if (cam != null) cam.enabled = value; },
			persistType = OptionsWindow.InputPersistType.None,
		};

		OptionsWindow.BoolInput m_pausePlayerSpawning = new OptionsWindow.BoolInput ("Pause player spawning") {
			isAvailable = () => SpawnManager.Instance != null,
			getValue = () => SpawnManager.Instance.IsSpawningPaused,
			setValue = (value) => { SpawnManager.Instance.IsSpawningPaused = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.FloatInput m_playerSpawnInterval = new OptionsWindow.FloatInput ("Player spawn interval", 0, 30) {
			isAvailable = () => SpawnManager.Instance != null,
			getValue = () => SpawnManager.Instance.spawnInterval,
			setValue = (value) => { SpawnManager.Instance.spawnInterval = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.FloatInput m_projectileReloadTime = new OptionsWindow.FloatInput ("Projectile reload time", 0, 5) {
			isAvailable = () => WeaponsManager.Instance != null,
			getValue = () => WeaponsManager.Instance.projectileReloadTime,
			setValue = (value) => { WeaponsManager.Instance.projectileReloadTime = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.FloatInput m_vehicleDetachedPartLifetime = new OptionsWindow.FloatInput ("Vehicle's detached part lifetime", 10, 120) {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.explosionLeftoverPartsLifetime,
			setValue = (value) => { VehicleManager.Instance.explosionLeftoverPartsLifetime = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		/*OptionsWindow.BoolInput m_showSpeedometerInput = new OptionsWindow.BoolInput() {
			description = "Show speedometer",
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.showPedSpeedometer,
			setValue = (value) => { PedManager.Instance.showPedSpeedometer = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};*/
		OptionsWindow.FloatInput m_turnSpeedInput = new OptionsWindow.FloatInput() {
			description = "Turn speed",
			minValue = 3,
			maxValue = 30,
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.pedTurnSpeed,
			setValue = (value) => { PedManager.Instance.pedTurnSpeed = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_deadBodyLifetime = new OptionsWindow.FloatInput
		{
			description = "Dead body lifetime",
			minValue = 0.5f,
			maxValue = 300f,
			getValue = () => PedManager.Instance.ragdollLifetime,
			setValue = (value) => { PedManager.Instance.ragdollLifetime = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};



		void Awake ()
		{
			var inputs = new OptionsWindow.Input[]
			{
				m_playerNameInput,
				m_timeScaleInput,
				m_physicsUpdateRate,
				m_gravityInput,
				m_displayMinimapInput,
				m_runInBackgroundInput,
				m_drawLineFromGunInput,
				m_enableCamera,
				m_displayFpsInput,
				m_pausePlayerSpawning,
				m_playerSpawnInterval,
				m_vehicleDetachedPartLifetime,
				m_deadBodyLifetime,
				m_projectileReloadTime,
				m_turnSpeedInput,
			};

			foreach (var input in inputs)
			{
				input.category = "MISC";
				OptionsWindow.RegisterInput (input);
			}

			Player.onStart += OnPlayerStart;

		}

		static void DisplayFPS(bool bDisplay)
		{
			FPSDisplay.Instance.fpsImage.enabled = bDisplay;
			FPSDisplay.Instance.fpsText.enabled = bDisplay;
			FPSDisplay.Instance.updateFPS = bDisplay;
		}

		void OnPlayerStart(Player player)
		{
			if (player == Player.Local)
				ApplyPlayerName(s_playerName);
		}

		static void ApplyPlayerName(string newPlayerName)
		{
			s_playerName = newPlayerName;

			if (Player.Local != null)
				Player.Local.RequestNameChange(newPlayerName);
		}

	}

}
