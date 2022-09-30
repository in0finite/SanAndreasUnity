using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public interface IPedState : IState {

		/// <summary> Called at the end of Update(). </summary>
		void PostUpdateState ();

		//void OnCollision(Collision info);
		
		void OnButtonPressed(string buttonName);
		void OnFireButtonPressed();
		void OnAimButtonPressed();
		void OnSubmitPressed ();
		void OnJumpPressed ();
		void OnCrouchButtonPressed ();
		void OnNextWeaponButtonPressed();
		void OnPreviousWeaponButtonPressed();
		void OnFlyButtonPressed();
		void OnFlyThroughButtonPressed();

		/// <summary> Called when server sends a message that ped state has changed. </summary>
		void OnSwitchedStateByServer(byte[] data);
		/// <summary> Called when state is switched on server. The returned data will be available on client 
		/// when the state gets activated. </summary>
		byte[] GetAdditionalNetworkData();

		void OnChangedWeaponByServer(int newSlot);

		void OnWeaponFiredFromServer(Weapon weapon, Vector3 firePos);

		Ped.DamageResult OnDamaged(DamageInfo damageInfo);

	}

}
