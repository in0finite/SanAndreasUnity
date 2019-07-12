using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IAimState : IPedState
	{

		void StartFiring ();

		void OnClientTriedToFire(Vector3 firePos, Vector3 fireDir);

	}

}
