using UnityEngine;
using SanAndreasUnity.UI;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Settings {

	public class CameraSettings : MonoBehaviour {
		
		static float s_farClipPlane = 1000;
		static float s_fieldOfView = 60;

		OptionsWindow.FloatInput m_farClipPlaneInput = new OptionsWindow.FloatInput() {
			description = "Far clip plane",
			minValue = 100,
			maxValue = 5000,
			getValue = () => Camera.main != null ? Camera.main.farClipPlane : s_farClipPlane,
			setValue = (value) => { s_farClipPlane = value; if (Camera.main != null) Camera.main.farClipPlane = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_fieldOfViewInput = new OptionsWindow.FloatInput() {
			description = "Field of view",
			minValue = 20,
			maxValue = 120,
			getValue = () => Camera.main != null ? Camera.main.fieldOfView : s_fieldOfView,
			setValue = (value) => { s_fieldOfView = value; if (Camera.main != null) Camera.main.fieldOfView = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_cameraDistanceFromPed = new OptionsWindow.FloatInput() {
			description = "Camera distance from ped",
			minValue = 0.1f,
			maxValue = 100f,
			getValue = () => PedManager.Instance.cameraDistanceFromPed,
			setValue = (value) => { PedManager.Instance.cameraDistanceFromPed = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};
		OptionsWindow.FloatInput m_cameraDistanceFromVehicle = new OptionsWindow.FloatInput() {
			description = "Camera distance from vehicle",
			minValue = 0.1f,
			maxValue = 100f,
			getValue = () => VehicleManager.Instance.cameraDistanceFromVehicle,
			setValue = (value) => { VehicleManager.Instance.cameraDistanceFromVehicle = value; },
			persistType = OptionsWindow.InputPersistType.OnStart,
		};



		void Awake ()
		{
			OptionsWindow.RegisterInputs ("CAMERA", m_farClipPlaneInput, m_fieldOfViewInput, m_cameraDistanceFromPed, m_cameraDistanceFromVehicle);
			UnityEngine.SceneManagement.SceneManager.activeSceneChanged += (s1, s2) => OnActiveSceneChanged();
		}

		void Start ()
		{
			// assign min & max values for camera distance
			m_cameraDistanceFromPed.minValue = m_cameraDistanceFromVehicle.minValue = PedManager.Instance.minCameraDistanceFromPed;
			m_cameraDistanceFromPed.maxValue = m_cameraDistanceFromVehicle.maxValue = PedManager.Instance.maxCameraDistanceFromPed;
		}

		void OnActiveSceneChanged()
		{
			// apply settings

			Camera cam = Utilities.F.FindMainCameraEvenIfDisabled();
			if (cam != null)
			{
				cam.farClipPlane = s_farClipPlane;
				cam.fieldOfView = s_fieldOfView;
			}
			
		}

	}

}
