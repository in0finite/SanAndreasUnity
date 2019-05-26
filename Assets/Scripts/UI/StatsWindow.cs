using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.UI
{

	public class StatsWindow : PauseMenuWindow
    {
		int m_tabIndex = 0;


		StatsWindow()
        {
			// set default parameters

			this.windowName = "Stats";
			this.useScrollView = true;

		}

		void Start ()
        {
			this.RegisterButtonInPauseMenu ();

			// adjust rect
			this.windowRect = Utilities.GUIUtils.GetCenteredRectPerc(new Vector2(0.8f, 0.8f));
		}


		protected override void OnWindowGUI ()
		{
            Utilities.Stats.DisplayRect = this.windowRect;
            var categories = Utilities.Stats.Categories.ToArray();
            m_tabIndex = GUIUtils.TabsControl(m_tabIndex, categories);
            if (m_tabIndex >= 0)
            {
                var stats = Utilities.Stats.Entries.ElementAt(m_tabIndex).Value;
                foreach (var stat in stats)
                {
                    if (!string.IsNullOrEmpty(stat.text))
                        GUILayout.Label(stat.text);
                    if (stat.onGUI != null)
                        stat.onGUI();
                }
            }
		}

	}

}
