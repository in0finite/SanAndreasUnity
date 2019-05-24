using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
    public partial class Ped
    {
        public NetworkTransform NetTransform { get; private set; }

        [Range(1f / 60f, 0.5f)] [SerializeField] float m_inputSendInterval = 1f / 30f;
        float m_timeSinceSentInput = 0f;

        [SyncVar(hook=nameof(Net_OnIdChanged))] int m_net_pedId = 0;
        [SyncVar(hook=nameof(Net_OnStateChanged))] string m_net_state = "";
        //[SyncVar] Weapon m_net_weapon = null;
        


        void Awake_Net()
        {
            this.NetTransform = this.GetComponentOrThrow<NetworkTransform>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (this.isServer)
                return;

            //this.PlayerModel.Load(m_net_pedId);
        }

        void Start_Net()
        {
            this.ApplySyncRate(PedManager.Instance.pedSyncRate);
        }

        public void ApplySyncRate(float newSyncRate)
        {
            float newSyncInterval = 1.0f / newSyncRate;

            foreach (var comp in this.GetComponents<NetworkBehaviour>())
                comp.syncInterval = newSyncInterval;

            // also change it for NetworkTransform, because it can be disabled
            if (this.NetTransform != null)
                this.NetTransform.syncInterval = newSyncInterval;
        }

        void Update_Net()
        {
            
            // update syncvars
            if (NetStatus.IsServer)
            {
                if (this.PedDef != null && this.PedDef.Id != m_net_pedId)
                    m_net_pedId = this.PedDef.Id;

                string newStateName = this.CurrentState != null ? this.CurrentState.GetType().Name : "";
                if (newStateName != m_net_state)
                    m_net_state = newStateName;
            }
            
            // send input to server
            if (!NetStatus.IsServer && this.IsControlledByLocalPlayer && PedSync.Local != null)
            {
                m_timeSinceSentInput += Time.unscaledDeltaTime;
                if (m_timeSinceSentInput >= m_inputSendInterval)
                {
                    m_timeSinceSentInput = 0f;
                    PedSync.Local.SendInput();
                }
            }
            
        }

        void FixedUpdate_Net()
        {
            
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
            //Debug.LogFormat("ped (net id {0}) changed state to {1}", this.netId, newStateName);

            if (this.isServer)
                return;
            
            //m_net_state = newStateName;

            if (string.IsNullOrEmpty(newStateName))
            {
                // don't do anything, this only happens when creating the ped
                return;
            }

            // forcefully change the state

            F.RunExceptionSafe( () => {
                var newState = this.States.FirstOrDefault(state => state.GetType().Name == newStateName);
                if (null == newState)
                {
                    Debug.LogErrorFormat("New ped state '{0}' could not be found", newStateName);
                }
                else
                {
                    newState.OnSwitchedStateByServer();
                }
            });

        }

    }
}
