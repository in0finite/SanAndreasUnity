using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class AssetExporter : EditorWindowBase
    {
        private const string DefaultFolderName = "ExportedAssets";
        private string m_selectedFolder = "Assets/" + DefaultFolderName;

        string ModelsPath => m_selectedFolder + "/Models";
        string CollisionModelsPath => m_selectedFolder + "/CollisionModels";
        string MaterialsPath => m_selectedFolder + "/Materials";
        string TexturesPath => m_selectedFolder + "/Textures";
        string PrefabsPath => m_selectedFolder + "/Prefabs";

        private CoroutineInfo m_coroutineInfo;

        private int m_numNewlyExportedAssets = 0;
        private int m_numAlreadyExportedAssets = 0;

        private enum ExportType
        {
            None = 0,
            FromSelection,
            FromLoadedWorld,
            FromGameFiles,
        }

        private ExportType m_exportType;

        private bool m_exportFromSelection => m_exportType == ExportType.FromSelection;
        private bool IsExportingFromLoadedWorld => m_exportType == ExportType.FromLoadedWorld;
        private bool IsExportingFromGameFiles => m_exportType == ExportType.FromGameFiles;

        private bool m_exportRenderMeshes = true;
        private bool m_exportMaterials = true;
        private bool m_exportTextures = true;
        private bool m_exportCollisionMeshes = true;
        private bool m_exportPrefabs = false;

        private struct SaveAssetAction
        {
            public UnityEngine.Object asset;
            public string path;
            public Action<UnityEngine.Object> assignAsset;
        }

        private readonly List<SaveAssetAction> m_saveAssetActions = new List<SaveAssetAction>();



        [MenuItem(EditorCore.MenuName + "/" + "Asset exporter")]
        static void Init()
        {
            var window = GetWindow<AssetExporter>();
            window.Show();
        }

        public AssetExporter()
        {
            this.titleContent = new GUIContent("Asset exporter");
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "This tool can export all currenty loaded world objects as assets and prefabs.\n" +
                "It will store them in a separate folder, and will only export those objects that were not already exported.",
                MessageType.Info,
                true);

            GUILayout.Space(30);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder: " + m_selectedFolder);
            if (GUILayout.Button("Change"))
                this.ChangeFolder();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            m_exportRenderMeshes = EditorGUILayout.Toggle("Export render meshes", m_exportRenderMeshes);
            m_exportMaterials = EditorGUILayout.Toggle("Export materials", m_exportMaterials);
            m_exportTextures = EditorGUILayout.Toggle("Export textures", m_exportTextures);
            m_exportCollisionMeshes = EditorGUILayout.Toggle("Export collision meshes", m_exportCollisionMeshes);
            m_exportPrefabs = EditorGUILayout.Toggle("Export prefabs", m_exportPrefabs);

            GUILayout.Space(30);

            if (GUILayout.Button("Export from game files"))
                this.Export(ExportType.FromGameFiles);

            if (GUILayout.Button("Export from world"))
                this.Export(ExportType.FromLoadedWorld);

            if (GUILayout.Button("Export from selection"))
                this.Export(ExportType.FromSelection);
        }

        void ChangeFolder()
        {
            string newFolder = EditorUtility.SaveFolderPanel(
                "Select folder where to export files",
                m_selectedFolder,
                "");
            if (string.IsNullOrWhiteSpace(newFolder))
            {
                return;
            }

            newFolder = FileUtil.GetProjectRelativePath(newFolder);
            if (string.IsNullOrWhiteSpace(newFolder))
            {
                EditorUtility.DisplayDialog("", "Folder must be inside project.", "Ok");
            }

            m_selectedFolder = newFolder;
        }

        void Export(ExportType exportType)
        {
            if (CoroutineManager.IsRunning(m_coroutineInfo))
                return;

            m_exportType = exportType;

            m_coroutineInfo = CoroutineManager.Start(this.ExportCoroutine(), this.Cleanup, ex => this.Cleanup());
        }

        void Cleanup()
        {
            EditorUtility.ClearProgressBar();
        }

        IEnumerator ExportCoroutine()
        {
            yield return null;

            m_numNewlyExportedAssets = 0;
            m_numAlreadyExportedAssets = 0;

            if (string.IsNullOrWhiteSpace(m_selectedFolder))
            {
                EditorUtility.DisplayDialog("", "Select a folder first.", "Ok");
                yield break;
            }

            if (m_exportFromSelection)
            {
                if (Selection.transforms.Length == 0)
                {
                    EditorUtility.DisplayDialog("", "No object selected.", "Ok");
                    yield break;
                }
            }

            var cell = Cell.Instance;
            if (null == cell && this.IsExportingFromLoadedWorld)
            {
                EditorUtility.DisplayDialog("", $"{nameof(Cell)} script not found in scene. Make sure that you started the game with the correct scene.", "Ok");
                yield break;
            }

            if (this.IsExportingFromGameFiles)
            {
                LoadingThread.Singleton.EnsureBackgroundThreadStarted();
                if (!LoadingThread.Singleton.IsBackgroundThreadRunning())
                {
                    EditorUtility.DisplayDialog("", "Background thread for asset loading is not running. Try restarting Unity.", "Ok");
                    yield break;
                }

                if (!Loader.HasLoaded)
                {
                    EditorUtility.DisplayDialog("", "Game data must be loaded first.", "Ok");
                    yield break;
                }

                cell = Cell.Instance;
                if (cell != null)
                {
                    if (!EditorUtility.DisplayDialog("", $"Found existing {nameof(Cell)} script in scene. Would you like to use this game object for creating world objects ?\r\n\r\nIf it is part of a prefab, the prefab will be unpacked.", "Ok", "Cancel"))
                        yield break;

                    if (PrefabUtility.IsPartOfPrefabInstance(cell.gameObject))
                    {
                        PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetNearestPrefabInstanceRoot(cell.gameObject), PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
                        EditorUtilityEx.MarkActiveSceneAsDirty();
                    }
                }

                if (null == cell)
                {
                    GameObject worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorCore.PrefabsPath + "/World.prefab");

                    GameObject worldObject = (GameObject)PrefabUtility.InstantiatePrefab(worldPrefab);
                    EditorUtilityEx.MarkActiveSceneAsDirty();

                    cell = Cell.Instance;
                    if (null == cell)
                        throw new Exception("Failed to create world object");
                }
            }

            EditorUtility.DisplayProgressBar("", "Gathering info...", 0f);

            Transform[] objectsToExport = Array.Empty<Transform>();

            if (m_exportFromSelection)
                objectsToExport = Selection.transforms.Where(_ => _.gameObject.activeInHierarchy).Where(_ => _.GetComponent<MapObject>() != null).ToArray();
            else if (this.IsExportingFromLoadedWorld)
                objectsToExport = cell.transform.GetFirstLevelChildren().Where(_ => _.gameObject.activeInHierarchy).ToArray();
            else if (this.IsExportingFromGameFiles)
            {
                cell.ignoreLodObjectsWhenInitializing = true;

                EditorUtilityEx.MarkActiveSceneAsDirty();

                EditorUtility.DisplayProgressBar("", "Creating static geometry...", 0f);
                cell.CreateStaticGeometry();
                EditorUtility.DisplayProgressBar("", "Initializing static geometry...", 0f);
                cell.InitStaticGeometry();

                objectsToExport = cell.StaticGeometries.Select(_ => _.Value.transform).ToArray();
            }

            EditorUtility.ClearProgressBar();

            if (0 == objectsToExport.Length)
            {
                EditorUtility.DisplayDialog("", "No suitable objects to export.", "Ok");
                yield break;
            }

            if (!EditorUtility.DisplayDialog(
                "",
                $"Found {objectsToExport.Length} objects to export.\r\nProceed ?",
                "Ok",
                "Cancel"))
            {
                yield break;
            }

            if (EditorApplication.isPlaying)
                EditorApplication.isPaused = true;

            var stopwatch = Stopwatch.StartNew();

            EditorUtility.DisplayProgressBar("", "Creating folders...", 0f);

            this.CreateFolders();

            if (this.IsExportingFromGameFiles)
            {
                EditorUtility.DisplayProgressBar("", "Preparing...", 0f);

                // disable automatic light baking, otherwise Editor will be very slow after assets are loaded and will fill the whole memory
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

                DayTimeManager.Singleton.SetTime(13, 0, true); // to make TOBJ objects visible

                EditorUtility.ClearProgressBar();
                yield return null;
                yield return null; // let the Editor update after changing day-time, who knows what all is changed

                LoadingThread.Singleton.maxTimePerFrameMs = 500;
            }

            EditorUtilityEx.MarkActiveSceneAsDirty();

            int nextIndexToTriggerLoad = 0;
            var isCanceledRef = new Ref<bool>();
            var etaStopwatch = Stopwatch.StartNew();
            string etaTime = "0";

            for (int i = 0; i < objectsToExport.Length; i++)
            {
                Transform currentObject = objectsToExport[i];

                if (this.IsExportingFromGameFiles && nextIndexToTriggerLoad == i)
                {
                    // loading of objects is done asyncly, so first we need to trigger load, then wait for it to complete

                    int nextNextIndex = Mathf.Min(i + 100, objectsToExport.Length);

                    for (int triggerLoadIndex = i; triggerLoadIndex < nextNextIndex; triggerLoadIndex++)
                    {
                        Transform triggerLoadObject = objectsToExport[triggerLoadIndex];

                        if (DisplayPausableProgressBar("", $"Triggering async load ({triggerLoadIndex + 1}/{objectsToExport.Length}), ETA {etaTime} ... {triggerLoadObject.name}", i / (float)objectsToExport.Length))
                            yield break;

                        var mapObject = triggerLoadObject.GetComponentOrThrow<MapObject>();
                        mapObject.UnShow();
                        mapObject.Show(1f);
                    }

                    nextIndexToTriggerLoad = nextNextIndex;

                    // wait for completion of jobs

                    foreach (var item in WaitForCompletionOfLoadingJobs(
                        $"\r\nETA {etaTime}, objects processed {i}/{objectsToExport.Length}",
                        i / (float)objectsToExport.Length,
                        nextNextIndex / (float)objectsToExport.Length,
                        4,
                        isCanceledRef))
                        yield return item;

                    if (isCanceledRef.value)
                        yield break;

                }

                if (DisplayPausableProgressBar("", $"Creating assets ({i + 1}/{objectsToExport.Length}), ETA {etaTime} ... {currentObject.name}", i / (float)objectsToExport.Length))
                    yield break;

                currentObject.gameObject.SetActive(true); // enable it so it can be seen when Editor un-freezes
                this.ExportAssets(currentObject.gameObject);

                if ((i % 50 == 0) || i == objectsToExport.Length - 1)
                {
                    var writeActions = new List<SaveAssetAction>();

                    // first read-only access
                    foreach (var action in m_saveAssetActions)
                    {
                        if (action.path.IsNullOrWhiteSpace())
                            continue;
                        if (AssetDatabase.Contains(action.asset))
                            continue;
                        if (AssetExistsAtPath(action.path))
                        {
                            action.assignAsset(AssetDatabase.LoadMainAssetAtPath(action.path));
                            continue;
                        }

                        writeActions.Add(action);
                    }

                    // now write access
                    if (writeActions.Count > 0)
                    {
                        AssetDatabase.StartAssetEditing();
                        try
                        {
                            foreach (var action in writeActions.DistinctBy(a => a.asset))
                            {
                                AssetDatabase.CreateAsset(action.asset, action.path);
                                //action.assignAsset();
                                m_numNewlyExportedAssets++;
                            }
                        }
                        finally
                        {
                            AssetDatabase.StopAssetEditing();
                        }
                    }

                    // now callbacks
                    foreach (var action in m_saveAssetActions)
                    {
                        if (action.path.IsNullOrWhiteSpace())
                            action.assignAsset(null);
                    }

                    m_saveAssetActions.Clear();
                }

                if (i % 50 == 0)
                {
                    // update ETA
                    double numPerSecond = 50 / etaStopwatch.Elapsed.TotalSeconds;
                    etaStopwatch.Restart();
                    int numLeft = objectsToExport.Length - i;
                    double secondsLeft = numLeft / numPerSecond;
                    etaTime = F.FormatElapsedTime((float) secondsLeft);
                }
            }

            if (m_exportPrefabs)
            {
                EditorUtility.DisplayProgressBar("", "Creating prefabs...", 1f);

                if (this.IsExportingFromLoadedWorld)
                    PrefabUtility.SaveAsPrefabAsset(cell.gameObject, $"{PrefabsPath}/ExportedWorld.prefab");
                else if (m_exportFromSelection)
                {
                    foreach (var obj in objectsToExport)
                    {
                        PrefabUtility.SaveAsPrefabAsset(obj.gameObject, $"{PrefabsPath}/{obj.gameObject.name}.prefab");
                    }
                }
                else if (this.IsExportingFromGameFiles)
                {
                    PrefabUtility.SaveAsPrefabAsset(cell.transform.root.gameObject, $"{PrefabsPath}/ExportedWorldFromGameFiles.prefab");
                }
            }

            EditorUtility.DisplayProgressBar("", "Refreshing asset database...", 1f);
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            string displayText = $"number of newly exported asssets {m_numNewlyExportedAssets}, number of already exported assets {m_numAlreadyExportedAssets}, time elapsed {stopwatch.Elapsed}";
            UnityEngine.Debug.Log($"Exporting of assets finished, {displayText}");
            EditorUtility.DisplayDialog("", $"Finished ! \r\n\r\n{displayText}", "Ok");
        }

        private static IEnumerable WaitForCompletionOfLoadingJobs(
            string textSuffix,
            float startPerc,
            float endPerc,
            int numIterations,
            Ref<bool> isCanceledRef)
        {
            if (numIterations < 1)
                throw new ArgumentOutOfRangeException(nameof(numIterations));

            isCanceledRef.value = false;

            float diffPerc = endPerc - startPerc;

            // TODO: this should be removed
            yield return null; // this must be done, otherwise LoadingThread does not start processing any job

            for (int i = 0; i < numIterations; i++)
            {
                long initialNumPendingJobs = LoadingThread.Singleton.GetNumPendingJobs();
                long numPendingJobs = initialNumPendingJobs;

                do
                {
                    long numJobsProcessed = initialNumPendingJobs - numPendingJobs;

                    float currentPerc = startPerc + diffPerc * (0 == initialNumPendingJobs ? 0f : numJobsProcessed / (float)initialNumPendingJobs);
                    if (DisplayPausableProgressBar("", $"Waiting for async jobs to finish ({numJobsProcessed}/{initialNumPendingJobs})...{textSuffix}", currentPerc))
                    {
                        isCanceledRef.value = true;
                        yield break;
                    }

                    LoadingThread.Singleton.UpdateJobs();

                    //System.Threading.Thread.Sleep(10); // don't interact with background thread too often, and also reduce CPU usage
                    yield return null;

                    numPendingJobs = LoadingThread.Singleton.GetNumPendingJobs();
                    initialNumPendingJobs = Math.Max(initialNumPendingJobs, numPendingJobs);

                } while (numPendingJobs > 0);
            }

        }

        void CreateFolders()
        {
            if (!Directory.Exists(m_selectedFolder))
                Directory.CreateDirectory(m_selectedFolder);
            
            string[] folders = new string[]
            {
                ModelsPath,
                MaterialsPath,
                TexturesPath,
                PrefabsPath,
                CollisionModelsPath,
            };

            foreach (string folder in folders)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
        }

        private void RegisterSaveAssetAction(UnityEngine.Object asset, string path, Action<UnityEngine.Object> assignAsset)
        {
            m_saveAssetActions.Add(new SaveAssetAction
            {
                asset = asset,
                path = path,
                assignAsset = assignAsset,
            });
        }

        public void ExportAssets(GameObject go)
        {
            string assetName = go.name;

            if (m_exportRenderMeshes)
            {
                var meshFilters = go.GetComponentsInChildren<MeshFilter>();

                for (int i = 0; i < meshFilters.Length; i++)
                {
                    MeshFilter meshFilter = meshFilters[i];
                    string indexPath = meshFilters.Length == 1 ? "" : "-" + i;
                    RegisterSaveAssetAction(meshFilter.sharedMesh, $"{ModelsPath}/{assetName}{indexPath}.asset", (obj) => meshFilter.sharedMesh = (Mesh)obj);
                    //meshFilter.sharedMesh = (Mesh)CreateAssetIfNotExists(meshFilter.sharedMesh, $"{ModelsPath}/{assetName}{indexPath}.asset");
                }
            }

            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                ExportMeshRenderer(go, meshRenderers[i], meshRenderers.Length == 1 ? (int?)null : i);
            }

            if (m_exportCollisionMeshes)
            {
                var meshColliders = go.GetComponentsInChildren<MeshCollider>();

                for (int i = 0; i < meshColliders.Length; i++)
                {
                    int tempColliderIndex = i;
                    string indexPath = meshColliders.Length == 1 ? "" : "-" + i;
                    RegisterSaveAssetAction(meshColliders[i].sharedMesh, $"{CollisionModelsPath}/{assetName}{indexPath}.asset", obj => meshColliders[tempColliderIndex].sharedMesh = (Mesh)obj);
                    //meshColliders[i].sharedMesh = (Mesh)CreateAssetIfNotExists(meshColliders[i].sharedMesh, $"{CollisionModelsPath}/{assetName}{indexPath}.asset");
                }
            }

        }

        public void ExportMeshRenderer(GameObject rootGo, MeshRenderer meshRenderer, int? index)
        {
            if (!m_exportTextures && !m_exportMaterials)
                return;

            string indexPath = index.HasValue ? "-" + index.Value : "";
            string assetName = rootGo.name + indexPath;

            var mats = meshRenderer.sharedMaterials.ToArray();

            for (int i = 0; i < mats.Length; i++)
            {
                if (m_exportTextures)
                {
                    int tempTexIndex = i;
                    var tex = mats[i].mainTexture;
                    if (tex != null && tex != Texture2D.whiteTexture) // sometimes materials will have white texture assigned, and Unity will crash if we attempt to create asset from it
                        RegisterSaveAssetAction(tex, $"{TexturesPath}/{assetName}-{i}.asset", obj => mats[tempTexIndex].mainTexture = (Texture)obj);
                        //mats[i].mainTexture = (Texture)CreateAssetIfNotExists(tex, $"{TexturesPath}/{assetName}-{i}.asset");
                }

                int tempMatIndex = i;
                if (m_exportMaterials)
                    RegisterSaveAssetAction(mats[i], $"{MaterialsPath}/{assetName}-{i}.mat", obj => mats[tempMatIndex] = (Material)obj);
                    //mats[i] = (Material)CreateAssetIfNotExists(mats[i], $"{MaterialsPath}/{assetName}-{i}.mat");
            }

            RegisterSaveAssetAction(null, "", obj => meshRenderer.sharedMaterials = mats);
            //meshRenderer.sharedMaterials = mats;
        }

        private UnityEngine.Object CreateAssetIfNotExists(UnityEngine.Object asset, string path)
        {
            if (AssetDatabase.Contains(asset))
                return asset;

            if (AssetExistsAtPath(path))
            {
                return AssetDatabase.LoadMainAssetAtPath(path);
            }

            AssetDatabase.CreateAsset(asset, path);

            m_numNewlyExportedAssets++;

            return asset;
        }

        private bool AssetExistsAtPath(string path)
        {
            if (File.Exists(Path.Combine(Directory.GetParent(Application.dataPath).FullName, path)))
            {
                m_numAlreadyExportedAssets++;
                return true;
            }
            return false;
        }

        public static bool DisplayPausableProgressBar(string title, string text, float progress, string dialogText, string ok, string cancel)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, text, progress))
            {
                EditorUtility.ClearProgressBar();
                // ok = continue
                return !EditorUtility.DisplayDialog(title, dialogText, ok, cancel);
            }

            return false;
        }

        public static bool DisplayPausableProgressBar(string title, string text, float progress)
        {
            return DisplayPausableProgressBar(title, text, progress, "Are you sure ?", "Continue", "Quit");
        }

    }
}
