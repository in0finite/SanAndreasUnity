using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Importing.Animation;
using System.Linq;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class VehicleExitingState : BaseVehicleState
	{
		Coroutine m_coroutine;
		bool m_isExitingImmediately = false;


		public override void OnBecameActive()
		{
			base.OnBecameActive();
			this.ExitVehicleInternal();
		}

		public override void OnBecameInactive()
		{
			this.Cleanup();

			m_isExitingImmediately = false;

			if (m_coroutine != null)
				StopCoroutine(m_coroutine);
			m_coroutine = null;

			base.OnBecameInactive();
		}

		public override void OnSwitchedStateByServer()
		{
			// obtain current vehicle and seat from Ped
			this.CurrentVehicle = m_ped.CurrentVehicle;
			this.CurrentVehicleSeat = m_ped.CurrentVehicleSeat;

			m_isExitingImmediately = false;

			// switch state
			m_ped.SwitchState(this.GetType());
		}

		public void ExitVehicle(bool immediate = false)
		{
			if (!m_ped.IsInVehicle || !m_ped.IsInVehicleSeat)
				return;
			
			// obtain current vehicle from Ped
			this.CurrentVehicle = m_ped.CurrentVehicle;
			this.CurrentVehicleSeat = m_ped.CurrentVehicleSeat;

			m_isExitingImmediately = immediate;

			// after obtaining parameters, switch to this state
			m_ped.SwitchState<VehicleExitingState> ();
			
			// we'll do the rest of the work when our state gets activated

		}

		void ExitVehicleInternal()
		{
			VehicleEnteringState.PreparePedForVehicle(m_ped, this.CurrentVehicle, this.CurrentVehicleSeat);

			// TODO: no need for this, vehicle should know when there is no driver
			// TODO: but, right now, this should be included in cleanup
			if (this.CurrentVehicleSeat.IsDriver)
				this.CurrentVehicle.StopControlling();
			
			if (m_isServer && this.CurrentVehicleSeat.IsDriver)
			{
				// remove authority
				Net.NetManager.RemoveAuthority(this.CurrentVehicle.gameObject);
			}

			// send message to clients
			if (m_isServer)
			{
				if (!m_isExitingImmediately)
				{
					// We don't need to send this message. If ped exits immediately, he will switch to stand state.
					// If he starts exiting, he will change state to this one. Either way, switching state on client
					// will be performed ok.
				}
			}

			m_coroutine = StartCoroutine (ExitVehicleAnimation (m_isExitingImmediately));

		}

		private System.Collections.IEnumerator ExitVehicleAnimation(bool immediate)
		{

			var seat = this.CurrentVehicleSeat;

			var animIndex = seat.IsLeftHand ? AnimIndex.GetOutLeft : AnimIndex.GetOutRight;

			m_model.VehicleParentOffset = Vector3.Scale(m_model.GetAnim(AnimGroup.Car, animIndex).RootStart, new Vector3(-1, -1, -1));

			if (!immediate)
			{
				var animState = m_model.PlayAnim(AnimGroup.Car, animIndex, PlayMode.StopAll);
				animState.wrapMode = WrapMode.Once;

				// wait until anim finishes or stops
				while (animState.enabled)
					yield return new WaitForEndOfFrame();
			}

			// ped now completely exited the vehicle


			m_ped.transform.localPosition = m_model.VehicleParentOffset;
			m_ped.transform.localRotation = Quaternion.identity;

			m_model.VehicleParentOffset = Vector3.zero;
			
			// now switch to other state
			// when our state gets deactivated, it will cleanup everything
			
			if (m_isServer)
				m_ped.SwitchState<StandState> ();

		}

	}

}
