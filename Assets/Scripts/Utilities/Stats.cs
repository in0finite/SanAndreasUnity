using System.Collections.Generic;
using System.Linq;

namespace SanAndreasUnity.Utilities
{
    public class Stats
    {
        public class Entry
        {
            public string category = "";
            public string text = null;
            public System.Action onGUI = null;
        }

        static Dictionary<string, List<Entry>> s_entries = new Dictionary<string, List<Entry>>();
        public static IEnumerable<KeyValuePair<string, List<Entry>>> Entries => s_entries;
        public static IEnumerable<string> Categories => s_entries.Select(pair => pair.Key);


        public static void RegisterStat(Entry entry)
        {
            if (s_entries.ContainsKey(entry.category))
                s_entries[entry.category].Add(entry);
            else
                s_entries[entry.category] = new List<Entry>(){entry};
        }

    }
}
