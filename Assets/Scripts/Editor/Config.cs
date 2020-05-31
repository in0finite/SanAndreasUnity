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
			if (!IsStandaloneTarget (target))
				return;

            var destDir = Path.GetDirectoryName(pathToBuiltProject);

			// copy config file

            var dest = Path.Combine(destDir, Utilities.Config.FileName);
            File.Copy(Utilities.Config.FilePath, dest, true);

			// copy Data folder

            var dataDir = SanAndreasUnity.Utilities.Config.DataPath;

			dest = Path.Combine(destDir, Path.GetFileNameWithoutExtension(pathToBuiltProject) + "_Data");

            dest = Path.Combine(dest, "Data");

            if (Directory.Exists(dest))
            {
                Directory.Delete(dest);
            }

            FileUtil.CopyFileOrDirectory(dataDir, dest);
        }

		private static bool IsStandaloneTarget (BuildTarget target)
		{
			switch (target)
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
				case BuildTarget.StandaloneLinux:
				case BuildTarget.StandaloneLinux64:
				case BuildTarget.StandaloneLinuxUniversal:
				case BuildTarget.StandaloneOSX:
					return true;
			}

			return false;
		}

    }
}