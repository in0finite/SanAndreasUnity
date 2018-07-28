using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
	
	public class PedStalker : MonoBehaviour
	{

		public Player Player { get; private set; }



		void Awake ()
		{
			this.Player = this.GetComponentOrLogError<Player> ();
		}

		void Update ()
		{

			// reset input
			this.Player.ResetInput ();

			// run towards player instance

			if (Player.Instance != null) {

				Vector3 diff = Player.InstancePos - this.transform.position;
				Vector3 diffDir = diff.normalized;

				this.Player.IsRunning = true;
				this.Player.Movement = diffDir;
				this.Player.Heading = diffDir;

			}

		}

	}

}
