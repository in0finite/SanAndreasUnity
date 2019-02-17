using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IPedState : Utilities.IState {

		//void OnCollision(Collision info);
		void OnDamaged(DamageInfo info);

	}

}
