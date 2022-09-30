using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using SanAndreasUnity.Behaviours.World;
using System.Collections;
using UGameCore.Utilities;
using UnityEditor.SceneManagement;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Editor
{
    public class NavMeshGenerator
    {
        private NavMeshBuildSettings m_navMeshBuildSettings;
        private NavMeshData m_navMeshData;
        public NavMeshData NavMeshData => m_navMeshData;
        private NavMeshDataInstance m_navMeshDataInstance = new NavMeshDataInstance();
        private static CoroutineInfo s_coroutine;
        private bool m_isSilentMode = false;

        public bool IsRunning => CoroutineManager.IsRunning(s_coroutine);

        public bool FinishedSuccessfully { get; private set; } = false;

        public bool LogProgressPeriodically { get; set; } = false;



        public NavMeshGenerator(NavMeshData navMeshData)
        {
            m_navMeshData = navMeshData;
        }

        void Cleanup()
        {
            EditorUtility.ClearProgressBar();
            CancelBuild();
        }

        public void ClearCurrentNavMesh()
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

        public void RemoveAllNavMeshes()
        {
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            NavMesh.RemoveAllNavMeshData();
        }

        public void CancelBuild()
        {
            UnityEditor.AI.NavMeshBuilder.Cancel();
            if (m_navMeshData != null)
                NavMeshBuilder.Cancel(m_navMeshData);
        }

        public void SaveNavMesh()
        {
            if (null == m_navMeshData)
                return;

            EditorUtility.ClearProgressBar();

            // CreateAsset() probably requires the file to be in project
            string saveFilePath = EditorUtility.SaveFilePanelInProject("Save nav mesh", "NavMesh.asset", "asset", "");

            if (!string.IsNullOrWhiteSpace(saveFilePath))
            {
                this.SaveNavMesh(saveFilePath);
            }
        }

        public void SaveNavMesh(string saveFilePath)
        {
            if (null == m_navMeshData)
                return;

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

        public void Generate(NavMeshBuildSettings navMeshBuildSettings, bool isSilentMode)
        {
            if (this.IsRunning)
                return;

            Cleanup();

            this.FinishedSuccessfully = false;
            m_navMeshBuildSettings = navMeshBuildSettings;
            m_isSilentMode = isSilentMode;

            s_coroutine = CoroutineManager.Start(this.DoGenerate(), this.Cleanup, ex => this.Cleanup());
        }

        IEnumerator DoGenerate()
        {
            yield return null;
            
            if (UnityEditor.AI.NavMeshBuilder.isRunning)
            {
                DisplayMessage("Navmesh build is already running");
                yield break;
            }

            var cell = Cell.Instance;
            if (null == cell)
            {
                DisplayMessage($"{nameof(Cell)} script not found in scene. Make sure you loaded the correct scene.");
                yield break;
            }

            NavMeshBuildSettings navMeshBuildSettings = m_navMeshBuildSettings;

            EditorUtility.DisplayProgressBar("Generating nav mesh", "Collecting objects...", 0f);

            var navMeshBuildSources = new List<NavMeshBuildSource>(1024 * 20);
            int numObjects = 0;

            foreach (MapObject mapObject in cell.gameObject.GetFirstLevelChildrenSingleComponent<MapObject>())
            {
                if (!mapObject.gameObject.activeInHierarchy)
                    continue;
                
                numObjects++;

                mapObject.AddNavMeshBuildSources(navMeshBuildSources);
            }

            if (cell.Water != null && cell.Water.gameObject.activeInHierarchy)
            {
                navMeshBuildSources.AddRange(cell.GetWaterNavMeshBuildSources());
                numObjects++;
            }

            EditorUtility.ClearProgressBar();

            if (!AskDialog(true, $"Found total of {numObjects} objects with {navMeshBuildSources.Count} sources. Proceed ?", "Ok", "Cancel"))
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
                new Bounds(cell.transform.position, new Vector3(cell.WorldSize, (cell.interiorHeightOffset + 1000f + 300f) * 2f, cell.WorldSize)));

            var etaMeasurer = new ETAMeasurer(2f);
            var logStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var totalUpdateTimeStopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (!asyncOperation.isDone)
            {
                yield return null;

                etaMeasurer.UpdateETA(asyncOperation.progress);

                if (this.LogProgressPeriodically && logStopwatch.Elapsed.TotalSeconds > 20)
                {
                    Debug.Log($"Updating nav mesh... ETA: {etaMeasurer.ETA}, progress: {asyncOperation.progress}, elapsed: {totalUpdateTimeStopwatch.Elapsed}");
                    logStopwatch.Restart();
                }

                if (EditorUtils.DisplayPausableProgressBar("", $"Updating nav mesh... ETA: {etaMeasurer.ETA}", asyncOperation.progress))
                    yield break;
            }

            EditorUtility.ClearProgressBar();

            if (asyncOperation.progress != 1f)
            {
                DisplayMessage($"Updating nav mesh did not finish completely, progress is {asyncOperation.progress}");
                yield break;
            }

            EditorUtility.ClearProgressBar();
            DisplayMessage($"Nav mesh generation complete !\r\nElapsed time: {totalUpdateTimeStopwatch.Elapsed}");

            this.FinishedSuccessfully = true;
        }

        private void DisplayMessage(string message)
        {
            if (m_isSilentMode)
                UnityEngine.Debug.Log(message);
            else
                EditorUtility.DisplayDialog("", message, "Ok");
        }

        private bool AskDialog(bool defaultValue, string message, string ok, string cancel)
        {
            if (m_isSilentMode)
                return defaultValue;
            return EditorUtility.DisplayDialog("", message, ok, cancel);
        }
    }
}
