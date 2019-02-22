using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IPedState : Utilities.IState {

		/// <summary> Called at the end of Update(). </summary>
		void PostUpdateState ();

		//void OnCollision(Collision info);
		void OnDamaged(DamageInfo info);

		void OnSubmitPressed ();
		void OnJumpPressed ();
		void OnFlyButtonPressed();
		void OnFlyThroughButtonPressed();

	}

}
