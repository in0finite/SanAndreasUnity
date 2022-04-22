using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanAndreasUnity.Utilities
{
    public class Stats
    {
        public class Entry
        {
            public string category = "";
            public string text = null;
            public System.Action<GetStatsContext> getStatsAction = null;
        }

        public class GetStatsContext
        {
            /// <summary>
            /// This is where the stats should be stored.
            /// </summary>
            public readonly StringBuilder stringBuilder = new StringBuilder();

            /// <summary>
            /// If true, stats can be drawn using imGui, for slightly nicer output.
            /// </summary>
            public readonly bool isOnGui = false;

            public GetStatsContext()
            {
            }

            public GetStatsContext(bool isOnGui)
            {
                this.isOnGui = isOnGui;
            }

            public void AppendLine(string text) => this.stringBuilder.AppendLine(text);
            public void AppendLine() => this.stringBuilder.AppendLine();
            public void Append(string text) => this.stringBuilder.Append(text);
        }

        static Dictionary<string, List<Entry>> s_entries = new Dictionary<string, List<Entry>>();
        public static IEnumerable<KeyValuePair<string, List<Entry>>> Entries => s_entries;
        public static IEnumerable<string> Categories => s_entries.Select(pair => pair.Key);

        public static UnityEngine.Rect DisplayRect { get; set; }


        public static void RegisterStat(Entry entry)
        {
            if (s_entries.ContainsKey(entry.category))
                s_entries[entry.category].Add(entry);
            else
                s_entries[entry.category] = new List<Entry>(){entry};
        }

    }
}
