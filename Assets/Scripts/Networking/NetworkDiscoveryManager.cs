using UnityEngine;
using UGameCore.Utilities;
using NetworkDiscoveryUnity;

namespace SanAndreasUnity.Net
{
    public class NetworkDiscoveryManager : StartupSingleton<NetworkDiscoveryManager>
    {
        public const string NumPlayersKey = "NumPlayers";
        public const string MaxNumPlayersKey = "MaxNumPlayers";

        public NetworkDiscovery NetworkDiscovery { get; private set; }


        protected override void OnSingletonAwake()
        {
            this.NetworkDiscovery = this.GetComponentOrThrow<NetworkDiscovery>();
        }

        protected override void OnSingletonStart()
        {
            NetManager.Instance.onServerStatusChanged -= OnServerStatusChanged;
            NetManager.Instance.onServerStatusChanged += OnServerStatusChanged;

            Player.onStart -= OnPlayerStart;
            Player.onStart += OnPlayerStart;

            Player.onDisable -= OnPlayerDisable;
            Player.onDisable += OnPlayerDisable;

            this.AssignPlayerCounts();
        }

        private void OnServerStatusChanged()
        {
            if (NetStatus.IsServer)
            {
                this.AssignPlayerCounts();
                this.NetworkDiscovery.EnsureServerIsInitialized();
            }
            else
                this.NetworkDiscovery.CloseServerUdpClient();
        }

        private void OnPlayerStart(Player player)
        {
            this.AssignPlayerCounts();
        }

        private void OnPlayerDisable(Player player)
        {
            this.AssignPlayerCounts();
        }

        private void AssignPlayerCounts()
        {
            this.NetworkDiscovery.RegisterResponseData(NumPlayersKey, Player.AllPlayersList.Count.ToString());
            this.NetworkDiscovery.RegisterResponseData(MaxNumPlayersKey, NetManager.maxNumPlayers.ToString());
        }
    }
}
