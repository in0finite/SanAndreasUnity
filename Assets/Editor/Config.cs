using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;

namespace SanAndreasUnity.Editor
{
    public class Config : MonoBehaviour
    {
        [PostProcessBuild]
        public static void CopyConfig(BuildTarget target, string pathToBuiltProject)
        {
            var destDir = Path.GetDirectoryName(pathToBuiltProject);

            var dest = Path.Combine(destDir, Utilities.Config.FileName);
            File.Copy(Utilities.Config.FilePath, dest, true);

            dest = Path.Combine(destDir, Utilities.Config.UserFileName);
            File.Copy(Utilities.Config.UserFilePath, dest, true);
        }
    }
}
