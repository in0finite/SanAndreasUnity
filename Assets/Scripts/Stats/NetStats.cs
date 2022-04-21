using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours.Peds;
using SanAndreasUnity.Net;
using System;

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
            var sb = new System.Text.StringBuilder();

            AddTimeSpan(sb, "Network time", NetworkTime.time);
            AddTimeSpan(sb, "Local network time", NetworkTime.localTime);

            if (NetStatus.IsServer)
            {
                sb.AppendLine("-----------------------");
                sb.AppendLine("Num connections: " + NetworkServer.connections.Count);
                sb.AppendLine("Max num players: " + NetManager.maxNumPlayers);
                sb.AppendLine($"Dead body traffic per client: {DeadBody.DeadBodies.Sum(db => db.TrafficKbps)} Kb/s");
            }

            if (NetStatus.IsClientActive())
            {
                sb.AppendLine("-----------------------");
                AddAsMs(sb, "Ping", NetworkTime.rtt);
                AddAsMs(sb, "Ping send frequency", NetworkTime.PingFrequency);
                AddAsMs(sb, "Rtt sd", NetworkTime.rttStandardDeviation);
                AddAsMs(sb, "Rtt var", NetworkTime.rttVariance);
                sb.AppendLine("Server ip: " + NetworkClient.serverIp);
                AddAsMs(sb, "Time since last message",
                                Time.time - NetworkClient.connection.lastMessageTime);
            }

            sb.AppendLine($"Num spawned network objects: {NetManager.NumSpawnedNetworkObjects}");

            GUILayout.Label(sb.ToString());
        }

        private static void AddTimeSpan(System.Text.StringBuilder sb, string text, double seconds)
        {
            sb.AppendLine($"{text}: {TimeSpan.FromSeconds(seconds)}");
        }

        private static void AddAsMs(System.Text.StringBuilder sb, string text, double seconds)
        {
            sb.AppendLine($"{text}: {seconds * 1000:0.00} ms");
        }
    }
}