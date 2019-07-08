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

        public void SendInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, 
            bool isJumpOn, Vector3 heading, Vector3 aimDir, bool isAimOn, bool isFireOn)
        {
            this.CmdSendingInput(isWalkOn, isRunOn, isSprintOn, movementInput, isJumpOn, heading, aimDir, isAimOn, isFireOn);
        }

        public void SendInput()
        {
            Ped ped = m_ped;
            this.SendInput(ped.IsWalkOn, ped.IsRunOn, ped.IsSprintOn, ped.Movement, ped.IsJumpOn, ped.Heading, 
                ped.AimDirection, ped.IsAimOn, ped.IsFireOn);
        }

        [Command]
        void CmdSendingInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, 
            bool isJumpOn, Vector3 heading, Vector3 aimDir, bool isAimOn, bool isFireOn)
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
            ped.AimDirection = aimDir;
            ped.IsAimOn = isAimOn;
            ped.IsFireOn = isFireOn;

        }

        public void SendVehicleInput(float acceleration, float steering, float braking) => 
            this.CmdSendingVehicleInput(acceleration, steering, braking);

        [Command]
        void CmdSendingVehicleInput(float acceleration, float steering, float braking)
        {
            if (null == m_ped || null == m_ped.CurrentVehicle)
                return;

            var v = m_ped.CurrentVehicle;
            v.Accelerator = acceleration;
            v.Steering = steering;
            v.Braking = braking;
        }


        public void OnCrouchButtonPressed() => this.CmdOnCrouchButtonPressed();
        
        [Command]
        void CmdOnCrouchButtonPressed()
        {
            if (m_ped)
                m_ped.OnCrouchButtonPressed();
        }

        public void OnSubmitButtonPressed() => this.CmdOnSubmitButtonPressed();
        
        [Command]
        void CmdOnSubmitButtonPressed()
        {
            if (m_ped)
                m_ped.OnSubmitPressed();
        }

        public void OnFireButtonPressed() => this.CmdOnFireButtonPressed();
        
        [Command]
        void CmdOnFireButtonPressed()
        {
            if (m_ped != null)
                m_ped.OnFireButtonPressed();
        }

        public void OnAimButtonPressed() => this.CmdOnAimButtonPressed();
        
        [Command]
        void CmdOnAimButtonPressed()
        {
            if (m_ped != null)
                m_ped.OnAimButtonPressed();
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

    }
}
