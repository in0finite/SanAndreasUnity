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

        [SyncVar] GameObject m_net_playerOwnerGameObject;
        internal GameObject NetPlayerOwnerGameObject { set { m_net_playerOwnerGameObject = value; } }
        public Player PlayerOwner => Player.GetOwningPlayer(this);

        [SyncVar(hook=nameof(Net_OnIdChanged))] int m_net_pedId = 0;

        struct StateSyncData
        {
            public string state;
            public string additionalData;
        }
        //[SyncVar(hook=nameof(Net_OnStateChanged))] StateSyncData m_net_stateData;
        [SyncVar] string m_net_additionalStateData = "";
        [SyncVar(hook=nameof(Net_OnStateChanged))] string m_net_state = "";
        //[SyncVar] Weapon m_net_weapon = null;
        
        public static int NumStateChangesReceived { get; private set; }

        public class SyncDictionaryStringUint : Mirror.SyncDictionary<string, uint> { }

        public SyncDictionaryStringUint syncDictionaryStringUint = new SyncDictionaryStringUint();

        [SyncVar] Vector3 m_net_movementInput;
        [SyncVar] Vector3 m_net_heading;

        [SyncVar] float m_net_health;

        //[SyncVar(hook=nameof(Net_OnWeaponChanged))] GameObject m_net_weaponGameObject;
        [SyncVar(hook=nameof(Net_OnWeaponChanged))] int m_net_currentWeaponSlot;

        [SyncVar] internal Vector3 m_net_aimDir;

        public Vector3 NetFirePos { get; set; }
        public Vector3 NetFireDir { get; set; }



        void Awake_Net()
        {
            this.NetTransform = this.GetComponentOrThrow<NetworkTransform>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (this.isServer)
                return;

            // owner player sync var should point to created game object, because player's game object is always created before
            // ped's game object
            // assign var in Player script
            if (m_net_playerOwnerGameObject != null)
                m_net_playerOwnerGameObject.GetComponent<Player>().OwnedPed = this;

            this.TryToLoadNewModel(m_net_pedId);

            // switch weapon
            F.RunExceptionSafe( () => this.WeaponHolder.SwitchWeapon(m_net_currentWeaponSlot) );

            this.ChangeStateBasedOnSyncData(new StateSyncData(){state = m_net_state, additionalData = m_net_additionalStateData});
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
            
            if (NetStatus.IsServer)
            {
                // update syncvars

                if (this.PedDef != null && this.PedDef.Id != m_net_pedId)
                    m_net_pedId = this.PedDef.Id;

                string newStateName = this.CurrentState != null ? this.CurrentState.GetType().Name : "";
                if (newStateName != m_net_state)
                {
                    // state changed

                    //Debug.LogFormat("Updating state syncvar - ped {0}, new state {1}, old state {2}", this.netId, newStateName, m_net_state);

                    //m_net_stateData = new StateSyncData();

                    // obtain additional data from state
                    byte[] data = this.CurrentState != null ? this.CurrentState.GetAdditionalNetworkData() : null;
                    // assign additional data
                    m_net_additionalStateData = data != null ? System.Convert.ToBase64String(data) : "";
                    // assign new state
                    m_net_state = newStateName;
                }

                if (m_net_movementInput != this.Movement)
                    m_net_movementInput = this.Movement;

                if (m_net_heading != this.Heading)
                    m_net_heading = this.Heading;

                if (m_net_health != this.Health)
                    m_net_health = this.Health;

                Vector3 aimDir = this.AimDirection;
                if (m_net_aimDir != aimDir)
                    m_net_aimDir = aimDir;

                if (this.WeaponHolder.CurrentWeaponSlot != m_net_currentWeaponSlot)
                {
                    m_net_currentWeaponSlot = this.WeaponHolder.CurrentWeaponSlot;
                }

            }
            else
            {
                // apply syncvars

                if (!this.IsControlledByLocalPlayer)
                {
                    this.Movement = m_net_movementInput;
                    this.Heading = m_net_heading;
                }

                this.Health = m_net_health;

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

        void TryToLoadNewModel(int newId)
        {

            if (this.PedDef != null && this.PedDef.Id == newId) // same id
                return;

            if (newId > 0)
                F.RunExceptionSafe( () => this.PlayerModel.Load(newId) );
            
        }

        void Net_OnIdChanged(int newId)
        {
            //Debug.LogFormat("ped (net id {0}) changed model id to {1}", this.netId, newId);
            
            if (this.isServer)
                return;
            
            this.TryToLoadNewModel(newId);
        }

        void Net_OnStateChanged(string newStateName)
        {
            if (this.isServer)
                return;

            StateSyncData newStateData = new StateSyncData(){state = newStateName, additionalData = m_net_additionalStateData};

            //Debug.LogFormat("Net_OnStateChanged(): ped {0} changed state to {1}", this.netId, newStateData.state);

            NumStateChangesReceived ++;

            this.ChangeStateBasedOnSyncData(newStateData);

        }

        void ChangeStateBasedOnSyncData(StateSyncData newStateData)
        {

            if (string.IsNullOrEmpty(newStateData.state))
            {
                // don't do anything, this only happens when creating the ped
                return;
            }

            // forcefully change the state

            F.RunExceptionSafe( () => {
                var newState = this.States.FirstOrDefault(state => state.GetType().Name == newStateData.state);
                if (null == newState)
                {
                    Debug.LogErrorFormat("New ped state '{0}' could not be found", newStateData.state);
                }
                else
                {
                    //Debug.LogFormat("Switching state based on sync data - ped: {0}, state: {1}", this.netId, newState.GetType().Name);
                    byte[] data = string.IsNullOrEmpty(newStateData.additionalData) ? null : System.Convert.FromBase64String(newStateData.additionalData);
                    newState.OnSwitchedStateByServer(data);
                }
            });
            
        }

        void Net_OnWeaponChanged(int newSlot)
        {

            if (NetStatus.IsServer)
                return;

            F.RunExceptionSafe( () => {

                //Debug.LogFormat("weapon slot changed for ped {0} to {1}", this.DescriptionForLogging, newSlot);

                if (this.CurrentState != null)
                {
                    //this.CurrentState.OnChangedWeaponByServer(newWeaponGameObject != null ? newWeaponGameObject.GetComponent<Weapon>() : null);
                    this.CurrentState.OnChangedWeaponByServer(newSlot);
                }

            });
            
        }

    }
}
