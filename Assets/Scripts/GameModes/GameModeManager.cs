using System;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.GameModes
{
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

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


        void Awake()
        {
            Instance = this;
        }

        public void RegisterGameMode(GameModeInfo gameModeInfo)
        {
            m_gameModes.Add(gameModeInfo);
        }

        public void ActivateGameMode(GameModeInfo gameModeInfo)
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
