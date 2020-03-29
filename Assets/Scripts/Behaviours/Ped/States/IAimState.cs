using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IAimState : IPedState
	{

		void StartFiring ();

        Vector3 GetFirePosition();
        Vector3 GetFireDirection();

    }

}
