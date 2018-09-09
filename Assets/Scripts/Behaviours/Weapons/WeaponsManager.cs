using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Weapons
{
	
	public class WeaponsManager : MonoBehaviour {

		[SerializeField] [Range(2.5f, 4.5f)] private float m_animConvertMultiplier = 3.5f;
		public float AnimConvertMultiplier { get { return m_animConvertMultiplier; } set { m_animConvertMultiplier = value; } }

		[SerializeField] [Range(0.0f, 2.0f)] private float m_gunFlashDuration = 0.035f;
		public float GunFlashDuration { get { return m_gunFlashDuration; } set { m_gunFlashDuration = value; } }

		[SerializeField] [Range(0.0f, 7200.0f)] private float m_gunFlashRotationSpeed = 1800.0f;
		public float GunFlashRotationSpeed { get { return m_gunFlashRotationSpeed; } set { m_gunFlashRotationSpeed = value; } }

		[Space(10)]
		[Header("AIMWITHARM")]

		public Vector3 AIMWITHARM_headRotationOffset = Vector3.zero;

		public Vector3 AIMWITHARM_clavicleRotationOffset = Vector3.zero;
		public Vector3 AIMWITHARM_upperArmRotationOffset = Vector3.zero;
		public Vector3 AIMWITHARM_foreArmRotationOffset = Vector3.zero;
		public Vector3 AIMWITHARM_handRotationOffset = Vector3.zero;

		public bool AIMWITHARM_controlUpperArm = true;
		public bool AIMWITHARM_controlForeArm = true;
		public bool AIMWITHARM_controlHand = true;

		// relative to player
		public Vector3 AIMWITHARM_upperArmStartRotationEulers = new Vector3 (-1.686f, 164.627f, -97.904f);
		public Vector3 AIMWITHARM_upperArmEndRotationEulers = new Vector3 (150f, -90f, 0f);

		[Range(5, 175)] public float AIMWITHARM_maxAimAngle = 90f;

		[Range(0, 90)] public float AIMWITHARM_maxHeadRotationAngle = 75f;

		[Space(15)]

		public LayerMask projectileRaycastMask = Physics.DefaultRaycastLayers;

		public bool drawLineFromGun = false;


		public static WeaponsManager Instance { get; private set; }



		void Awake ()
		{
			Instance = this;

		}

		public static float ConvertAnimTime (float timeInFile)
		{
			return timeInFile * Instance.AnimConvertMultiplier / 100.0f;
		}

	}

}
