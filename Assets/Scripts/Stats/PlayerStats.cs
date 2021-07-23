using System.Linq;
using UnityEngine;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Stats
{
    public class PlayerStats : MonoBehaviour
    {
        [SerializeField] float[] m_widths = new float[]{150, 110, 50, 70, 80, 150, 50, 80};
        [SerializeField] string[] m_columnNames = new string[]{"Name", "Address", "Net id", "Ped net id", "Ped model", "Ped state", "Health", "Weapon"};
        int m_currentIndex = 0;

        float[] m_currentWidths = new float[0];
        string[] m_currentColumnNames = new string[0];

        public const string ColumnDataKeysKey = "player_stats_column_data_keys";
        public const string ColumnNamesKey = "player_stats_column_names";
        public const string ColumnWidthsKey = "player_stats_column_widths";


        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "PLAYERS", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {
            
            bool isServer = NetStatus.IsServer;

            string[] dataKeys = SyncedServerData.Data.GetStringArray(ColumnDataKeysKey) ?? new string[0];

            m_currentColumnNames = m_columnNames.Concat(SyncedServerData.Data.GetStringArray(ColumnNamesKey) ?? new string[0]).ToArray();
            m_currentWidths = m_widths.Concat(SyncedServerData.Data.GetFloatArray(ColumnWidthsKey) ?? new float[0]).ToArray();

            if (m_currentColumnNames.Length != m_currentWidths.Length)
                return;

            if (dataKeys.Length + m_widths.Length != m_currentWidths.Length)
                return;

            // columns
            GUILayout.BeginHorizontal();
            m_currentIndex = 0;
            for (int i=0; i < m_currentColumnNames.Length; i++)
                GUILayout.Button(m_currentColumnNames[i], GUILayout.Width(GetWidth()));
            GUILayout.EndHorizontal();

            foreach (var p in Player.AllPlayersEnumerable)
            {
                GUILayout.BeginHorizontal();

                m_currentIndex = 0;
                GUILayout.Label(p.PlayerName, GUILayout.Width(GetWidth()));
                GUILayout.Label(isServer ? p.connectionToClient.address : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.netId.ToString(), GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.netId.ToString() : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.PedDef != null ? p.OwnedPed.PedDef.ModelName : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.CurrentState != null ? p.OwnedPed.CurrentState.GetType().Name : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null ? p.OwnedPed.Health.ToString() : "", GUILayout.Width(GetWidth()));
                GUILayout.Label(p.OwnedPed != null && p.OwnedPed.CurrentWeapon != null && p.OwnedPed.CurrentWeapon.Definition != null ? p.OwnedPed.CurrentWeapon.Definition.ModelName : "", GUILayout.Width(GetWidth()));

                foreach (string dataKey in dataKeys)
                {
                    string data = p.ExtraData.GetString(dataKey);
                    GUILayout.Label(data, GUILayout.Width(GetWidth()));
                }

                GUILayout.EndHorizontal();
            }

        }

        float GetWidth() => m_currentWidths[m_currentIndex++];

    }
}
