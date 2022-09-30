using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Importing.RenderWareStream;
using UGameCore.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class ImportingMenuTools
    {
        private const string MenuItemPrefix = EditorCore.MenuName + "/Importing/";


        static void DisplayMessage(string msg)
        {
            EditorUtility.DisplayDialog("", msg, "Ok");
        }

        [MenuItem(MenuItemPrefix + "List clumps with collision")]
        static void ListClumpsWithCollision()
        {
            var dffList = ArchiveManager.GetFileNamesWithExtension(".dff");

            if (!EditorUtility.DisplayDialog("", $"Found {dffList.Count} DFF files.\r\nProceed ?", "Ok", "Cancel"))
                return;

            var found = new List<(Clump clump, string dffName)>();

            int i = 0;
            foreach (string fileName in dffList)
            {
                Clump clump = null;
                F.RunExceptionSafe(() => clump = ArchiveManager.ReadFile<Clump>(fileName));
                if (clump != null && clump.Collision != null)
                    found.Add((clump, fileName));

                if (EditorUtility.DisplayCancelableProgressBar("", $"{fileName} , found {found.Count}", i / (float)dffList.Count))
                {
                    break;
                }

                i++;
            }

            string msg = $"Finished searching for clumps with collision: num DFF files {dffList.Count}, num with collision {found.Count}";
            Debug.Log(msg);

            string logMsg = $"DFF | collision name | model id :\r\n{string.Join("\r\n", found.Select(_ => _.dffName + " | " + _.clump.Collision.Name + " | " + _.clump.Collision.ModelId))}";
            Debug.Log(logMsg);

            DisplayMessage(msg);
        }
    }
}
