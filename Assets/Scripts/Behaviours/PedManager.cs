using System.Collections.Generic;
using SanAndreasUnity.Behaviours.World;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedManager : MonoBehaviour
	{
		public static PedManager Instance { get; private set; }

		public GameObject pedPrefab;

		public float pedTurnSpeed = 10f;

		public bool showPedSpeedometer = true;

		public LayerMask groundFindingIgnoredLayerMask = 0;

		public FocusPointParameters playerPedFocusPointParameters = new FocusPointParameters(false, 0f, 3f);
		public FocusPointParameters npcPedFocusPointParameters = FocusPointParameters.Default;

		public float minTimeToReturnToAimState = 0.33f;

		[Header("Camera")]

		public float cameraDistanceFromPed = 3f;
		public float minCameraDistanceFromPed = 2f;
		public float maxCameraDistanceFromPed = 30f;

		public LayerMask cameraRaycastIgnoredLayerMask = 0;

		[Header("Damage")]

		[Range(0f, 10f)] public float pedDamageMultiplier = 1f;

		public float legAndArmDamageMultiplier = 0.8f;
		public float stomachAndChestDamageMultiplier = 1.0f;
		public float headDamageMultiplier = 4.0f;

		public float inflictedDamageMessageVelocityInScreenPerc = 0.2f;
		public float inflictedDamageMessageLifetime = 1.0f;
		public Color inflictedDamageMessageColor = Color.green;

		[Header("Health bar")]

		public float healthBarVisibleTimeAfterDamage = 3f;
		public float healthBarWorldWidth = 0.5f;
		public float healthBarWorldHeight = 0.1f;
		public float healthBarMaxScreenHeight = 20f;
		public float healthBarVerticalOffset = 0.3f;
		public Color healthColor = Color.red;
		public Color healthBackgroundColor = (Color.red + Color.black) * 0.5f;

		[Header("Ped AI")]

		public float AIStoppingDistance = 3f;
		public float AIVehicleEnterDistance = 1.25f;

		[Header("Net")]

		public float pedSyncRate = 10;

		[Header("Ragdoll")]

		public GameObject ragdollPrefab;
		public float ragdollMass = 100f;
		public float ragdollLifetime = 30f;
		public float ragdollDrag = 0.05f;
		public float ragdollMaxDepenetrationVelocity = 10f;
		public float ragdollDamageForce = 4f;
		public float ragdollDamageForceWhenDetached = 4f;
		public CollisionDetectionMode ragdollCollisionDetectionMode = CollisionDetectionMode.Discrete;
		[Range(1, 60)] public float ragdollSyncRate = 20f;
		public RigidbodyInterpolation ragdollInterpolationMode = RigidbodyInterpolation.Extrapolate;


		void Awake ()
		{
			Instance = this;
		}

	}

}
