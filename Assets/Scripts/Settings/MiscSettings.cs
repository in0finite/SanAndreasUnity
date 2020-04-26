using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Weapons;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Settings {

	public class MiscSettings : MonoBehaviour {
		
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
		OptionsWindow.BoolInput m_displayHealthBarsInput = new OptionsWindow.BoolInput ("Display health bar above peds") {
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.displayHealthBarAbovePeds,
			setValue = (value) => { PedManager.Instance.displayHealthBarAbovePeds = value; },
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
		OptionsWindow.BoolInput m_useTouchInput = new OptionsWindow.BoolInput ("Use touch input") {
			isAvailable = () => UIManager.Instance != null,
			getValue = () => UIManager.Instance.UseTouchInput,
			setValue = (value) => { UIManager.Instance.UseTouchInput = value; },
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

		OptionsWindow.FloatInput m_pedSyncRate = new OptionsWindow.FloatInput ("Ped sync rate", 1, 60) {
			isAvailable = () => PedManager.Instance != null,
			getValue = () => PedManager.Instance.pedSyncRate,
			setValue = (value) => { ApplyPedSyncRate(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};

		OptionsWindow.FloatInput m_vehicleSyncRate = new OptionsWindow.FloatInput ("Vehicle sync rate", 1, 60) {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.vehicleSyncRate,
			setValue = (value) => { ApplyVehicleSyncRate(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehicleTransformUsingSyncVars = new OptionsWindow.BoolInput ("Sync vehicle transform using syncvars") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncVehicleTransformUsingSyncVars,
			setValue = (value) => { VehicleManager.Instance.syncVehicleTransformUsingSyncVars = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehiclesLinearVelocity = new OptionsWindow.BoolInput ("Sync vehicle's linear velocity") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncLinearVelocity,
			setValue = (value) => { VehicleManager.Instance.syncLinearVelocity = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncVehiclesAngularVelocity = new OptionsWindow.BoolInput ("Sync vehicle's angular velocity") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncAngularVelocity,
			setValue = (value) => { VehicleManager.Instance.syncAngularVelocity = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_controlWheelsOnLocalPlayer = new OptionsWindow.BoolInput ("Control wheels on local player") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.controlWheelsOnLocalPlayer,
			setValue = (value) => { VehicleManager.Instance.controlWheelsOnLocalPlayer = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_controlVehicleInputOnLocalPlayer = new OptionsWindow.BoolInput ("Control vehicle input on local player") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.controlInputOnLocalPlayer,
			setValue = (value) => { VehicleManager.Instance.controlInputOnLocalPlayer = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.EnumInput<WhenOnClient> m_whenToDisableVehiclesRigidBody = new OptionsWindow.EnumInput<WhenOnClient> () {
			description = "When to disable vehicle rigid body on clients",
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.whenToDisableRigidBody,
			setValue = (value) => { ApplyRigidBodyState(value); },
			persistType = OptionsWindow.InputPersistType.OnStart
		};
		OptionsWindow.BoolInput m_syncPedTransformWhileInVehicle = new OptionsWindow.BoolInput ("Sync ped transform while in vehicle") {
			isAvailable = () => VehicleManager.Instance != null,
			getValue = () => VehicleManager.Instance.syncPedTransformWhileInVehicle,
			setValue = (value) => { VehicleManager.Instance.syncPedTransformWhileInVehicle = value; },
			persistType = OptionsWindow.InputPersistType.OnStart
		};

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



		void Awake ()
		{
			var inputs = new OptionsWindow.Input[] { m_timeScaleInput, m_physicsUpdateRate, m_gravityInput, m_displayHealthBarsInput, m_displayMinimapInput,
				m_runInBackgroundInput, m_drawLineFromGunInput, m_enableCamera, m_useTouchInput, m_displayFpsInput,
				m_pausePlayerSpawning, m_playerSpawnInterval,
				m_pedSyncRate,
				m_vehicleSyncRate, m_syncVehicleTransformUsingSyncVars, m_syncVehiclesLinearVelocity, 
				m_syncVehiclesAngularVelocity, m_controlWheelsOnLocalPlayer, m_controlVehicleInputOnLocalPlayer, 
				m_whenToDisableVehiclesRigidBody, m_syncPedTransformWhileInVehicle,
			};

			foreach (var input in inputs)
			{
				input.category = "MISC";
				OptionsWindow.RegisterInput (input);
			}

		}

		static void ApplyPedSyncRate(float syncRate)
		{
			PedManager.Instance.pedSyncRate = syncRate;
			foreach (var ped in Ped.AllPedsEnumerable)
				ped.ApplySyncRate(syncRate);
		}

		static void ApplyVehicleSyncRate(float syncRate)
		{
			VehicleManager.Instance.vehicleSyncRate = syncRate;
			foreach (var v in Vehicle.AllVehicles)
			{
				v.ApplySyncRate(syncRate);
			}
		}

		static void ApplyRigidBodyState(WhenOnClient when)
		{
			VehicleManager.Instance.whenToDisableRigidBody = when;
			foreach (var v in Vehicle.AllVehicles)
				v.GetComponent<VehicleController>().EnableOrDisableRigidBody();
		}

		static void DisplayFPS(bool bDisplay)
		{
			FPSDisplay.Instance.fpsImage.enabled = bDisplay;
			FPSDisplay.Instance.fpsText.enabled = bDisplay;
			FPSDisplay.Instance.updateFPS = bDisplay;
		}


	}

}
