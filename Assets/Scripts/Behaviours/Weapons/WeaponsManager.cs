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
