using SanAndreasUnity.Importing.GXT;
using UnityEngine;

namespace SanAndreasUnity.UI
{
    class GXTWindow : PauseMenuWindow
    {
        GXTWindow()
        {
            //this will use as menu item name as well as window title
            this.windowName = "GXTWindow";
        }

        void Start()
        {
            this.RegisterButtonInPauseMenu();
            this.useScrollView = true;
            this.windowRect = Utilities.GUIUtils.GetCenteredRect(new Vector2(600, 400));

		}

        protected override void OnWindowGUI()
        {
	        base.OnWindowGUI();

	        foreach (var gxtSubTableName in GXT.Gxt.SubTableNames)
	        {
		        if (GUILayout.Button(gxtSubTableName))
		        {
					Debug.LogError(gxtSubTableName);
		        }
	        }
        }

        protected override void OnWindowStart()
        {
            base.OnWindowStart();
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
		}

        protected override void OnWindowClosed()
        {
            base.OnWindowClosed();
        }

        protected override void OnLoaderFinished()
        {
            base.OnLoaderFinished();
        }

        protected override void OnWindowGUIBeforeContent()
        {
            base.OnWindowGUIBeforeContent();
        }

        protected override void OnWindowGUIAfterContent()
        {
            base.OnWindowGUIAfterContent();
        }
    }
}
