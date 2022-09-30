using System;
using System.Collections.Generic;
using SanAndreasUnity.Net;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.GameModes
{
    public class GameModeManager : StartupSingleton<GameModeManager>
    {
        public static GameModeManager Instance => Singleton;

        public class GameModeInfo
        {
            public string Name { get; }
            public string Description { get; }
            public System.Action ActivationCallback { get; }

            public GameModeInfo(string name, string description, Action activationCallback)
            {
                Name = name;
                Description = description;
                ActivationCallback = activationCallback;
            }
        }

        List<GameModeInfo> m_gameModes = new List<GameModeInfo>();
        public IReadOnlyList<GameModeInfo> GameModes => m_gameModes;

        private GameModeInfo m_activeGameMode;

        private GameModeInfo m_selectedGameMode;


        protected override void OnSingletonStart()
        {
            NetManager.Instance.onServerStatusChanged += OnServerStatusChanged;
        }

        private void OnServerStatusChanged()
        {
            if (!NetStatus.IsServer)
                return;

            if (null == m_selectedGameMode)
                return;

            ActivateGameMode(m_selectedGameMode);
        }

        public void RegisterGameMode(GameModeInfo gameModeInfo)
        {
            m_gameModes.Add(gameModeInfo);
        }

        /// <summary>
        /// Select game mode which should be activated when the server starts.
        /// </summary>
        public void SelectGameMode(GameModeInfo gameModeInfo)
        {
            if (m_activeGameMode != null)
            {
                throw new Exception("Can not select game mode if there is an active game mode");
            }

            m_selectedGameMode = gameModeInfo;
        }

        private void ActivateGameMode(GameModeInfo gameModeInfo)
        {
            if (m_activeGameMode != null)
            {
                if (m_activeGameMode == gameModeInfo)
                    throw new Exception($"Can not activate game mode '{gameModeInfo.Name}' because it is already activated");
                throw new Exception($"Can not activate game mode '{gameModeInfo.Name}' because another one ('{m_activeGameMode.Name}') is already activated");
            }

            m_activeGameMode = gameModeInfo;
            gameModeInfo.ActivationCallback();
        }
    }
}
