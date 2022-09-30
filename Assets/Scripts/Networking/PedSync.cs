using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using UGameCore.Utilities;
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
            bool isJumpOn, Vector3 heading, Vector3 aimDir, Vector3 firePos, Vector3 fireDir, bool isAimOn, bool isFireOn)
        {
            this.CmdSendingInput(isWalkOn, isRunOn, isSprintOn, movementInput, isJumpOn, heading, aimDir, firePos, fireDir, isAimOn, isFireOn);
        }

        public void SendInput()
        {
            Ped ped = m_ped;
            this.SendInput(ped.IsWalkOn, ped.IsRunOn, ped.IsSprintOn, ped.Movement, ped.IsJumpOn, ped.Heading, 
                ped.AimDirection, ped.FirePosition, ped.FireDirection, ped.IsAimOn, ped.IsFireOn);
        }

        [Command]
        void CmdSendingInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, 
            bool isJumpOn, Vector3 heading, Vector3 aimDir, Vector3 firePos, Vector3 fireDir, bool isAimOn, bool isFireOn)
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
            ped.NetFirePos = firePos;
            ped.NetFireDir = fireDir;
            ped.IsAimOn = isAimOn;
            ped.IsFireOn = isFireOn;

        }

        public void SendVehicleInput(Vehicle.VehicleInput vehicleInput) => 
            this.CmdSendingVehicleInput(vehicleInput.accelerator, vehicleInput.steering, vehicleInput.isHandBrakeOn);

        [Command]
        void CmdSendingVehicleInput(float acceleration, float steering, bool isHandBrakeOn)
        {
            if (null == m_ped)
                return;

            var vehicle = m_ped.CurrentVehicle;
            if (null == vehicle)
                return;

            var input = new Vehicle.VehicleInput();
            input.accelerator = acceleration;
            input.steering = steering;
            input.isHandBrakeOn = isHandBrakeOn;
            vehicle.Input = input;
        }


        public void OnButtonPressed(string buttonName) => this.CmdOnButtonPressed(buttonName);

        [Command]
        void CmdOnButtonPressed(string buttonName)
        {
            if (m_ped != null)
                m_ped.OnButtonPressed(buttonName);
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

        public void OnNextWeaponButtonPressed() => this.CmdOnNextWeaponButtonPressed();
        
        [Command]
        void CmdOnNextWeaponButtonPressed()
        {
            if (m_ped != null)
                m_ped.OnNextWeaponButtonPressed();
        }

        public void OnPreviousWeaponButtonPressed() => this.CmdOnPreviousWeaponButtonPressed();
        
        [Command]
        void CmdOnPreviousWeaponButtonPressed()
        {
            if (m_ped != null)
                m_ped.OnPreviousWeaponButtonPressed();
        }


        public static void OnWeaponFired(Ped ped, Weapon weapon, Vector3 firePos)
        {
            foreach (var p in Player.AllPlayersEnumerable)
                p.GetComponent<PedSync>().TargetOnWeaponFired(p.connectionToClient, ped.gameObject, weapon.gameObject, firePos);
        }

        [TargetRpc]
        void TargetOnWeaponFired(NetworkConnection conn, GameObject pedGo, GameObject weaponGo, Vector3 firePos)
        {
            if (NetStatus.IsServer)
                return;
            if (null == weaponGo || null == pedGo)
                return;
            
            F.RunExceptionSafe( () => {
                var ped = pedGo.GetComponent<Ped>();
                var weapon = weaponGo.GetComponent<Weapon>();

                if (ped.CurrentState != null)
                    ped.CurrentState.OnWeaponFiredFromServer(weapon, firePos);
            });
            
        }

        public static void SendDamagedEvent(GameObject damagedPedGo, GameObject attackingPedGo, float damageAmount)
        {
            NetStatus.ThrowIfNotOnServer();

            foreach (var p in Player.AllPlayersEnumerable)
            {
                p.GetComponent<PedSync>().TargetDamagedEvent(p.connectionToClient, damagedPedGo, attackingPedGo, damageAmount);
            }
        }

        [TargetRpc]
        private void TargetDamagedEvent(NetworkConnection conn, GameObject damagedPedGo, 
            GameObject attackingPedGo, float damageAmount)
        {
            F.RunExceptionSafe(() =>
            {
                Ped damagedPed = damagedPedGo != null ? damagedPedGo.GetComponent<Ped>() : null;
                Ped attackingPed = attackingPedGo != null ? attackingPedGo.GetComponent<Ped>() : null;

                if (damagedPed != null)
                    damagedPed.OnReceivedDamageEventFromServer(damageAmount, attackingPed);
            });
        }

    }
}
