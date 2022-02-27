using SanAndreasUnity.Behaviours;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class LocalPedMenuTools
    {
        private const string LocalPedMenuItemPrefix = EditorCore.MenuName + "/Local ped/";


        static void DisplayMessage(string msg)
        {
            EditorUtility.DisplayDialog("", msg, "Ok");
        }

        static Ped GetLocalPedOrShowMessage()
        {
            if (null == Ped.LocalPed)
            {
                DisplayMessage("Local ped not found");
                return null;
            }

            return Ped.LocalPed;
        }

        [MenuItem(LocalPedMenuItemPrefix + "Select local ped")]
        static void Select()
        {
            var localPed = GetLocalPedOrShowMessage();
            if (null == localPed)
                return;

            Selection.activeTransform = localPed.transform;
            EditorGUIUtility.PingObject(localPed);
        }

        [MenuItem(LocalPedMenuItemPrefix + "Move to SceneView camera")]
        static void MoveToCamera()
        {
            var localPed = GetLocalPedOrShowMessage();
            if (null == localPed)
                return;

            var cam = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.camera
                : null;

            if (null == cam)
            {
                DisplayMessage("Failed to find SceneView camera");
                return;
            }

            localPed.transform.position = cam.transform.position;
        }

        [MenuItem(LocalPedMenuItemPrefix + "Move to selection")]
        static void MoveToSelection()
        {
            var localPed = GetLocalPedOrShowMessage();
            if (null == localPed)
                return;

            Transform[] selected = Selection.transforms;

            if (selected.Length != 1)
            {
                DisplayMessage("Select exactly 1 scene object");
                return;
            }

            localPed.transform.position = selected[0].position;
        }
    }
}
