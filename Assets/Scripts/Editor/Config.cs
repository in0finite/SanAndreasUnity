using System;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

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

            var dataDir = SanAndreasUnity.Utilities.Config.DataPath;

            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneLinux:
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneLinuxUniversal:
                    dest = Path.Combine(destDir, Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");
                    break;

                default:
                    throw new NotImplementedException(String.Format("Build target '{0}' is not supported.", target));
            }

            dest = Path.Combine(dest, "Data");

            if (Directory.Exists(dest))
            {
                Directory.Delete(dest);
            }

            FileUtil.CopyFileOrDirectory(dataDir, dest);
        }
    }
}