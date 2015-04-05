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
            var dest = Path.Combine(destDir, Utilities.Config.TemplateFileName);
            File.Copy(Utilities.Config.TemplateFilePath, dest, true);
        }
    }
}
