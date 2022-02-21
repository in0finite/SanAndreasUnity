using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SanAndreasUnity.Behaviours.World;
using System.Collections;
using SanAndreasUnity.Utilities;
using UnityEditor.SceneManagement;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Editor
{
    public class NavMeshGenerator : EditorWindowBase
    {
        private static int s_selectedAgentId = 0;
        [SerializeField] private NavMeshData m_navMeshData;
        private NavMeshDataInstance m_navMeshDataInstance = new NavMeshDataInstance();
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

            GUILayout.BeginHorizontal();
            GUILayout.Label("Agent ID:");
            s_selectedAgentId = EditorGUILayout.IntField(s_selectedAgentId);
            if (GUILayout.Button("Edit"))
                UnityEditor.AI.NavMeshEditorHelpers.OpenAgentSettings(s_selectedAgentId);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

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
            if (GUILayout.Button("Remove all navmeshes"))
                RemoveAllNavMeshes();

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

                NavMesh.RemoveNavMeshData(m_navMeshDataInstance);
                m_navMeshDataInstance = new NavMeshDataInstance();

                if (!AssetDatabase.Contains(m_navMeshData))
                    F.DestroyEvenInEditMode(m_navMeshData);

                m_navMeshData = null;
            }
        }

        void RemoveAllNavMeshes()
        {
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            NavMesh.RemoveAllNavMeshData();
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

                try
                {
                    if (AssetDatabase.Contains(m_navMeshData))
                    {
                        NavMesh.RemoveAllNavMeshData();
                        UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
                        m_navMeshDataInstance = new NavMeshDataInstance();

                        m_navMeshData = Object.Instantiate(m_navMeshData);

                        m_navMeshDataInstance = NavMesh.AddNavMeshData(m_navMeshData);
                    }

                    AssetDatabase.CreateAsset(m_navMeshData, saveFilePath);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }
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

            foreach (MapObject mapObject in cell.gameObject.GetFirstLevelChildrenSingleComponent<MapObject>())
            {
                if (!mapObject.gameObject.activeInHierarchy)
                    continue;
                if (mapObject is StaticGeometry staticGeometry && null != staticGeometry.LodParent)
                    continue;

                numObjects++;

                navMeshBuildSources.AddRange(Cell.GetNavMeshBuildSources(mapObject.transform));
            }

            if (cell.Water != null && cell.Water.gameObject.activeInHierarchy)
            {
                navMeshBuildSources.AddRange(cell.GetWaterNavMeshBuildSources());
                numObjects++;
            }

            EditorUtility.ClearProgressBar();

            if (!EditorUtility.DisplayDialog("", $"Found total of {numObjects} objects with {navMeshBuildSources.Count} sources. Proceed ?", "Ok", "Cancel"))
                yield break;

            if (m_navMeshData == null)
            {
                m_navMeshData = NavMeshBuilder.BuildNavMeshData(
                    navMeshBuildSettings,
                    new List<NavMeshBuildSource>(),
                    new Bounds(),
                    cell.transform.position,
                    cell.transform.rotation);
            }

            m_navMeshDataInstance = NavMesh.AddNavMeshData(m_navMeshData);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(cell.gameObject.scene);

            var asyncOperation = NavMeshBuilder.UpdateNavMeshDataAsync(
                m_navMeshData,
                navMeshBuildSettings,
                navMeshBuildSources,
                new Bounds(cell.transform.position, Vector3.one * cell.WorldSize));

            var etaMeasurer = new ETAMeasurer(2f);

            while (!asyncOperation.isDone)
            {
                yield return null;

                etaMeasurer.UpdateETA(asyncOperation.progress);

                if (EditorUtils.DisplayPausableProgressBar("", $"Updating nav mesh... ETA: {etaMeasurer.ETA}", asyncOperation.progress))
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
