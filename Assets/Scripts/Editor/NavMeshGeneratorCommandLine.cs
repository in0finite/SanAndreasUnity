using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;
using SanAndreasUnity.Utilities;
using UnityEngine.AI;
using SanAndreasUnity.Behaviours.World;
using SanAndreasUnity.Behaviours;
using UnityEditor.SceneManagement;

namespace SanAndreasUnity.Editor
{
    public static class NavMeshGeneratorCommandLine
    {
        // TODO: remove menu item
        [MenuItem(EditorCore.MenuName + "/" + "Generate nav mesh in batch")]
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

            // open startup scene
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
            yield return null;
            yield return null;

            // load game data

            Loader.StartLoading();

            while (Loader.IsLoading)
                yield return null;

            // TODO: dialog is shown when loading completes, it will block Editor ...

            if (!Loader.HasLoaded)
                throw new Exception("Loader did not finish successfully");

            // use AssetExporter to load game collision

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

            yield return null;

            // we need to create collision for water
            Cell.Singleton.Water.CreateCollisionObjects = true;
            Cell.Singleton.Water.Initialize(Cell.Singleton.WorldSize * Vector2.one);

            // TODO: just some testing, remove ...
            DisableObjects();

            // now fire up NavMeshGenerator

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

            yield return null;

            navMeshGenerator.SaveNavMesh("Assets/GeneratedNavMeshFromCommandLine.asset");

            Debug.Log("Finished generation of nav mesh from command line");

            yield return null;
            yield return null;

            EditorApplication.Exit(0);
        }

        static void OnFinishWithError(Exception exception)
        {
            EditorApplication.Exit(1);
        }

        static void DisableObjects()
        {
            Cell.Singleton.gameObject.GetFirstLevelChildrenSingleComponent<MapObject>().ForEach(mapObject =>
            {
                if (mapObject.transform.Distance(Vector3.zero) > 400)
                    mapObject.gameObject.SetActive(false);
            });
        }
    }
}
