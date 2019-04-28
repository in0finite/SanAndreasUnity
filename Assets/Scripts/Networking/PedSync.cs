using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Behaviours.Peds.States;

namespace SanAndreasUnity.Net
{
    public class PedSync : NetworkBehaviour
    {        
        Player m_player;
        Ped m_ped { get { return m_player.OwnedPed; } }
        public static PedSync Local { get; private set; }


        void Awake()
        {
            m_player = this.GetComponentOrThrow<Player>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Local = this;
        }

        void Start()
        {

        }

        public void SendInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, bool isJumpOn, Vector3 heading)
        {
            this.CmdSendingInput(isWalkOn, isRunOn, isSprintOn, movementInput, isJumpOn, heading);
        }

        public void SendInput()
        {
            Ped ped = m_ped;
            this.SendInput(ped.IsWalkOn, ped.IsRunOn, ped.IsSprintOn, ped.Movement, ped.IsJumpOn, ped.Heading);
        }

        [Command]
        void CmdSendingInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, bool isJumpOn, Vector3 heading)
        {
            if (null == m_ped)
                return;

            Ped ped = m_ped;

            ped.IsWalkOn = isWalkOn;
            ped.IsRunOn = isRunOn;
            ped.IsSprintOn = isSprintOn;
            ped.Movement = movementInput;
            ped.IsJumpOn = isJumpOn;
            ped.Heading = heading;

        }

        public void OnCrouchButtonPressed() => this.CmdOnCrouchButtonPressed();
        
        [Command]
        void CmdOnCrouchButtonPressed()
        {
            if (m_ped)
                m_ped.OnCrouchButtonPressed();
        }

        public void PedStartedEnteringVehicle(Ped ped)
        {
            NetStatus.ThrowIfNotOnServer();

            if (this.isLocalPlayer)
                return;

            // send message to client
            this.TargetPedStartedEnteringVehicle(this.connectionToClient, ped.gameObject, ped.CurrentVehicle.gameObject,
                ped.CurrentVehicleSeatAlignment);
        }

        [TargetRpc]
        void TargetPedStartedEnteringVehicle(NetworkConnection conn, GameObject pedGo, 
            GameObject vehicleGo, Vehicle.SeatAlignment seatAlignment)
        {
            if (null == pedGo || null == vehicleGo)
                return;

            pedGo.GetComponent<Ped>().GetStateOrLogError<VehicleEnteringState>()
                .EnterVehicle(vehicleGo.GetComponent<Vehicle>(), seatAlignment, false);
        }

        public void PedEnteredVehicle(Ped ped)
        {
            NetStatus.ThrowIfNotOnServer();

            if (this.isLocalPlayer)
                return;

            this.TargetPedEnteredVehicle(this.connectionToClient, ped.gameObject, ped.CurrentVehicle.gameObject,
                ped.CurrentVehicleSeatAlignment);
        }

        [TargetRpc]
        void TargetPedEnteredVehicle(NetworkConnection conn, GameObject pedGo, 
            GameObject vehicleGo, Vehicle.SeatAlignment seatAlignment)
        {
            if (null == pedGo || null == vehicleGo)
                return;

            pedGo.GetComponent<Ped>().GetStateOrLogError<VehicleSittingState>()
                .EnterVehicle(vehicleGo.GetComponent<Vehicle>(), seatAlignment);
        }

        public void PedStartedExitingVehicle(Ped ped)
        {
            NetStatus.ThrowIfNotOnServer();

            // send rpc to clients
            
        }

    }
}
