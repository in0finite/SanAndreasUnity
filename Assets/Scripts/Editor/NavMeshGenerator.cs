using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SanAndreasUnity.Behaviours.World;
using System.Collections;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Editor
{
    public class NavMeshGenerator : EditorWindowBase
    {
        private static int s_selectedAgentId = 0;
        [SerializeField] private NavMeshData m_navMeshData;
        private static CoroutineInfo s_coroutine;


        [MenuItem(EditorCore.MenuName + "/" + "Generate nav mesh")]
        static void Init()
        {
            var window = GetWindow<NavMeshGenerator>();
            window.Show();
        }

        public NavMeshGenerator()
        {
            this.titleContent = new GUIContent("Nav mesh generator");
        }

        void Cleanup()
        {
            EditorUtility.ClearProgressBar();
            CancelBuild();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Generate nav mesh from loaded world.\n" +
                "Only high LOD world objects and water are included.\n" +
                "Only collision data will be included, rendering data will be ignored.",
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

            EditorGUILayout.PrefixLabel("Current nav mesh:");
            EditorGUILayout.ObjectField(m_navMeshData, typeof(NavMeshData), false);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate"))
                Generate();

            GUI.enabled = m_navMeshData != null;
            if (GUILayout.Button("Save navmesh as asset"))
                SaveNavMesh();

            GUI.enabled = m_navMeshData != null;
            if (GUILayout.Button("Clear current navmesh"))
                ClearCurrentNavMesh();

            GUI.enabled = true;
            if (GUILayout.Button("Cancel build"))
                CancelBuild();

            GUI.enabled = true;
        }

        void ClearCurrentNavMesh()
        {
            if (m_navMeshData != null)
            {
                NavMeshBuilder.Cancel(m_navMeshData);
                F.DestroyEvenInEditMode(m_navMeshData);
                m_navMeshData = null;
            }
        }

        void CancelBuild()
        {
            UnityEditor.AI.NavMeshBuilder.Cancel();
            if (m_navMeshData != null)
                NavMeshBuilder.Cancel(m_navMeshData);
        }

        void SaveNavMesh()
        {
            if (null == m_navMeshData)
                return;

            EditorUtility.ClearProgressBar();

            // CreateAsset() probably requires the file to be in project
            string saveFilePath = EditorUtility.SaveFilePanelInProject("Save nav mesh", "NavMesh.asset", "asset", "");

            if (!string.IsNullOrWhiteSpace(saveFilePath))
            {
                EditorUtility.DisplayProgressBar("", "Saving nav mesh...", 1f);

                AssetDatabase.CreateAsset(m_navMeshData, saveFilePath);
            }

            EditorUtility.ClearProgressBar();
        }

        void Generate()
        {
            if (CoroutineManager.IsRunning(s_coroutine))
                return;

            Cleanup();

            s_coroutine = CoroutineManager.Start(this.DoGenerate(), this.Cleanup, ex => this.Cleanup());
        }

        IEnumerator DoGenerate()
        {
            yield return null;
            
            if (UnityEditor.AI.NavMeshBuilder.isRunning)
            {
                EditorUtility.DisplayDialog("", "Navmesh build is already running", "Ok");
                yield break;
            }

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

            /*UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            NavMesh.RemoveAllNavMeshData();*/

            if (m_navMeshData == null)
            {
                m_navMeshData = new NavMeshData(s_selectedAgentId);
                NavMesh.AddNavMeshData(m_navMeshData);
            }

            var asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
                m_navMeshData,
                navMeshBuildSettings,
                navMeshBuildSources,
                new Bounds(cell.transform.position, Vector3.one * cell.WorldSize));

            while (!(asyncOperation.isDone || asyncOperation.progress == 1f))
            {
                yield return null;

                if (EditorUtils.DisplayPausableProgressBar("", "Updating nav mesh...", asyncOperation.progress))
                    yield break;
            }

            EditorUtility.ClearProgressBar();

            if (asyncOperation.progress != 1f)
            {
                EditorUtility.DisplayDialog("", $"Updating nav mesh did not finish completely, progress is {asyncOperation.progress}", "Ok");
                yield break;
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("", "Nav mesh generation complete !", "Ok");
        }
    }
}
