using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SanAndreasUnity.Behaviours.World;
using UGameCore.Utilities;
using SanAndreasUnity.Behaviours;
using System.Linq;

namespace SanAndreasUnity.Editor
{
    public class WorldTools
    {
        private static IEnumerable<Transform> GetAllWorldObjects()
        {
            var world = Cell.Singleton;
            if (world == null)
                return Enumerable.Empty<Transform>();

            return world.transform.GetFirstLevelChildren();
        }

        [MenuItem(EditorCore.MenuName + "/World/Select all world objects")]
        static void SelectAllWorldObjects()
        {
            Object[] objectsToSelect = GetAllWorldObjects()
                .Select(_ => (Object)_)
                .ToArrayOfLength(Cell.Singleton.transform.childCount);

            if (objectsToSelect.Length == 0)
                return;

            Selection.objects = objectsToSelect;
        }

        [MenuItem(EditorCore.MenuName + "/World/Select active world objects without renderer")]
        static void SelectObjectsWithoutRenderer()
        {
            GameObject[] objectsToSelect = GetAllWorldObjects()
                .Where(_ => _.gameObject.activeSelf && _.GetComponent<Renderer>() == null)
                .Select(_ => _.gameObject)
                .ToArray();

            if (objectsToSelect.Length == 0)
                return;

            Selection.objects = objectsToSelect;
            EditorGUIUtility.PingObject(objectsToSelect[0]);
        }

        [MenuItem(EditorCore.MenuName + "/World/Enable all world objects")]
        static void EnableAllWorldObjects()
        {
            GetAllWorldObjects().ForEach(_ => _.gameObject.SetActive(true));
        }

        [MenuItem(EditorCore.MenuName + "/World/Disable all world objects")]
        static void DisableAllWorldObjects()
        {
            GetAllWorldObjects().ForEach(_ => _.gameObject.SetActive(false));
        }

        private static IEnumerable<Transform> GetAllWorldObjectsCloseToCamera()
        {
            SceneView lastActiveSceneView = SceneView.lastActiveSceneView;
            if (lastActiveSceneView == null)
                return Enumerable.Empty<Transform>();

            Vector2 pos = lastActiveSceneView.camera.transform.position.ToVec2WithXAndZ();

            return GetAllWorldObjects()
                .Where(_ => Vector2.Distance(_.transform.position.ToVec2WithXAndZ(), pos) < 600f);
        }

        [MenuItem(EditorCore.MenuName + "/World/Enable all world objects close to camera")]
        static void EnableAllWorldObjectsCloseToCamera()
        {
            GetAllWorldObjectsCloseToCamera().ForEach(_ => _.gameObject.SetActive(true));
        }

        [MenuItem(EditorCore.MenuName + "/World/Disable all world objects close to camera")]
        static void DisableAllWorldObjectsCloseToCamera()
        {
            GetAllWorldObjectsCloseToCamera().ForEach(_ => _.gameObject.SetActive(false));
        }
    }
}
