using System.Collections;
using System.Linq;
using SanAndreasUnity.GameModes;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Net
{
    public class NetCmdLineHandler : MonoBehaviour
    {
        public int numFramesToWait = 5;


        IEnumerator Start()
        {
            if (!F.IsInHeadlessMode)
                yield break;
            
            for (int i = 0; i < this.numFramesToWait; i++)
                yield return null;

            ushort portNum = (ushort)NetManager.defaultListenPortNumber;
            CmdLineUtils.GetUshortArgument("portNum", ref portNum);

            string sceneName = "Main";
            CmdLineUtils.GetArgument("scene", ref sceneName);

            ushort maxNumPlayers = (ushort)NetManager.maxNumPlayers;
            CmdLineUtils.GetUshortArgument("maxNumPlayers", ref maxNumPlayers);

            string serverIp = "127.0.0.1";
            CmdLineUtils.GetArgument("serverIp", ref serverIp);

            string gameModeName = null;
            CmdLineUtils.GetArgument("gameMode", ref gameModeName);
            if (!string.IsNullOrWhiteSpace(gameModeName))
            {
                var gameModeInfo = GameModeManager.Instance.GameModes.FirstOrDefault(gm => gm.Name == gameModeName);
                if (gameModeInfo != null)
                    GameModeManager.Instance.SelectGameMode(gameModeInfo);
                else
                    Debug.LogError($"Game mode with name '{gameModeName}' not found");
            }

            if (CmdLineUtils.HasArgument("startServer"))
            {
                Debug.LogFormat("Starting server in headless mode, params: {0}, {1}, {2}", portNum, sceneName,
                    maxNumPlayers);
                NetManager.StartServer(portNum, sceneName, maxNumPlayers, true, false);
            }
            else if (CmdLineUtils.HasArgument("startClient"))
            {
                Debug.LogFormat("Starting client in headless mode, params: {0}, {1}", serverIp, portNum);
                NetManager.StartClient(serverIp, portNum);
            }
        }
    }
}