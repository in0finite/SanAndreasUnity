using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedManager : MonoBehaviour
	{
		public static PedManager Instance { get; private set; }

		public GameObject pedPrefab;

		[Header("Health bar")]

		public bool displayHealthBarAbovePeds = false;
		public float healthBarWorldWidth = 0.5f;
		public float healthBarWorldHeight = 0.1f;
		public float healthBarMaxScreenHeight = 20f;
		public float healthBarVerticalOffset = 0.3f;

		[Header("Ped AI")]

		public float AIStoppingDistance = 3f;
		public float AIVehicleEnterDistance = 1.25f;
		public float AIOutOfRangeTimeout = 5f;
		public float AIOutOfRangeDistance = 250f;

		[Header("Net")]

		public float pedSyncRate = 10;


		void Awake ()
		{
			Instance = this;
		}

	}

}
