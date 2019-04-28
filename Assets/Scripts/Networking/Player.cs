using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Net
{

    public class Player : NetworkBehaviour
    {

        static List<Player> s_allPlayers = new List<Player>();
        public static Player[] AllPlayers { get { return s_allPlayers.ToArray(); } }

        /// <summary>Local player.</summary>
        public static Player Local { get; private set; }

        public static event System.Action<Player> onStart = delegate {};

        [SyncVar(hook=nameof(OnOwnedGameObjectChanged))] GameObject m_ownedGameObject;
        Ped m_ownedPed;
        //public GameObject OwnedGameObject { get { return m_ownedGameObject; } internal set { m_ownedGameObject = value; } }
        public Ped OwnedPed { get { return m_ownedPed; } internal set { m_ownedPed = value; m_ownedGameObject = value != null ? value.gameObject : null; } }


        void OnEnable()
        {
            s_allPlayers.Add(this);
        }

        void OnDisable()
        {
            s_allPlayers.Remove(this);

            // kill player's ped
            if (NetStatus.IsServer)
            {
                if (this.OwnedPed)
                    Destroy(this.OwnedPed.gameObject);
            }

            // log some info about this
            if (NetStatus.IsServer && !this.isLocalPlayer)
                Debug.LogFormat("Player (netId={0}, addr={1}) disconnected", this.netId, this.connectionToClient.address);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (this.isServer)
                return;
            
            m_ownedPed = m_ownedGameObject != null ? m_ownedGameObject.GetComponent<Ped>() : null;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Local = this;
        }

        void Start()
        {
            // log some info
            if (NetStatus.IsServer && !this.isLocalPlayer)
                Debug.LogFormat("Player (netId={0}, addr={1}) connected", this.netId, this.connectionToClient.address);

            F.InvokeEventExceptionSafe(onStart, this);
        }

        void OnOwnedGameObjectChanged(GameObject newGo)
        {
            Debug.LogFormat("Owned game object changed for player (net id {0})", this.netId);

            if (this.isServer)
                return;

            m_ownedGameObject = newGo;

            m_ownedPed = m_ownedGameObject != null ? m_ownedGameObject.GetComponent<Ped>() : null;
        }

    }

}
