using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SanAndreasUnity.Behaviours
{
    public partial class Ped
    {
        [SyncVar(hook=nameof(Net_OnIdChanged))] int m_net_pedId = 0;
        [SyncVar(hook=nameof(Net_OnStateChanged))] string m_net_state = "";
        //[SyncVar] Weapon m_net_weapon = null;
        


        void Awake_Net()
        {

        }

        void Start_Net()
        {

        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (this.isServer)
                return;

            this.PlayerModel.Load(m_net_pedId);
        }

        void Update_Net()
        {
            m_net_pedId = this.PedDef.Id;
            m_net_state = this.CurrentState != null ? this.CurrentState.GetType().Name : "";
            //m_net_weapon = this.CurrentWeapon;
        }

        void Net_OnIdChanged(int newId)
        {
            if (this.isServer)
                return;
            
            this.PlayerModel.Load(newId);
        }

        void Net_OnStateChanged(string newState)
        {
            if (this.isServer)
                return;
            

        }

    }
}
