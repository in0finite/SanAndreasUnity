using Mirror;
using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class BaseVehicleState : BaseScriptState, IVehicleState
	{
		private Vehicle m_currentVehicle;
		public Vehicle CurrentVehicle { get { return m_currentVehicle; } protected set { m_currentVehicle = value; } }

		public Vehicle.Seat CurrentVehicleSeat { get => this.CurrentVehicle != null ? this.CurrentVehicle.GetSeat(this.CurrentVehicleSeatAlignment) : null; }
		public Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get; protected set; }

		protected uint m_currentVehicleNetId = 0;



		public override void OnSwitchedStateByServer(byte[] data)
		{
			// we need to wait for end of frame, because vehicle may not be spawned yet
			//this.StartCoroutine(this.SwitchStateAtEndOfFrame(data));


			// check if this state was already activated
			// it can happen when, among other things, syncvar hooks get invoked twice when creating the ped
			if (this.IsActiveState)
				return;
			
			this.ReadNetworkData(data);

			m_ped.SwitchState(this.GetType());

			if (this.CurrentVehicle != null)
				this.OnVehicleAssigned();

		}

		protected void ReadNetworkData(byte[] data)
		{
			var reader = new Mirror.NetworkReader(data);
			this.ReadNetworkData(reader);
		}

		protected virtual void ReadNetworkData(Mirror.NetworkReader reader)
		{
			// first reset params
			this.CurrentVehicle = null;
			this.CurrentVehicleSeatAlignment = Vehicle.SeatAlignment.None;
			m_currentVehicleNetId = 0;

			// extract vehicle and seat from data

			int magicNumber = reader.ReadInt();
			m_currentVehicleNetId = reader.ReadUInt();
			this.CurrentVehicleSeatAlignment = (Vehicle.SeatAlignment) reader.ReadSByte();

			// assign current vehicle
			GameObject vehicleGo = Net.NetManager.GetNetworkObjectById(m_currentVehicleNetId);
			this.CurrentVehicle = vehicleGo != null ? vehicleGo.GetComponent<Vehicle>() : null;

			if (magicNumber != 123456789)
				Debug.LogErrorFormat("magicNumber {0}, m_currentVehicleNetId {1}, data size {2} - this should not happen", magicNumber, m_currentVehicleNetId, reader.Length);

		}

		public override byte[] GetAdditionalNetworkData()
		{
			var writer = new Mirror.NetworkWriter();
			this.GetAdditionalNetworkData(writer);
			return writer.ToArray();
		}

		protected virtual void GetAdditionalNetworkData(Mirror.NetworkWriter writer)
		{
			writer.Write((int)123456789);
			if (this.CurrentVehicle != null) {
				writer.Write((uint)this.CurrentVehicle.NetTransform.netId);
				writer.Write((sbyte)this.CurrentVehicleSeatAlignment);
			} else {
				writer.Write((uint)0);
				writer.Write((sbyte)Vehicle.SeatAlignment.None);
			}
		}

		System.Collections.IEnumerator SwitchStateAtEndOfFrame(byte[] data)
		{
			var oldState = m_ped.CurrentState;

			yield return new WaitForEndOfFrame();

			if (oldState != m_ped.CurrentState)
			{
				// state changed in the meantime
				// did server change it ? or syncvar hooks invoked twice ? either way, we should stop here

				// Debug.LogFormat("state changed in the meantime, old: {0}, new: {1}", oldState != null ? oldState.GetType().Name : "",
				// 	m_ped.CurrentState != null ? m_ped.CurrentState.GetType().Name : "");
				yield break;
			}

			// read current vehicle here - it should've been spawned by now
			this.ReadNetworkData(data);

		//	Debug.LogFormat("Switching to state {0}, vehicle: {1}, seat: {2}", this.GetType().Name, this.CurrentVehicle, this.CurrentVehicleSeat);

			// now we can enter this state
			m_ped.SwitchState(this.GetType());
		}

		protected virtual void OnVehicleAssigned()
		{

		}

		public static void PreparePedForVehicle(Ped ped, Vehicle vehicle, Vehicle.Seat seat)
		{

			seat.OccupyingPed = ped;

			ped.characterController.enabled = false;


			ped.transform.SetParent(seat.Parent);
			ped.transform.localPosition = Vector3.zero;
			ped.transform.localRotation = Quaternion.identity;

			ped.PlayerModel.IsInVehicle = true;

			if (!VehicleManager.Instance.syncPedTransformWhileInVehicle) {
				if (ped.NetTransform != null)
					ped.NetTransform.enabled = false;
			}

			F.RunExceptionSafe( () => vehicle.OnPedPreparedForVehicle(ped, seat) );

			ped.NavMeshAgent.enabled = false;

		}

		protected void Cleanup()
		{
			if (!m_ped.IsInVehicle)
			{
				m_ped.characterController.enabled = true;
				m_ped.transform.SetParent(null, true);
				m_model.IsInVehicle = false;
				if (m_ped.NetTransform != null)
                {
                    m_ped.NetTransform.enabled = true;
					m_ped.NetTransform.TransformSyncer.ResetSyncDataToTransform();
                }
                if (this.CurrentVehicle != null)
					F.RunExceptionSafe( () => this.CurrentVehicle.OnPedRemovedFromVehicle(m_ped, this.CurrentVehicleSeat) );
				m_ped.NavMeshAgent.enabled = true;
			}

			if (this.CurrentVehicleSeat != null && this.CurrentVehicleSeat.OccupyingPed == m_ped)
				this.CurrentVehicleSeat.OccupyingPed = null;

			this.CurrentVehicle = null;
			this.CurrentVehicleSeatAlignment = Vehicle.SeatAlignment.None;
			m_currentVehicleNetId = 0;

		}


		public override void UpdateState()
		{
			base.UpdateState();

			if (!this.IsActiveState)
				return;
			
			if (Net.NetStatus.IsClientOnly)
			{
				if (null == this.CurrentVehicle)
				{
					// check if vehicle was spawned in the meantime
					GameObject vehicleGo = Net.NetManager.GetNetworkObjectById(m_currentVehicleNetId);
					if (vehicleGo != null)
					{
						// vehicle is spawned
						this.CurrentVehicle = vehicleGo.GetComponent<Vehicle>();
						this.OnVehicleAssigned();
					}
				}
			}

		}

		protected override void UpdateHeading()
		{
			
		}

		protected override void UpdateRotation()
		{
			
		}

		protected override void UpdateMovement()
		{
			
		}

		protected override void ConstrainRotation ()
		{
			
		}

		public bool CanEnterVehicle (Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
		{
			if (m_ped.IsInVehicle)
				return false;

			// this should be removed
			if (m_ped.IsAiming || m_ped.WeaponHolder.IsFiring)
				return false;

			var seat = vehicle.GetSeat (seatAlignment);
			if (null == seat)
				return false;

			// check if specified seat is taken
			if (seat.IsTaken)
				return false;

			// everything is ok, we can enter vehicle

			return true;
		}

		public override void KillPed()
		{
			if (m_ped.CurrentVehicle != null && m_ped.CurrentVehicle.ExplodedThisFrame)
			{
				base.KillPed();
				return;
			}
			
			// don't detach ragdoll, because it will collide with vehicle and vehicle will fly away
			Object.Destroy(m_ped.gameObject);
		}

	}

}
