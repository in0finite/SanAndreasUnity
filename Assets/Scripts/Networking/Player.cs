using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Net
{

    public class Player : NetworkBehaviour
    {

        static List<Player> s_allPlayers = new List<Player>();
        public static Player[] AllPlayers { get { return s_allPlayers.ToArray(); } }
        public static IEnumerable<Player> AllPlayersEnumerable { get { return s_allPlayers; } }

        /// <summary>Local player.</summary>
        public static Player Local { get; private set; }

        public static event System.Action<Player> onStart = delegate {};

        [SyncVar(hook=nameof(OnOwnedGameObjectChanged))] GameObject m_ownedGameObject;
        Ped m_ownedPed;
        //public GameObject OwnedGameObject { get { return m_ownedGameObject; } internal set { m_ownedGameObject = value; } }
        public Ped OwnedPed { get { return m_ownedPed; } internal set { m_ownedPed = value; m_ownedGameObject = value != null ? value.gameObject : null; } }



        public static Player GetOwningPlayer(Ped ped)
        {
            if (null == ped)
                return null;
            return AllPlayersEnumerable.FirstOrDefault(p => p.OwnedPed == ped);
        }

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
                Debug.LogFormat("Player (netId={0}, addr={1}) connected, time: {2}", this.netId, this.connectionToClient.address, F.CurrentDateForLogging);

            F.InvokeEventExceptionSafe(onStart, this);
        }

        public override void OnNetworkDestroy()
        {
            base.OnNetworkDestroy();
            
            // log some info about this
            if (NetStatus.IsServer && !this.isLocalPlayer)
                Debug.LogFormat("Player (netId={0}, addr={1}) disconnected, time: {2}", this.netId, this.connectionToClient.address, F.CurrentDateForLogging);
        }

        void OnOwnedGameObjectChanged(GameObject newGo)
        {
            Debug.LogFormat("Owned game object changed for player (net id {0})", this.netId);

            if (this.isServer)
                return;

            m_ownedGameObject = newGo;

            m_ownedPed = m_ownedGameObject != null ? m_ownedGameObject.GetComponent<Ped>() : null;
        }

        void Update()
        {

            // Telepathy does not detect dead connections, so we'll have to detect them ourselves
            if (NetStatus.IsServer && !this.isLocalPlayer)
            {
                if (Time.time - this.connectionToClient.lastMessageTime > 6f)
                {
                    // disconnect client
                    this.connectionToClient.Disconnect();
                }
            }

        }

    }

}
