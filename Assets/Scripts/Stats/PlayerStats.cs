using System.Linq;
using UnityEngine;
using SanAndreasUnity.Net;
using System.Collections.Generic;

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

        private readonly List<string> m_textsForPlayer = new List<string>();


        void Start()
        {
            UGameCore.Utilities.Stats.RegisterStat(new UGameCore.Utilities.Stats.Entry(){category = "PLAYERS", getStatsAction = GetStats});
        }

        void GetStats(UGameCore.Utilities.Stats.GetStatsContext context)
        {

            var sb = context.stringBuilder;
            bool isServer = NetStatus.IsServer;

            string[] dataKeys = SyncedServerData.Data.GetStringArray(ColumnDataKeysKey) ?? new string[0];

            m_currentColumnNames = m_columnNames.Concat(SyncedServerData.Data.GetStringArray(ColumnNamesKey) ?? new string[0]).ToArray();
            m_currentWidths = m_widths.Concat(SyncedServerData.Data.GetFloatArray(ColumnWidthsKey) ?? new float[0]).ToArray();

            if (m_currentColumnNames.Length != m_currentWidths.Length)
                return;

            if (dataKeys.Length + m_widths.Length != m_currentWidths.Length)
                return;

            // columns

            if (context.isOnGui)
                GUILayout.BeginHorizontal();

            m_currentIndex = 0;
            for (int i=0; i < m_currentColumnNames.Length; i++)
            {
                if (context.isOnGui)
                    GUILayout.Button(m_currentColumnNames[i], GUILayout.Width(GetWidth()));
                else
                    sb.Append(m_currentColumnNames[i].PadRight(GetWidthForText()));
            }

            if (context.isOnGui)
                GUILayout.EndHorizontal();
            else
                sb.AppendLine();

            foreach (var p in Player.AllPlayersEnumerable)
            {
                if (context.isOnGui)
                    GUILayout.BeginHorizontal();

                m_currentIndex = 0;
                m_textsForPlayer.Clear();

                m_textsForPlayer.Add(p.PlayerName);
                m_textsForPlayer.Add(p.CachedIpAddress);
                m_textsForPlayer.Add(p.netId.ToString());
                m_textsForPlayer.Add(p.OwnedPed != null ? p.OwnedPed.netId.ToString() : "");
                m_textsForPlayer.Add(p.OwnedPed != null && p.OwnedPed.PedDef != null ? p.OwnedPed.PedDef.ModelName : "");
                m_textsForPlayer.Add(p.OwnedPed != null && p.OwnedPed.CurrentState != null ? p.OwnedPed.CurrentState.GetType().Name : "");
                m_textsForPlayer.Add(p.OwnedPed != null ? p.OwnedPed.Health.ToString() : "");
                m_textsForPlayer.Add(p.OwnedPed != null && p.OwnedPed.CurrentWeapon != null && p.OwnedPed.CurrentWeapon.Definition != null ? p.OwnedPed.CurrentWeapon.Definition.ModelName : "");

                foreach (string dataKey in dataKeys)
                {
                    string data = p.ExtraData.GetString(dataKey) ?? string.Empty;
                    m_textsForPlayer.Add(data);
                }

                foreach (string text in m_textsForPlayer)
                {
                    if (context.isOnGui)
                        GUILayout.Label(text, GUILayout.Width(GetWidth()));
                    else
                        sb.Append(text.PadRight(GetWidthForText()));
                }

                if (context.isOnGui)
                    GUILayout.EndHorizontal();
                else
                    sb.AppendLine();
            }

        }

        float GetWidth() => m_currentWidths[m_currentIndex++];

        int GetWidthForText() => Mathf.RoundToInt(this.GetWidth() / 3f);

    }
}
