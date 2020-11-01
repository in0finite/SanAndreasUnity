using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Net
{

    public class CustomNetworkManager : NetworkManager
    {

        public override void OnClientConnect(NetworkConnection conn)
        {
            if (NetStatus.IsServer)
            {
                // just do default action
                base.OnClientConnect(conn);
                return;
            }

            // default method: if no scene was loaded, do Ready/AddPlayer

            // we won't do this until loading process finishes

            if (Loader.HasLoaded)
                base.OnClientConnect(conn);
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            if (NetStatus.IsServer)
            {
                // just do default action
                base.OnClientSceneChanged(conn);
                return;
            }

            // default method: do Ready/AddPlayer

            // we won't do this until loading process finishes

            if (Loader.HasLoaded)
                base.OnClientSceneChanged(conn);
        }

        void OnLoaderFinished()
        {
            if (NetStatus.IsServer)
            {
                if (Config.Get<bool>("RCON_enabled"))
                    RCON.RCONManager.StartServer();
                return; // don't do anything on server
            }

            if (!NetworkClient.isConnected)
            {
                // client is not connected ? hmm... then loading process could not have started
                Debug.LogErrorFormat("Loader finished, but client is not connected");
                return;
            }

            // make client ready
            if (ClientScene.ready)
                Debug.LogErrorFormat("Client was made ready before loader finished");
            else
                ClientScene.Ready(NetworkClient.connection);

            // add player if specified
            if (autoCreatePlayer && ClientScene.localPlayer == null)
            {
                ClientScene.AddPlayer();
            }
        }

        public override void ConfigureServerFrameRate()
        {
            // don't set frame rate
            // it will be done by other scripts
            
        }

    }

}
