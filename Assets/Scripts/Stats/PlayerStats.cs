using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Stats
{
    public class PlayerStats : MonoBehaviour
    {
        float[] m_widths = new float[]{0.25f, 0.1f, 0.1f, 0.25f, 0.15f, 0.1f};
        string[] m_columnNames = new string[]{"Address", "Net id", "Ped net id", "Ped model", "Health", "Ping"};


        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "PLAYERS", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {
            
            bool isServer = NetStatus.IsServer;

            // columns
            GUILayout.BeginHorizontal();
            for (int i=0; i < m_columnNames.Length; i++)
                GUILayout.Button(m_columnNames[i], GUILayout.Width(GetWidth(i)));
            GUILayout.EndHorizontal();

            foreach (var p in Player.AllPlayersEnumerable)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(isServer ? p.connectionToClient.address : "", GUILayout.Width(GetWidth(0)));
                GUILayout.Label(p.netId.ToString(), GUILayout.Width(GetWidth(1)));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.netId.ToString() : "", GUILayout.Width(GetWidth(2)));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.PedDef != null ? p.OwnedPed.PedDef.ModelName : "", GUILayout.Width(GetWidth(3)));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.Health.ToString() : "", GUILayout.Width(GetWidth(4)));
                GUILayout.Label("", GUILayout.Width(GetWidth(5)));

                GUILayout.EndHorizontal();
            }

        }

        float GetWidth(int index) => m_widths[index] * Utilities.Stats.DisplayRect.width;

    }
}
