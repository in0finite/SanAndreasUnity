using System;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public class CheckmarkMenuItem
{
    private static List<CheckmarkMenuItem> instances = new List<CheckmarkMenuItem>();

    private string MENU_NAME;
    private bool enabled_;

    public Action menuAction;

    private CheckmarkMenuItem()
    {
    }

    public CheckmarkMenuItem(string menuName)
    {
        MENU_NAME = menuName;
        instances.Add(this);
    }

    public CheckmarkMenuItem(string menuName, bool enabled)
    {
        MENU_NAME = menuName;
        enabled_ = enabled;
        instances.Add(this);
    }

    /// Called on load thanks to the InitializeOnLoad attribute
    static CheckmarkMenuItem()
    {
        foreach (CheckmarkMenuItem menuItem in instances)
        {
            menuItem.enabled_ = EditorPrefs.GetBool(menuItem.MENU_NAME, false);

            /// Delaying until first editor tick so that the menu
            /// will be populated before setting check state, and
            /// re-apply correct action
            EditorApplication.delayCall += () =>
            {
                menuItem.PerformAction(menuItem.enabled_);
            };
        }
    }

    public void ToggleAction()
    {
        /// Toggling action
        PerformAction(!enabled_);
    }

    private void PerformAction(bool enabled)
    {
        /// Set checkmark on menu item
        Menu.SetChecked(MENU_NAME, enabled);
        /// Saving editor state
        EditorPrefs.SetBool(MENU_NAME, enabled);

        enabled_ = enabled;

        /// Perform your logic here...
        menuAction();
    }
}