using System.Collections;
using System.Linq;
using SanAndreasUnity.GameModes;
using UnityEngine;
using UGameCore.Utilities;

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

            ushort portNum = CmdLineUtils.GetUshortArgumentOrDefault(
                "portNum", (ushort)NetManager.defaultListenPortNumber);

            string sceneName = CmdLineUtils.GetStringArgumentOrDefault("scene", "Main");

            ushort maxNumPlayers = CmdLineUtils.GetUshortArgumentOrDefault(
                "maxNumPlayers", (ushort)NetManager.maxNumPlayers);

            string serverIp = CmdLineUtils.GetStringArgumentOrDefault("serverIp", "127.0.0.1");

            if (CmdLineUtils.TryGetStringArgument("gameMode", out string gameModeName))
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