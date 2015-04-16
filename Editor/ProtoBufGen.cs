using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace Facepunch.Editor
{
    public class ProtoBufGen : MonoBehaviour
    {
        public static string AssetsPath { get; set; }
        public static string RootPath { get; set; }

        static ProtoBufGen()
        {
            AssetsPath = new DirectoryInfo(UnityEngine.Application.dataPath).FullName;
            RootPath = new DirectoryInfo(Path.Combine(AssetsPath, "..")).FullName;
        }

        [MenuItem("Facepunch/Generate ProtoBuf")]
// ReSharper disable once UnusedMember.Local
        private static void Generate()
        {
            var fileName = "premake5";

            if (UnityEngine.Application.platform == RuntimePlatform.WindowsEditor) {
                fileName = Path.Combine(RootPath, fileName + ".exe");
            }

            using (var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = fileName,
                    WorkingDirectory = RootPath,
                    Arguments = "--protogen",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            }) {
                var timer = new Stopwatch();

                DataReceivedEventHandler hander = (sender, e) => {
                    timer.Reset();
                    timer.Start();

                    if (!e.Data.Contains(": error CS001: ")) return;

                    UnityEngine.Debug.LogError(e.Data.Substring(AssetsPath.Length + 1));
                };

                process.OutputDataReceived += hander;
                process.ErrorDataReceived += hander;

                timer.Start();
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(100)) {
                    if (timer.Elapsed.TotalSeconds > 2.5) {
                        UnityEngine.Debug.LogError("Process timed out while generating ProtoBuf classes");
                        return;
                    }
                }

                if (process.ExitCode == 0) {
                    UnityEngine.Debug.Log("Successfully generated ProtoBuf classes");
                } else {
                    UnityEngine.Debug.LogError("An error occurred while generating ProtoBuf classes");
                }
            }
        }
    }
}
