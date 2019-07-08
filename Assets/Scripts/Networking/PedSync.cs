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

    }
}
