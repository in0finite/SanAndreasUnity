using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Net
{
    public class CustomNetworkManager : NetworkManager
    {
        public override void OnClientConnect()
        {
            if (NetStatus.IsServer)
            {
                // just do default action
                base.OnClientConnect();
                return;
            }

            // default method: if no scene was loaded, do Ready/AddPlayer
            // we won't do this until loading process finishes

            if (Loader.HasLoaded)
                base.OnClientConnect();
        }

        public override void OnClientSceneChanged()
        {
            if (NetStatus.IsServer)
            {
                // just do default action
                base.OnClientSceneChanged();
                return;
            }

            // default method: do Ready/AddPlayer
            // we won't do this until loading process finishes

            if (Loader.HasLoaded)
                base.OnClientSceneChanged();
        }

        void OnLoaderFinished()
        {
            if (F.IsAppInEditMode)
                return;

            if (NetStatus.IsServer) // don't do anything on server
                return;

            if (!NetworkClient.isConnected)
            {
                // client is not connected ? hmm... then loading process could not have started
                Debug.LogErrorFormat("Loader finished, but client is not connected");
                return;
            }


            // make client ready
            if (NetworkClient.ready)
                Debug.LogErrorFormat("Client was made ready before loader finished");
            else
            {
                NetworkClient.Ready();
            }

            // add player if specified
            if (autoCreatePlayer && NetworkClient.localPlayer == null)
            {
                NetworkClient.AddPlayer();
            }
        }

        public override void ConfigureHeadlessFrameRate()
        {
           //Overriden so that other scripts can set the framerate without Mirror overriding it.
        }
    }
}