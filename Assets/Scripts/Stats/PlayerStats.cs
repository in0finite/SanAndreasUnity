using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Stats
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] float[] m_widths = new float[]{110, 50, 70, 80, 150, 50};
        [SerializeField] string[] m_columnNames = new string[]{"Address", "Net id", "Ped net id", "Ped model", "Ped state", "Health"};
        int m_currentIndex = 0;


        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "PLAYERS", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {
            
            bool isServer = NetStatus.IsServer;

            // columns
            GUILayout.BeginHorizontal();
            m_currentIndex = 0;
            for (int i=0; i < m_columnNames.Length; i++)
                GUILayout.Button(m_columnNames[i], GUILayout.Width(GetWidth()));
            GUILayout.EndHorizontal();

            foreach (var p in Player.AllPlayersEnumerable)
            {
                GUILayout.BeginHorizontal();

                m_currentIndex = 0;
                GUILayout.Label(isServer ? p.connectionToClient.address : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.netId.ToString(), GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.netId.ToString() : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.PedDef != null ? p.OwnedPed.PedDef.ModelName : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.CurrentState != null ? p.OwnedPed.CurrentState.GetType().Name : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.Health.ToString() : "", GUILayout.Width(GetWidth()));

                GUILayout.EndHorizontal();
            }

        }

        float GetWidth() => m_widths[m_currentIndex++];

    }
}
