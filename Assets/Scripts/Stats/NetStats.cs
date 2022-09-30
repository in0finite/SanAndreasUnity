using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours.Peds;
using SanAndreasUnity.Net;
using System;
using UGameCore.Utilities;

namespace SanAndreasUnity.Stats
{
    public class NetStats : MonoBehaviour
    {
        void Start()
        {
            UGameCore.Utilities.Stats.RegisterStat(new UGameCore.Utilities.Stats.Entry() { category = "NET", getStatsAction = GetStats });
        }

        void GetStats(UGameCore.Utilities.Stats.GetStatsContext context)
        {
            var sb = context.stringBuilder;

            AddTimeSpan(sb, "Network time", NetworkTime.time);
            AddTimeSpan(sb, "Local network time", NetworkTime.localTime);
            AddTimeSpan(sb, "Diff between network times", NetworkTime.time - NetworkTime.localTime, true);

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
        }

        private static void AddTimeSpan(System.Text.StringBuilder sb, string text, double seconds, bool useMilliseconds = false)
        {
            sb.AppendLine($"{text}: {F.FormatElapsedTime(seconds, useMilliseconds)}");
        }

        private static void AddAsMs(System.Text.StringBuilder sb, string text, double seconds)
        {
            sb.AppendLine($"{text}: {seconds * 1000:0.00} ms");
        }
    }
}