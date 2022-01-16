using SanAndreasUnity.Behaviours.World;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

            GUILayout.Space(20);

            if (GUILayout.Button("Export"))
                this.Export();
        }

        void Export()
        {
            if (this.IsCoroutineRunning(m_coroutineInfo))
                return;

            m_coroutineInfo = this.StartCoroutine(this.ExportCoroutine(), this.Cleanup, ex => this.Cleanup());
        }

        void Cleanup()
        {
            EditorUtility.ClearProgressBar();
        }

        IEnumerator ExportCoroutine()
        {
            yield return null;

            var cell = Cell.Instance;
            if (null == cell)
            {
                EditorUtility.DisplayDialog("", $"{nameof(Cell)} script not found in scene. Make sure that you started the game with the correct scene.", "Ok");
                yield break;
            }

            EditorUtility.DisplayProgressBar("", "Gathering info...", 0f);

            int numObjectsActive = 0;
            for (int i = 0; i < cell.transform.childCount; i++)
            {
                var child = cell.transform.GetChild(i);

                if (child.gameObject.activeInHierarchy)
                    numObjectsActive++;
            }

            EditorUtility.ClearProgressBar();

            if (!EditorUtility.DisplayDialog(
                "",
                $"There are {cell.transform.childCount} world objects, with {numObjectsActive} active ones.\nProceed ?",
                "Ok",
                "Cancel"))
            {
                yield break;
            }

            m_selectedFolder = EditorUtility.SaveFolderPanel(
                "Select folder where to export files",
                m_selectedFolder,
                "");
            if (string.IsNullOrWhiteSpace(m_selectedFolder))
            {
                yield break;
            }

            m_selectedFolder = FileUtil.GetProjectRelativePath(m_selectedFolder);
            if (string.IsNullOrWhiteSpace(m_selectedFolder))
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("", "Folder must be inside project.", "Ok");
                yield break;
            }

            if (EditorApplication.isPlaying)
                EditorApplication.isPaused = true;

            EditorUtility.DisplayProgressBar("", "Creating folders...", 0f);

            this.CreateFolders();

            EditorUtility.DisplayProgressBar("", "Creating assets...", 0f);

            int numExported = 0;
            for (int i = 0; i < cell.transform.childCount; i++)
            {
                var child = cell.transform.GetChild(i);

                if (!child.gameObject.activeInHierarchy)
                    continue;

                if (EditorUtility.DisplayCancelableProgressBar("", $"Creating assets...\n\n{child.name}", numExported / (float)numObjectsActive))
                    yield break;

                this.ExportAssets(child.gameObject);

                numExported++;
            }

            EditorUtility.DisplayProgressBar("", "Creating prefab...", 1f);
            PrefabUtility.SaveAsPrefabAsset(cell.gameObject, $"{PrefabsPath}/{cell.gameObject.name}.prefab");

            EditorUtility.DisplayProgressBar("", "Refreshing asset database...", 1f);
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("", "Finished !", "Ok");
        }

        void CreateFolders()
        {
            string[] folders = new string[]
            {
                "Models",
                "Materials",
                "Textures",
                "Prefabs",
                "CollisionModels",
            };

            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(Path.Combine(m_selectedFolder, folder)))
                    AssetDatabase.CreateFolder(m_selectedFolder, folder);
            }
        }

        public void ExportAssets(GameObject go)
        {
            string assetName = go.name;

            var meshFilters = go.GetComponentsInChildren<MeshFilter>();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter meshFilter = meshFilters[i];
                string indexPath = meshFilters.Length == 1 ? "" : "-" + i;
                CreateAssetIfNotExists(meshFilter.sharedMesh, $"{ModelsPath}/{assetName}{indexPath}.asset");
            }

            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshRenderers.Length; i++)
            {
                ExportMeshRenderer(go, meshRenderers[i], meshRenderers.Length == 1 ? (int?)null : i);
            }

            var meshColliders = go.GetComponentsInChildren<MeshCollider>();

            for (int i = 0; i < meshColliders.Length; i++)
            {
                string indexPath = meshColliders.Length == 1 ? "" : "-" + i;
                CreateAssetIfNotExists(meshColliders[i].sharedMesh, $"{CollisionModelsPath}/{assetName}{indexPath}.asset");
            }

            //PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabsPath}/{assetName}.prefab");
        }

        public void ExportMeshRenderer(GameObject rootGo, MeshRenderer meshRenderer, int? index)
        {
            string indexPath = index.HasValue ? "-" + index.Value : "";
            string assetName = rootGo.name + indexPath;

            var mats = meshRenderer.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                CreateAssetIfNotExists(mats[i].mainTexture, $"{TexturesPath}/{assetName}-{i}.asset");
                CreateAssetIfNotExists(mats[i], $"{MaterialsPath}/{assetName}-{i}.mat");
            }
        }

        private static bool CreateAssetIfNotExists(Object asset, string path)
        {
            if (AssetDatabase.Contains(asset))
                return false;

            if (!AssetDatabase.IsMainAssetAtPathLoaded(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                return true;
            }

            return false;
        }
    }
}
