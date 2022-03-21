using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Utilities;
using System.Linq;
using System.Text;

namespace SanAndreasUnity.Net
{

    public class Player : NetworkBehaviour
    {

        static List<Player> s_allPlayers = new List<Player>();
        public static Player[] AllPlayersCopy => s_allPlayers.ToArray();
        public static IEnumerable<Player> AllPlayersEnumerable => s_allPlayers;
        public static IReadOnlyList<Player> AllPlayersList => s_allPlayers;

        /// <summary>Local player.</summary>
        public static Player Local { get; private set; }

        public static event System.Action<Player> onStart = delegate {};
        public static event System.Action<Player> onDisable = delegate {};

        [SyncVar(hook=nameof(OnOwnedGameObjectChanged))] GameObject m_ownedGameObject;
        Ped m_ownedPed;
        //public GameObject OwnedGameObject { get { return m_ownedGameObject; } internal set { m_ownedGameObject = value; } }
        public Ped OwnedPed { get { return m_ownedPed; } internal set { m_ownedPed = value; m_ownedGameObject = value != null ? value.gameObject : null; } }

        [SyncVar]
        private string m_net_playerName = string.Empty;
        public string PlayerName
        {
            get => m_net_playerName;
            private set => m_net_playerName = value;
        }

        public static readonly int MaxPlayerNameLength = 20;
        public static readonly int MinPlayerNameLength = 2;
        public static readonly string DefaultPlayerName = "Player";

        public string DescriptionForLogging => "(netId=" + this.netId + ", addr=" + (this.connectionToClient != null ? this.connectionToClient.address : "") + ")";

        private readonly SyncedBag.StringSyncDictionary m_syncDictionary = new SyncedBag.StringSyncDictionary();
        public SyncedBag ExtraData { get; }


        Player()
        {
            ExtraData = new SyncedBag(m_syncDictionary);
        }

        public static Player GetOwningPlayer(Ped ped)
        {
            if (null == ped)
                return null;
            return AllPlayersEnumerable.FirstOrDefault(p => p.OwnedPed == ped);
        }

        private void Awake()
        {
            if (NetStatus.IsServer)
            {
                // assign default player name
                this.PlayerName = GeneratePlayerName(DefaultPlayerName);
            }
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

            F.InvokeEventExceptionSafe(onDisable, this);

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
            if (!this.isLocalPlayer)
                Debug.LogFormat("Player {0} connected, time: {1}", this.DescriptionForLogging, F.CurrentDateForLogging);

            F.InvokeEventExceptionSafe(onStart, this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            
            // log some info about this
            if (!this.isLocalPlayer)
                Debug.LogFormat("Player {0} disconnected, time: {1}", this.DescriptionForLogging, F.CurrentDateForLogging);
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
                    Debug.LogFormat("Detected dead connection for player {0}", this.DescriptionForLogging);
                    this.connectionToClient.Disconnect();
                }
            }

        }

        public void Disconnect()
        {
            this.connectionToClient.Disconnect();
        }

        public static Player GetByNetId(uint netId)
        {
            var go = NetManager.GetNetworkObjectById(netId);
            return go != null ? go.GetComponent<Player>() : null;
        }

        public static Player GetByName(string name)
        {
            return s_allPlayers.Find(p => p.PlayerName == name);
        }

        public static string ValidatePlayerName(string playerName)
        {
            if (null == playerName)
                return DefaultPlayerName;

            if (playerName.Length < MinPlayerNameLength)
                return DefaultPlayerName;

            if (playerName.Length > MaxPlayerNameLength)
                playerName = playerName.Substring(0, MaxPlayerNameLength);

            var sb = new StringBuilder(playerName);

            // remove tags
            sb.Replace("<", "< "); // the easiest way

            sb.Replace('\r', ' ');
            sb.Replace('\n', ' ');
            sb.Replace('\t', ' ');

            // trim has to be done after other operations, because they can produce whitespaces
            playerName = sb.ToString().Trim();

            if (playerName.Length < MinPlayerNameLength)
                return DefaultPlayerName;

            return playerName;
        }

        public static string GeneratePlayerName(string playerName)
        {
            playerName = ValidatePlayerName(playerName);

            // check if player with this name already exists

            if (GetByName(playerName) == null)
                return playerName;

            for (int i = 1; i < 10000; i++)
            {
                string newName = playerName + " (" + i + ")";
                if (null == GetByName(newName))
                    return newName;
            }

            throw new Exception("Failed to generate player name");
        }

        public void RequestNameChange(string newName) => this.CmdRequestNameChange(newName);

        [Command]
        private void CmdRequestNameChange(string newName)
        {
            this.PlayerName = GeneratePlayerName(newName);
        }

    }

}
