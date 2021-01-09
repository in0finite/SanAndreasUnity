using Newtonsoft.Json;
using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MasterServerClient : MonoBehaviour
{
    private string _masterServerUrl;
    private ServerInfo _serverInfo;
    private bool _updating;
    public static MasterServerClient Instance { get; private set; }
    private HttpClient _client;

    // Start is called before the first frame update
    private void Start()
    {
        Instance = this;

        _client = new HttpClient();

        Config.Load();
        _masterServerUrl = Config.Get<string>("masterserverurl");

        NetManager.Instance.onServerStatusChanged += OnServerStatusChange;
    }

    private async void OnServerStatusChange()
    {
        if (!NetStatus.IsServer) return;

        if (NetStatus.serverStatus == NetworkServerStatus.Started)
        {
            _serverInfo = new ServerInfo()
            {
                Name = Config.Get<string>("name"),
                IP = new WebClient().DownloadString("http://icanhazip.com").Trim(),
                Port = NetManager.listenPortNumber,
                NumPlayersOnline = Mirror.NetworkManager.singleton.numPlayers,
                MaxPlayers = NetManager.maxNumPlayers,
            };
            await RegisterServer();
        }
    }

    private async Task RegisterServer()
    {
        var res = await _client.PostAsync(_masterServerUrl + "/register", new StringContent(JsonConvert.SerializeObject(_serverInfo), Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            return;
        }

        _updating = true;
        Invoke(nameof(UpdateServer), 5);
    }

    private async Task UpdateServer()
    {
        while (_updating)
        {
            _serverInfo.NumPlayersOnline = Mirror.NetworkManager.singleton.numPlayers;

            var res = await _client.PostAsync(_masterServerUrl + "/register", new StringContent(JsonConvert.SerializeObject(_serverInfo), Encoding.UTF8, "application/json"));
            if (!res.IsSuccessStatusCode) _updating = false;
            await Task.Delay(5000);
        }
    }

    private async Task UnregisterServer()
    {
        await _client.PostAsync(_masterServerUrl + "/unregister", new StringContent(JsonConvert.SerializeObject(_serverInfo), Encoding.UTF8, "application/json"));
        _updating = false;
    }

    public async Task<List<ServerInfo>> GetAllServers()
    {
        return JsonConvert.DeserializeObject<List<ServerInfo>>(await _client.GetStringAsync(_masterServerUrl));
    }

    public async void OnDestroy()
    {
        if (_serverInfo != null)
            await UnregisterServer();
    }
}

public class ServerInfo
{
    public string Name { get; set; }
    public int NumPlayersOnline { get; set; }
    public string IP { get; set; }
    public int Port { get; set; }
    public int MaxPlayers { get; set; }
}