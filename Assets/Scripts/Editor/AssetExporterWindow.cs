using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class AssetExporterWindow : EditorWindowBase
    {
        private Vector2 m_scrollViewPos = Vector2.zero;
        private readonly AssetExporter m_assetExporter = new AssetExporter();



        public AssetExporterWindow()
        {
            this.titleContent = new GUIContent("Asset exporter");
            this.minSize = new Vector2(400, 200);
            this.position = new Rect(this.position.center, new Vector2(400, 500));
        }

        [MenuItem(EditorCore.MenuName + "/" + "Asset exporter")]
        static void Init()
        {
            var window = GetWindow<AssetExporterWindow>();
            window.Show();
        }

        void OnGUI()
        {
            m_scrollViewPos = EditorGUILayout.BeginScrollView(m_scrollViewPos);

            EditorGUILayout.HelpBox(
                "This tool can export assets from game into Unity project.\n" +
                "Later you can use these assets inside Unity Editor like any other asset. " +
                "It will store them in a separate folder, and will only export those objects that were not already exported. This means that you can cancel the process, and when you start it next time, it will skip already exported assets.",
                MessageType.Info,
                true);

            GUILayout.Space(30);

            EditorGUILayout.LabelField("Folder where assets are placed: " + m_assetExporter.SelectedFolder);

            m_assetExporter.ExportRenderMeshes = EditorGUILayout.Toggle("Export render meshes", m_assetExporter.ExportRenderMeshes);
            m_assetExporter.ExportMaterials = EditorGUILayout.Toggle("Export materials", m_assetExporter.ExportMaterials);
            m_assetExporter.ExportTextures = EditorGUILayout.Toggle("Export textures", m_assetExporter.ExportTextures);
            m_assetExporter.ExportCollisionMeshes = EditorGUILayout.Toggle("Export collision meshes", m_assetExporter.ExportCollisionMeshes);
            m_assetExporter.ExportPrefabs = EditorGUILayout.Toggle("Export prefabs", m_assetExporter.ExportPrefabs);

            GUILayout.Space(30);

            if (GUILayout.Button("Export from game files"))
                m_assetExporter.Export(AssetExporter.ExportType.FromGameFiles);

            if (GUILayout.Button("Export from world"))
                m_assetExporter.Export(AssetExporter.ExportType.FromLoadedWorld);

            if (GUILayout.Button("Export from selection"))
                m_assetExporter.Export(AssetExporter.ExportType.FromSelection);

            EditorGUILayout.EndScrollView();
        }
    }
}
