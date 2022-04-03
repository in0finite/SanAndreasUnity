using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours.Peds;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Stats
{
    public class NetStats : MonoBehaviour
    {
        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry() { category = "NET", onGUI = OnStatGUI });
        }

        void OnStatGUI()
        {
            GUILayout.Label("Network time: " + NetworkTime.time);
            GUILayout.Label("Local network time: " + NetworkTime.localTime);

            if (NetStatus.IsServer)
            {
                Utilities.GUIUtils.DrawHorizontalLine(1, 1, Color.black);
                GUILayout.Label("Num connections: " + NetworkServer.connections.Count);
                GUILayout.Label("Max num players: " + NetManager.maxNumPlayers);
                GUILayout.Label($"Dead body traffic per client: {DeadBody.DeadBodies.Sum(db => db.TrafficKbps)} Kb/s");
            }

            if (NetStatus.IsClientActive())
            {
                Utilities.GUIUtils.DrawHorizontalLine(1, 1, Color.black);
                GUILayout.Label("Ping: " + (NetworkTime.rtt * 1000) + " ms");
                GUILayout.Label("Ping send frequency: " + (NetworkTime.PingFrequency * 1000) + " ms");
                GUILayout.Label("Rtt sd: " + (NetworkTime.rttStandardDeviation * 1000) + " ms");
                GUILayout.Label("Rtt var: " + (NetworkTime.rttVariance * 1000) + " ms");
                GUILayout.Label("Server ip: " + NetworkClient.serverIp);
                GUILayout.Label("Time since last message: " +
                                (Time.time - NetworkClient.connection.lastMessageTime));
            }

            GUILayout.Label($"Num spawned network objects: {NetManager.NumSpawnedNetworkObjects}");
        }
    }
}