using Newtonsoft.Json;
using UGameCore.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class MasterServerClient : MonoBehaviour
    {
        private string _masterServerUrl;
        private ServerInfo _serverInfo;
        private bool _updating;
        public static MasterServerClient Instance { get; private set; }
        private HttpClient _client;

        private const int RegisterInterval = 20;

        public bool IsServerRegistrationEnabled { get; set; } = true;



        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _client = new HttpClient();
            _client.Timeout = System.TimeSpan.FromSeconds(7);

            _masterServerUrl = Config.GetString("master_server_url");

            if (string.IsNullOrWhiteSpace(_masterServerUrl))
                Debug.LogError("Url of master server not defined in config");

            NetManager.Instance.onServerStatusChanged += OnServerStatusChange;
        }

        private async void OnServerStatusChange()
        {
            if (!NetStatus.IsServer)
                return;

            if (string.IsNullOrWhiteSpace(_masterServerUrl))
                return;

            if (!IsServerRegistrationEnabled)
                return;

            _serverInfo = GetCurrentServerInfo();

            await RegisterServer();
        }

        private static ServerInfo GetCurrentServerInfo()
        {
            return new ServerInfo
            {
                Name = Config.GetString("server_name"),
                Port = NetManager.listenPortNumber,
                NumPlayersOnline = NetManager.numConnections,
                MaxPlayers = NetManager.maxNumPlayers,
                Map = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                Version = Application.version,
            };
        }

        private async Task RegisterServer()
        {
            await SendRequestToRegister();

            _updating = true;
            Invoke(nameof(UpdateServer), RegisterInterval);
        }

        private async Task UpdateServer()
        {
            while (_updating)
            {
                _serverInfo = GetCurrentServerInfo();

                await SendRequestToRegister();

                await Task.Delay(RegisterInterval * 1000);
            }
        }

        private async Task SendRequestToRegister()
        {
            var response = await _client.PostAsync(_masterServerUrl + "/register", new StringContent(JsonConvert.SerializeObject(_serverInfo), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                string str = await response.Content.ReadAsStringAsync();
                Debug.LogError($"Master server returned error while trying to register: {str}");
            }
        }

        private async Task UnregisterServer()
        {
            _updating = false;

            if (null == _serverInfo)
                return;

            if (string.IsNullOrWhiteSpace(_masterServerUrl))
                return;

            await _client.PostAsync(_masterServerUrl + "/unregister", new StringContent(JsonConvert.SerializeObject(_serverInfo), Encoding.UTF8, "application/json"));
        }

        public async Task<List<ServerInfo>> GetAllServers()
        {
            if (string.IsNullOrWhiteSpace(_masterServerUrl))
                return new List<ServerInfo>();

            return JsonConvert.DeserializeObject<List<ServerInfo>>(await _client.GetStringAsync(_masterServerUrl));
        }

        public async void OnDestroy()
        {
            await UnregisterServer();
        }
    }

    public class ServerInfo
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public int NumPlayersOnline { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public int MaxPlayers { get; set; }
        public string Map { get; set; }
    }
}
