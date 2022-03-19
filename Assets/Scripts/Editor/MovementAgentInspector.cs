using SanAndreasUnity.Utilities;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace SanAndreasUnity.Editor
{
    [CustomEditor(typeof(MovementAgent))]
    public class MovementAgentInspector : UnityEditor.Editor
    {
        static string[] s_propertyNames = new string[]
        {
            nameof(NavMeshAgent.hasPath),
            nameof(NavMeshAgent.pathPending),
            nameof(NavMeshAgent.pathStatus),
            nameof(NavMeshAgent.isPathStale),
            nameof(NavMeshAgent.isOnNavMesh),
            nameof(NavMeshAgent.isStopped),
            nameof(NavMeshAgent.remainingDistance),
            nameof(NavMeshAgent.desiredVelocity),
            nameof(NavMeshAgent.velocity),
        };

        static string[] s_propertyNamesToExcludeWhenNotOnNavMesh = new string[]
        {
            nameof(NavMeshAgent.isStopped),
            nameof(NavMeshAgent.remainingDistance),
        };

        static PropertyInfo[] s_propertyInfos = s_propertyNames
            .Select(n => typeof(NavMeshAgent).GetProperty(n, BindingFlags.Instance | BindingFlags.Public))
            .ToArray();

        static PropertyInfo[] s_propertyInfosWhenNotOnNavMesh = s_propertyNames.Except(s_propertyNamesToExcludeWhenNotOnNavMesh)
            .Select(n => typeof(NavMeshAgent).GetProperty(n, BindingFlags.Instance | BindingFlags.Public))
            .ToArray();


        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("Nav mesh agent info:");
            GUILayout.Space(10);

            var movementAgent = (MovementAgent)this.target;

            if (movementAgent.NavMeshAgent != null)
            {
                bool isOnNavMesh = movementAgent.NavMeshAgent.isOnNavMesh;
                EditorUtils.DrawPropertiesInInspector(movementAgent.NavMeshAgent, isOnNavMesh ? s_propertyInfos : s_propertyInfosWhenNotOnNavMesh, false);
                EditorGUILayout.LabelField("Diff between simulation position: " + (movementAgent.NavMeshAgent.nextPosition - movementAgent.transform.position));
            }
        }

    }
}