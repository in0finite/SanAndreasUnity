using System.Collections.Generic;

namespace SanAndreasUnity.UI
{

    public class MenuEntry
    {
        public string name = "";
        public int sortPriority = 0;
        public List<MenuEntry> children = new List<MenuEntry>();
        public System.Action drawAction = null;
        public System.Action clickAction = null;

        public int AddChild(MenuEntry entry)
        {
            int index = this.children.FindIndex(e => e.sortPriority > entry.sortPriority);

            if (index < 0)
            {
                this.children.Add(entry);
                return 0;
            }
            else
            {
                this.children.Insert(index, entry);
                return index;
            }

        }

    }

}
