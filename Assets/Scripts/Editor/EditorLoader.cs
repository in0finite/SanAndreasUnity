using SanAndreasUnity.Behaviours;
using UGameCore.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class EditorLoader
    {
        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            Loader.onLoadingFinished -= OnLoadingFinished;
            Loader.onLoadingFinished += OnLoadingFinished;
        }

        static void EditorUpdate()
        {
            if (!F.IsAppInEditMode)
                return;

            if (Loader.IsLoading)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Loading game data", Loader.LoadingStatus, Loader.GetProgressPerc()))
                {
                    Loader.StopLoading();
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
        }

        static void OnLoadingFinished()
        {
            EditorUtility.ClearProgressBar();

            if (!F.IsAppInEditMode)
                return;

            if (F.IsInHeadlessMode)
                return;

            if (Loader.HasLoaded)
                EditorUtility.DisplayDialog("", "Successfully loaded game data.", "Ok");
            else
                EditorUtility.DisplayDialog("", "Error in loading game data. Check console for more information.", "Ok");
        }

        [MenuItem(EditorCore.MenuName + "/" + "Load game data")]
        static void MenuItemLoadGameData()
        {
            if (!F.IsAppInEditMode)
            {
                EditorUtility.DisplayDialog("", "This can only be used in edit mode.", "Ok");
                return;
            }

            if (Loader.HasLoaded)
            {
                EditorUtility.DisplayDialog("", "Game data is already loaded.", "Ok");
                return;
            }

            if (null == Loader.Singleton)
            {
                new GameObject("Loader", typeof(Loader));
            }

            Loader.StartLoading();
        }

        [MenuItem(EditorCore.MenuName + "/" + "Change path to GTA")]
        static void MenuItemChangePath()
        {
            if (!F.IsAppInEditMode)
            {
                EditorUtility.DisplayDialog("", "Exit play mode first.", "Ok");
                return;
            }

            string selectedFolder = EditorUtility.OpenFolderPanel("Select GTA installation folder", Config.GamePath ?? "", "");
            if (string.IsNullOrWhiteSpace(selectedFolder))
            {
                return;
            }

            if (!Loader.IsGamePathCorrect(selectedFolder, out string errorMessage))
            {
                EditorUtility.DisplayDialog("", "Selected folder is not valid:\r\n\r\n" + errorMessage, "Ok");
                return;
            }

            Config.SetString(Config.const_game_dir, selectedFolder);
            Config.SaveUserConfig();

            EditorUtility.DisplayDialog("", "Successfully changed path.", "Ok");
        }
    }
}
