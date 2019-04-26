using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using SanAndreasUnity.Utilities;

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

            //this.PlayerModel.Load(m_net_pedId);
        }

        void Update_Net()
        {
            if (!this.isServer)
                return;

            if (this.PedDef != null && this.PedDef.Id != m_net_pedId)
                m_net_pedId = this.PedDef.Id;

            string newStateName = this.CurrentState != null ? this.CurrentState.GetType().Name : "";
            if (newStateName != m_net_state)
                m_net_state = newStateName;
            
            //m_net_weapon = this.CurrentWeapon;
        }

        void Net_OnIdChanged(int newId)
        {
            Debug.LogFormat("ped (net id {0}) changed model id to {1}", this.netId, newId);
            
            if (this.isServer)
                return;
            
            //m_net_pedId = newId;

            if (newId > 0)
                F.RunExceptionSafe( () => this.PlayerModel.Load(newId) );
        }

        void Net_OnStateChanged(string newStateName)
        {
            Debug.LogFormat("ped (net id {0}) changed state to {1}", this.netId, newStateName);

            if (this.isServer)
                return;
            
            //m_net_state = newStateName;

            // forcefully change the state

            F.RunExceptionSafe( () => {
                var newState = this.States.FirstOrDefault(state => state.GetType().Name == newStateName);
                if (null == newState)
                {
                    Debug.LogErrorFormat("New ped state '{0}' could not be found", newStateName);
                }
                else
                {
                    this.SwitchState(newState.GetType());
                }
            });

        }

    }
}
