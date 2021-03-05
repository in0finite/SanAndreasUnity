using System.IO;
using UnityEditor;
using UnityEngine;

namespace SanAndreasUnity.Editor
{
    public class AndroidBuilder
    {
        static void Build ()
        {
            string buildPath = Path.Combine(Application.dataPath, $"../Build/{nameof(SanAndreasUnity)}.apk");

            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, BuildTarget.Android, BuildOptions.None);
        }
    }
}
