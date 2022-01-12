using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Settings;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class InfiniteDrawDistance : MonoBehaviour
    {
        private const string PrefabPath = "Assets/Prefabs";


        [MenuItem(EditorCore.MenuName + "/" + "Enable infinite draw distance")]
        static void Init()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("", "Exit play mode first", "Ok");
                return;
            }

            if (!EditorUtility.DisplayDialog("", "This will modify some prefab parameters " +
                "in order to enable infinite draw distance. Low LOD objects will not load.\n\nProceed ?", "Ok", "Cancel"))
                return;
            
            GameObject settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/Settings.prefab");
            settingsPrefab.GetComponentInChildren<WorldSettings>().overridenMaxDrawDistance = 20000f;
            
            GameObject worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath + "/World.prefab");
            var cell = worldPrefab.GetComponentInChildren<Cell>();
            cell.ignoreLodObjectsWhenInitializing = true;
            cell.drawDistanceMultiplier = 20000f;
            cell.drawDistancesPerLayers[cell.drawDistancesPerLayers.Length - 1] = 20000f;
            cell.maxTimeToUpdatePerFrameMs = 200; // allow 5 FPS

            PrefabUtility.SavePrefabAsset(settingsPrefab);
            PrefabUtility.SavePrefabAsset(worldPrefab);

            PlayerPrefs.SetString(WorldSettings.DrawDistanceSerializationName, 20000f.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();

            EditorUtility.DisplayDialog("", "Done", "Ok");
        }
    }
}
