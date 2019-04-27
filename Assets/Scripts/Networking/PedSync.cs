using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;

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

        public void SendInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, bool isJumpOn)
        {
            this.CmdSendingInput(isWalkOn, isRunOn, isSprintOn, movementInput, isJumpOn);
        }

        [Command]
        void CmdSendingInput(bool isWalkOn, bool isRunOn, bool isSprintOn, Vector3 movementInput, bool isJumpOn)
        {
            if (null == m_ped)
                return;

            Ped ped = m_ped;

            ped.IsWalkOn = isWalkOn;
            ped.IsRunOn = isRunOn;
            ped.IsSprintOn = isSprintOn;
            ped.Movement = movementInput;
            ped.IsJumpOn = isJumpOn;

        }

    }
}
