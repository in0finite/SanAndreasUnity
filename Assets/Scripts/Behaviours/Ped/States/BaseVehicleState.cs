using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	public class BaseVehicleState : BaseScriptState, IVehicleState
	{
		private Vehicle m_currentVehicle;
		public Vehicle CurrentVehicle { get { return m_currentVehicle; } protected set { m_currentVehicle = value; } }

		public Vehicle.Seat CurrentVehicleSeat { get; protected set; }
		public Vehicle.SeatAlignment CurrentVehicleSeatAlignment { get { return this.CurrentVehicleSeat.Alignment; } }


		public override void OnSwitchedStateByServer(byte[] data)
		{
			// extract vehicle and seat from data
			var reader = new Mirror.NetworkReader(data);
			GameObject vehicleGo = reader.ReadGameObject();
			Vehicle.SeatAlignment seatAlignment = (Vehicle.SeatAlignment) reader.ReadSByte();

			// assign params
			this.CurrentVehicle = vehicleGo != null ? vehicleGo.GetComponent<Vehicle>() : null;
			this.CurrentVehicleSeat = this.CurrentVehicle != null ? this.CurrentVehicle.GetSeat(seatAlignment) : null;

			// switch state
			m_ped.SwitchState(this.GetType());
		}

		public override byte[] GetAdditionalNetworkData()
		{
			var writer = new Mirror.NetworkWriter();
			if (this.CurrentVehicle != null) {
				writer.Write(this.CurrentVehicle.gameObject);
				writer.Write((sbyte)this.CurrentVehicleSeatAlignment);
			} else {
				writer.Write((GameObject)null);
				writer.Write((sbyte)Vehicle.SeatAlignment.None);
			}
			
			return writer.ToArray();
		}

		protected void Cleanup()
		{
			if (!m_ped.IsInVehicle)
			{
				m_ped.characterController.enabled = true;
				m_ped.transform.SetParent(null, true);
				m_model.IsInVehicle = false;
				// enable network transform
				if (m_ped.NetTransform != null)
					m_ped.NetTransform.enabled = true;
			}

			if (this.CurrentVehicleSeat != null && this.CurrentVehicleSeat.OccupyingPed == m_ped)
				this.CurrentVehicleSeat.OccupyingPed = null;

			this.CurrentVehicle = null;
			this.CurrentVehicleSeat = null;
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

	}

}
