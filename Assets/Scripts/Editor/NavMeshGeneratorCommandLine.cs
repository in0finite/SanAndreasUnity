using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using SanAndreasUnity.Utilities;
using UnityEngine.AI;

namespace SanAndreasUnity.Editor
{
    public static class NavMeshGeneratorCommandLine
    {
        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            // we need to check this before starting coroutine, or otherwise Editor may exit
            if (args.Contains("-quit"))
            {
                throw new ArgumentException("Nav mesh generation from command line can not be used with '-quit' argument. " +
                    "Remove the argument, and Editor will be closed when nav mesh generation is finished.");
            }

            CoroutineManager.Start(RunCoroutine(), null, OnFinishWithError);
        }

        private static IEnumerator RunCoroutine()
        {
            yield return null;

            Debug.Log("Started nav mesh generation ...");

            // skip loading models and textures
            Config.SetString("loadStaticRenderModels", false.ToString());
            Config.SetString("dontLoadTextures", true.ToString());

            var assetExporter = new AssetExporter
            {
                ExportPrefabs = false,
                ExportRenderMeshes = false,
                ExportTextures = false,
                ExportMaterials = false,
                ExportCollisionMeshes = false,
                IsSilentMode = true
            };

            assetExporter.Export(AssetExporter.ExportType.FromGameFiles);

            while (assetExporter.IsRunning)
            {
                yield return null;
            }

            if (!assetExporter.FinishedSuccessfully)
                throw new Exception("Asset exporter did not finish successfully");

            var navMeshGenerator = new NavMeshGenerator(null);

            var navMeshBuildSettings = NavMesh.GetSettingsByID(0);
            navMeshBuildSettings.maxJobWorkers = CmdLineUtils.TryGetUshortArgument("navMeshGenerationMaxJobWorkers", out ushort maxJobWorkers) ? maxJobWorkers : (uint)2;

            navMeshGenerator.Generate(navMeshBuildSettings, true);

            while (navMeshGenerator.IsRunning)
            {
                yield return null;
            }

            if (!navMeshGenerator.FinishedSuccessfully)
                throw new Exception("Nav mesh generator did not finish successfully");

            navMeshGenerator.SaveNavMesh("Assets/GeneratedNavMeshFromCommandLine.asset");

            Debug.Log("Finished generator of nav mesh from command line");

            //EditorApplication.Exit(0);
        }

        static void OnFinishWithError(Exception exception)
        {
            //EditorApplication.Exit(1);
        }
    }
}
