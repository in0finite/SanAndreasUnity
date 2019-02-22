using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IPedState : Utilities.IState {

		//void OnCollision(Collision info);
		void OnDamaged(DamageInfo info);

		void OnSubmitPressed ();
		void OnJumpPressed ();
		void OnFlyButtonPressed();
		void OnFlyThroughButtonPressed();

	}

}
