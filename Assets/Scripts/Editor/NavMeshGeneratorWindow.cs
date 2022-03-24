using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace SanAndreasUnity.Editor
{
    public class NavMeshGeneratorWindow : EditorWindowBase
    {
        [SerializeField] private NavMeshData m_navMeshData;
        private int m_selectedAgentId = 0;
        private NavMeshBuildSettings m_navMeshBuildSettings;
        private NavMeshGenerator m_navMeshGenerator;



        public NavMeshGeneratorWindow()
        {
            this.titleContent = new GUIContent("Nav mesh generator");
        }

        void OnEnable()
        {
            m_navMeshBuildSettings = NavMesh.GetSettingsByID(m_selectedAgentId);
            m_navMeshGenerator = new NavMeshGenerator(m_navMeshData);
        }

        [MenuItem(EditorCore.MenuName + "/" + "Generate nav mesh")]
        static void Init()
        {
            var window = GetWindow<NavMeshGeneratorWindow>();
            window.Show();
        }

        void OnGUI()
        {
            m_navMeshData = m_navMeshGenerator.NavMeshData;

            EditorGUILayout.HelpBox(
                "Generate nav mesh from loaded world.\n" +
                "Only high LOD world objects and water are included.\n" +
                "Only collision data will be included, rendering data will be ignored.",
                MessageType.Info,
                true);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Agent ID:");
            m_selectedAgentId = EditorGUILayout.IntField(m_selectedAgentId);
            if (GUILayout.Button("Edit"))
                UnityEditor.AI.NavMeshEditorHelpers.OpenAgentSettings(m_selectedAgentId);
            if (GUILayout.Button("Use"))
                m_navMeshBuildSettings = NavMesh.GetSettingsByID(m_selectedAgentId);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            m_navMeshBuildSettings = (NavMeshBuildSettings)EditorUtils.DrawFieldsAndPropertiesInInspector(
                m_navMeshBuildSettings, 0, true);

            GUILayout.Space(20);

            EditorGUILayout.PrefixLabel("Current nav mesh:");
            EditorGUILayout.ObjectField(m_navMeshData, typeof(NavMeshData), false);

            GUILayout.Space(20);

            if (GUILayout.Button("Generate"))
                m_navMeshGenerator.Generate(m_navMeshBuildSettings, false);

            GUI.enabled = m_navMeshData != null;
            if (GUILayout.Button("Save navmesh as asset"))
                m_navMeshGenerator.SaveNavMesh();

            GUI.enabled = m_navMeshData != null;
            if (GUILayout.Button("Clear current navmesh"))
                m_navMeshGenerator.ClearCurrentNavMesh();

            GUI.enabled = true;
            if (GUILayout.Button("Remove all navmeshes"))
                m_navMeshGenerator.RemoveAllNavMeshes();

            GUI.enabled = true;
            if (GUILayout.Button("Cancel build"))
                m_navMeshGenerator.CancelBuild();

            GUI.enabled = true;

            m_navMeshData = m_navMeshGenerator.NavMeshData;
        }
    }
}
