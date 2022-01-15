using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SanAndreasUnity.Behaviours.World;
using System.Collections;

namespace SanAndreasUnity.Editor
{
    public class NavMeshGenerator : EditorWindowBase
    {
        private static int s_selectedAgentId = 0;
        private static NavMeshData s_navMeshData;
        private static IEnumerator s_coroutine;

        public NavMeshGenerator()
        {
            this.titleContent = new GUIContent("Nav mesh generator");
        }

        void Cleanup()
        {
            s_coroutine = null;

            EditorUtility.ClearProgressBar();

            if (s_navMeshData != null)
            {
                NavMeshBuilder.Cancel(s_navMeshData);
                Destroy(s_navMeshData);
                s_navMeshData = null;
            }
        }

        [MenuItem(EditorCore.MenuName + "/" + "Generate nav mesh")]
        static void Init()
        {
            var window = GetWindow<NavMeshGenerator>();
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Generate nav mesh from loaded world.\n" +
                "Only high LOD world objects and water are included.\n" +
                "At the end, you will choose where to save the generated nav mesh.",
                MessageType.Info,
                true);

            GUILayout.Space(20);

            EditorGUILayout.PrefixLabel("Agent ID:");
            s_selectedAgentId = EditorGUILayout.IntField(s_selectedAgentId);

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(NavMesh.GetSettingsNameFromID(s_selectedAgentId)))
            {
                var navMeshBuildSettings = NavMesh.GetSettingsByID(s_selectedAgentId);
                EditorUtils.DrawFieldsAndPropertiesInInspector(navMeshBuildSettings, 0);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Generate"))
                Generate();
            
        }

        void Generate()
        {
            if (s_coroutine != null)
                return;

            Cleanup();

            s_coroutine = this.DoGenerate();
            this.StartCoroutine(s_coroutine, this.Cleanup, ex => this.Cleanup());
        }

        IEnumerator DoGenerate()
        {
            yield return null;

            if (string.IsNullOrEmpty(NavMesh.GetSettingsNameFromID(s_selectedAgentId)))
            {
                EditorUtility.DisplayDialog("", "Invalid agent id", "Ok");
                yield break;
            }

            var cell = Cell.Instance;
            if (null == cell)
            {
                EditorUtility.DisplayDialog("", $"{nameof(Cell)} script not found in scene. Make sure you loaded the correct scene.", "Ok");
                yield break;
            }

            NavMeshBuildSettings navMeshBuildSettings = NavMesh.GetSettingsByID(s_selectedAgentId);

            NavMesh.RemoveAllNavMeshData();

            var navMeshData = s_navMeshData = new NavMeshData(s_selectedAgentId);

            NavMesh.AddNavMeshData(s_navMeshData);

            EditorUtility.DisplayProgressBar("Generating nav mesh", "Collecting objects...", 0f);

            var navMeshBuildSources = new List<NavMeshBuildSource>(1024 * 20);
            int numObjects = 0;

            foreach (StaticGeometry staticGeometry in cell.StaticGeometries.Values)
            {
                if (null != staticGeometry.LodParent)
                    continue;

                numObjects++;

                navMeshBuildSources.AddRange(Cell.GetNavMeshBuildSources(staticGeometry.transform));
            }

            navMeshBuildSources.AddRange(cell.GetWaterNavMeshBuildSources());

            EditorUtility.ClearProgressBar();

            if (!EditorUtility.DisplayDialog("", $"Found total of {numObjects} objects with {navMeshBuildSources.Count} sources. Proceed ?", "Ok", "Cancel"))
                yield break;

            var asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
                navMeshData,
                navMeshBuildSettings,
                navMeshBuildSources,
                new Bounds(cell.transform.position, Vector3.one * cell.WorldSize));

            while (!(asyncOperation.isDone || asyncOperation.progress == 1f))
            {
                yield return null;

                if (EditorUtility.DisplayCancelableProgressBar("", "Updating nav mesh...", asyncOperation.progress))
                    yield break;
            }

            EditorUtility.ClearProgressBar();

            if (asyncOperation.progress != 1f)
            {
                EditorUtility.DisplayDialog("", $"Updating nav mesh did not finish completely, progress is {asyncOperation.progress}", "Ok");
                yield break;
            }

            // save nav mesh

            // CreateAsset() probably requires the file to be in project
            string saveFilePath = EditorUtility.SaveFilePanelInProject("Save nav mesh", "NavMesh.asset", "asset", "");

            if (!string.IsNullOrWhiteSpace(saveFilePath))
            {
                EditorUtility.DisplayProgressBar("", "Saving nav mesh...", 1f);

                AssetDatabase.CreateAsset(s_navMeshData, saveFilePath);
                s_navMeshData = null; // this is now an asset, we don't want it to be destroyed
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("", "Nav mesh generation complete !", "Ok");
        }
    }
}
