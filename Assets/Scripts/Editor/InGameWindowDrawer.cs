using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SanAndreasUnity.UI;
using System.Linq;
using System;
using UGameCore.Utilities;

namespace SanAndreasUnity.Editor
{
    public class InGameWindowDrawer : EditorWindowBase
    {
        private bool m_searchedForWindows = false;
        private List<PauseMenuWindow> m_pauseMenuWindows = new List<PauseMenuWindow>();
        private int m_selectedWindowIndex = -1;
        private Vector2 m_scrollViewPosition = Vector2.zero;


        public InGameWindowDrawer()
        {
            this.titleContent = new GUIContent("In-game window drawer");
            this.minSize = new Vector2(300, 200);
            this.position = new Rect(this.position.center, new Vector2(700, 500));
        }

        [MenuItem(EditorCore.MenuName + "/" + "In-game window drawer")]
        static void Init()
        {
            var window = GetWindow<InGameWindowDrawer>();
            window.Show();
        }

        void SearchForWindows()
        {
            if (m_searchedForWindows)
                return;

            if (Behaviours.UIManager.Singleton != null)
            {
                m_searchedForWindows = true;
                m_pauseMenuWindows = Behaviours.UIManager.Singleton
                    .GetComponentsInChildren<PauseMenuWindow>()
                    .Where(w => w.DrawInEditMode)
                    .ToList();
            }
        }

        void OnGUI()
        {
            this.SearchForWindows();

            int tabsAreaWidth = 120;

            GUILayout.BeginArea(new Rect(0, 0, tabsAreaWidth, this.position.height));

            m_scrollViewPosition = GUILayout.BeginScrollView(m_scrollViewPosition);
            for (int i = 0; i < m_pauseMenuWindows.Count; i++)
            {
                if (null == m_pauseMenuWindows[i])
                    continue;

                GUI.enabled = m_selectedWindowIndex != i;
                if (GUILayout.Button(m_pauseMenuWindows[i].windowName, GUILayout.Width(tabsAreaWidth - 10)))
                    m_selectedWindowIndex = i;
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();

            GUILayout.EndArea();

            Rect windowRect = new Rect(tabsAreaWidth, 0, this.position.width - tabsAreaWidth, this.position.height);
            GUILayout.BeginArea(windowRect);

            if (m_selectedWindowIndex >= 0)
            {
                var window = m_pauseMenuWindows[m_selectedWindowIndex];
                if (window != null)
                {
                    window.windowRect = windowRect;
                    window.DrawWindowContent();
                }
            }

            GUILayout.EndArea();
        }
    }
}
