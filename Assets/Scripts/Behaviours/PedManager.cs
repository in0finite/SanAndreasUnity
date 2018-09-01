using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedManager : MonoBehaviour
	{
		public static PedManager Instance { get; private set; }

		public bool displayHealthBarAbovePeds = false;
		public float healthBarWorldWidth = 0.5f;
		public float healthBarWorldHeight = 0.1f;
		public float healthBarMaxScreenHeight = 20f;
		public float healthBarVerticalOffset = 0.3f;



		void Awake ()
		{
			Instance = this;
		}

	}

}
