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
        }

        protected override void OnWindowStart()
        {
            base.OnWindowStart();
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            //foreach (var kv in GTX.Gtx.EntryNameWordDict)
            //{
	           // Debug.LogError($"k {kv.Key} v {kv.Value}");
            //}
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

        protected override void OnWindowGUI()
        {
            base.OnWindowGUI();

        }

        protected override void OnWindowGUIAfterContent()
        {
            base.OnWindowGUIAfterContent();
        }
    }
}
