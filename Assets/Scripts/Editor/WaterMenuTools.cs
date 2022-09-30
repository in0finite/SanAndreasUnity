using SanAndreasUnity.Behaviours.World;
using UGameCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class WaterMenuTools
    {
        private const string WaterMenuItemPrefix = EditorCore.MenuName + "/Water/";


        static void DisplayMessage(string msg)
        {
            EditorUtility.DisplayDialog("", msg, "Ok");
        }

        static Water GetWaterOrShowMessage()
        {
            if (null == Cell.Singleton || null == Cell.Singleton.Water)
            {
                DisplayMessage("Water object not found");
                return null;
            }

            return Cell.Singleton.Water;
        }

        static void SetSelection(GameObject[] objects)
        {
            Selection.objects = objects;
            if (objects.Length > 0)
                EditorGUIUtility.PingObject(objects[0]);
        }

        [MenuItem(WaterMenuItemPrefix + "Initialize water")]
        static void InitWater()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            water.Initialize(Vector2.one * Cell.Singleton.WorldSize);
        }

        [MenuItem(WaterMenuItemPrefix + "Remove water")]
        static void RemoveWater()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            water.transform.GetFirstLevelChildrenPreallocated().ForEach(_ => UnityEngine.Object.DestroyImmediate(_.gameObject));
        }

        [MenuItem(WaterMenuItemPrefix + "Select water faces with non-4 vertices")]
        static void SelectNon4WaterFaces()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            var waterFaceInfos = water
                .GetComponentsInChildren<WaterFaceInfo>()
                .Where(_ => _.WaterFace.Vertices.Length != 4)
                .Select(_ => _.gameObject)
                .ToArray();

            SetSelection(waterFaceInfos);
        }

        [MenuItem(WaterMenuItemPrefix + "Select shallow water faces")]
        static void SelectShallowWaterFaces()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            var waterFaceInfos = water
                .GetComponentsInChildren<WaterFaceInfo>()
                .Where(_ => (_.WaterFace.Flags & Importing.Items.Placements.WaterFlags.Shallow) != 0)
                .Select(_ => _.gameObject)
                .ToArray();

            SetSelection(waterFaceInfos);
        }

        [MenuItem(WaterMenuItemPrefix + "Select water faces in interiors")]
        static void SelectWaterFacesInInteriors()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            var waterFaceInfos = water
                .GetComponentsInChildren<WaterFaceInfo>()
                .Where(_ => water.IsInterior(_.WaterFace))
                .Select(_ => _.gameObject)
                .ToArray();

            SetSelection(waterFaceInfos);
        }

        [MenuItem(WaterMenuItemPrefix + "Select water face by camera raycast")]
        static void SelectWaterFaceByCameraRaycast()
        {
            var cam = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.camera
                : null;

            if (null == cam)
            {
                DisplayMessage("Failed to find SceneView camera");
                return;
            }

            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, 10000f, Water.LayerMask, QueryTriggerInteraction.Collide))
                return;

            if (hit.transform.GetComponent<WaterFaceInfo>() == null)
                return;

            SetSelection(new GameObject[] { hit.transform.gameObject });
        }

        [MenuItem(WaterMenuItemPrefix + "Add focus point to every water face")]
        static void AddFocusPointToFaces()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            var waterFaceInfos = water.GetComponentsInChildren<WaterFaceInfo>();

            foreach (var waterFaceInfo in waterFaceInfos)
            {
                var focusPoint = waterFaceInfo.gameObject.GetComponent<FocusPoint>();
                if (focusPoint != null)
                    UnityEngine.Object.DestroyImmediate(focusPoint.gameObject);
                focusPoint = waterFaceInfo.gameObject.AddComponent<FocusPoint>();
                focusPoint.parameters = FocusPointParameters.Default;
            }
        }

        [MenuItem(WaterMenuItemPrefix + "Test collision against ground beneath")]
        static void TestCollision()
        {
            var water = GetWaterOrShowMessage();
            if (null == water)
                return;

            var waterFaceInfos = water.GetComponentsInChildren<WaterFaceInfo>();

            var collidingFaces = new List<GameObject>();

            foreach (var waterFaceInfo in waterFaceInfos)
            {
                var boxCollider = waterFaceInfo.GetComponentOrThrow<BoxCollider>();
                if (Physics.CheckBox(
                    waterFaceInfo.transform.position.WithAddedY(-20f),
                    boxCollider.size * 0.5f,
                    waterFaceInfo.transform.rotation,
                    Behaviours.GameManager.DefaultLayerMask,
                    QueryTriggerInteraction.Collide))
                    collidingFaces.Add(waterFaceInfo.gameObject);
            }

            if (collidingFaces.Count > 0)
                SetSelection(collidingFaces.ToArray());
        }
    }
}
