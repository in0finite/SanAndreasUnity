using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Weapons
{
	
	public class WeaponsManager : MonoBehaviour {

		[SerializeField] [Range(2.5f, 4.5f)] private float m_animConvertMultiplier = 3.5f;
		public float AnimConvertMultiplier { get { return m_animConvertMultiplier; } set { m_animConvertMultiplier = value; } }

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
