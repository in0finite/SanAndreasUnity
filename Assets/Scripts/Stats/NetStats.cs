using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Stats
{
    public class NetStats : MonoBehaviour
    {
        
        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "NET", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {
            
            GUILayout.Label("Time: " + NetworkTime.time);

            if (NetStatus.IsServer)
            {
                Utilities.GUIUtils.DrawHorizontalLine(1, 1, Color.black);
                GUILayout.Label("Num connections: " + NetworkServer.connections.Count);
            }

            if (NetStatus.IsClientActive())
            {
                Utilities.GUIUtils.DrawHorizontalLine(1, 1, Color.black);
                GUILayout.Label("Ping: " + NetworkTime.rtt);
                GUILayout.Label("Ping send frequency: " + NetworkTime.PingFrequency);
                GUILayout.Label("Rtt sd: " + NetworkTime.rttSd);
                GUILayout.Label("Rtt var: " + NetworkTime.rttVar);
                GUILayout.Label("Server ip: " + NetworkClient.serverIp);
                GUILayout.Label("Time since last message: " + (Time.unscaledTime - NetworkClient.connection.lastMessageTime));
            }

        }

    }
}
