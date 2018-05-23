#if !NOT_UNITY3D

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using ModestTree;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Zenject
{
    public static class ZenMenuItems
    {
        [MenuItem("Edit/Zenject/Validate Current Scenes #%v")]
        public static void ValidateCurrentScene()
        {
            ValidateCurrentSceneInternal();
        }

        [MenuItem("Edit/Zenject/Validate Then Run #%r")]
        public static void ValidateCurrentSceneThenRun()
        {
            if (ValidateCurrentSceneInternal())
            {
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Edit/Zenject/Help...")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/modesttree/zenject");
        }

        [MenuItem("GameObject/Zenject/Scene Context", false, 9)]
        public static void CreateSceneContext(MenuCommand menuCommand)
        {
            var root = new GameObject("SceneContext").AddComponent<SceneContext>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Zenject/Decorator Context", false, 9)]
        public static void CreateDecoratorContext(MenuCommand menuCommand)
        {
            var root = new GameObject("DecoratorContext").AddComponent<SceneDecoratorContext>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("GameObject/Zenject/Game Object Context", false, 9)]
        public static void CreateGameObjectContext(MenuCommand menuCommand)
        {
            var root = new GameObject("GameObjectContext").AddComponent<GameObjectContext>();
            Selection.activeGameObject = root.gameObject;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Edit/Zenject/Create Project Context")]
        public static void CreateProjectContextInDefaultLocation()
        {
            var fullDirPath = Path.Combine(Application.dataPath, "Resources");

            if (!Directory.Exists(fullDirPath))
            {
                Directory.CreateDirectory(fullDirPath);
            }

            CreateProjectContextInternal("Assets/Resources");
        }

        [MenuItem("Assets/Create/Zenject/Scriptable Object Installer", false, 1)]
        public static void CreateScriptableObjectInstaller()
        {
            AddCSharpClassTemplate("Scriptable Object Installer", "UntitledInstaller", false,
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\n[CreateAssetMenu(fileName = \"CLASS_NAME\", menuName = \"Installers/CLASS_NAME\")]"
                + "\npublic class CLASS_NAME : ScriptableObjectInstaller<CLASS_NAME>"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Mono Installer", false, 1)]
        public static void CreateMonoInstaller()
        {
            AddCSharpClassTemplate("Mono Installer", "UntitledInstaller", false,
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : MonoInstaller<CLASS_NAME>"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Installer", false, 1)]
        public static void CreateInstaller()
        {
            AddCSharpClassTemplate("Installer", "UntitledInstaller", false,
                  "using UnityEngine;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : Installer<CLASS_NAME>"
                + "\n{"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Editor Window", false, 20)]
        public static void CreateEditorWindow()
        {
            AddCSharpClassTemplate("Editor Window", "UntitledEditorWindow", true,
                  "using UnityEngine;"
                + "\nusing UnityEditor;"
                + "\nusing Zenject;"
                + "\n"
                + "\npublic class CLASS_NAME : ZenjectEditorWindow"
                + "\n{"
                + "\n    [MenuItem(\"Window/CLASS_NAME\")]"
                + "\n    public static CLASS_NAME GetOrCreateWindow()"
                + "\n    {"
                + "\n        var window = EditorWindow.GetWindow<CLASS_NAME>();"
                + "\n        window.titleContent = new GUIContent(\"CLASS_NAME\");"
                + "\n        return window;"
                + "\n    }"
                + "\n"
                + "\n    public override void InstallBindings()"
                + "\n    {"
                + "\n        // TODO"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Unit Test", false, 60)]
        public static void CreateUnitTest()
        {
            AddCSharpClassTemplate("Unit Test", "UntitledUnitTest", true,
                  "using Zenject;"
                + "\nusing NUnit.Framework;"
                + "\n"
                + "\n[TestFixture]"
                + "\npublic class CLASS_NAME : ZenjectUnitTestFixture"
                + "\n{"
                + "\n    [Test]"
                + "\n    public void RunTest1()"
                + "\n    {"
                + "\n        // TODO"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Integration Test", false, 60)]
        public static void CreateIntegrationTest()
        {
            AddCSharpClassTemplate("Integration Test", "UntitledIntegrationTest", false,
                  "using Zenject;"
                + "\nusing System.Collections;"
                + "\nusing UnityEngine.TestTools;"
                + "\n"
                + "\npublic class CLASS_NAME : ZenjectIntegrationTestFixture"
                + "\n{"
                + "\n    [UnityTest]"
                + "\n    public IEnumerator RunTest1()"
                + "\n    {"
                + "\n        // Setup initial state by creating game objects from scratch, loading prefabs/scenes, etc"
                + "\n"
                + "\n        PreInstall();"
                + "\n"
                + "\n        // Call Container.Bind methods"
                + "\n"
                + "\n        PostInstall();"
                + "\n"
                + "\n        // Add test assertions for expected state"
                + "\n        // Using Container.Resolve or [Inject] fields"
                + "\n        yield break;"
                + "\n    }"
                + "\n}");
        }

        [MenuItem("Assets/Create/Zenject/Project Context", false, 40)]
        public static void CreateProjectContext()
        {
            var absoluteDir = ZenUnityEditorUtil.TryGetSelectedFolderPathInProjectsTab();

            if (absoluteDir == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Could not find directory to place the '{0}.prefab' asset.  Please try again by right clicking in the desired folder within the projects pane."
                    .Fmt(ProjectContext.ProjectContextResourcePath), "Ok");
                return;
            }

            var parentFolderName = Path.GetFileName(absoluteDir);

            if (parentFolderName != "Resources")
            {
                EditorUtility.DisplayDialog("Error",
                    "'{0}.prefab' must be placed inside a directory named 'Resources'.  Please try again by right clicking within the Project pane in a valid Resources folder."
                    .Fmt(ProjectContext.ProjectContextResourcePath), "Ok");
                return;
            }

            CreateProjectContextInternal(absoluteDir);
        }

        static void CreateProjectContextInternal(string absoluteDir)
        {
            var assetPath = ZenUnityEditorUtil.ConvertFullAbsolutePathToAssetPath(absoluteDir);
            var prefabPath = (Path.Combine(assetPath, ProjectContext.ProjectContextResourcePath) + ".prefab").Replace("\\", "/");
            var emptyPrefab = PrefabUtility.CreateEmptyPrefab(prefabPath);

            var gameObject = new GameObject();

            try
            {
                gameObject.AddComponent<ProjectContext>();

                var prefabObj = PrefabUtility.ReplacePrefab(gameObject, emptyPrefab);

                Selection.activeObject = prefabObj;
            }
            finally
            {
                GameObject.DestroyImmediate(gameObject);
            }

            Debug.Log("Created new ProjectContext at '{0}'".Fmt(prefabPath));
        }

        static void AddCSharpClassTemplate(
            string friendlyName, string defaultFileName, bool editorOnly, string templateStr)
        {
            var folderPath = ZenUnityEditorUtil.GetCurrentDirectoryAssetPathFromSelection();

            if (editorOnly && !folderPath.Contains("/Editor"))
            {
                EditorUtility.DisplayDialog("Error",
                    "Editor window classes must have a parent folder above them named 'Editor'.  Please create or find an Editor folder and try again", "Ok");
                return;
            }

            var absolutePath = EditorUtility.SaveFilePanel(
                "Choose name for " + friendlyName,
                folderPath,
                defaultFileName + ".cs",
                "cs");

            if (absolutePath == "")
            {
                // Dialog was cancelled
                return;
            }

            if (!absolutePath.ToLower().EndsWith(".cs"))
            {
                absolutePath += ".cs";
            }

            var className = Path.GetFileNameWithoutExtension(absolutePath);
            File.WriteAllText(absolutePath, templateStr.Replace("CLASS_NAME", className));

            AssetDatabase.Refresh();

            var assetPath = ZenUnityEditorUtil.ConvertFullAbsolutePathToAssetPath(absolutePath);

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
        }

        [MenuItem("Edit/Zenject/Validate All Active Scenes")]
        public static void ValidateAllActiveScenes()
        {
            ValidateWrapper(() =>
                {
                    var numValidated = ZenUnityEditorUtil.ValidateAllActiveScenes();
                    ModestTree.Log.Info("Validated all '{0}' active scenes successfully", numValidated);
                });
        }

        static bool ValidateWrapper(Action action)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                var originalSceneSetup = EditorSceneManager.GetSceneManagerSetup();

                try
                {
                    action();
                    return true;
                }
                catch (Exception e)
                {
                    ModestTree.Log.ErrorException(e);
                    return false;
                }
                finally
                {
                    EditorSceneManager.RestoreSceneManagerSetup(originalSceneSetup);
                }
            }
            else
            {
                Debug.Log("Validation cancelled - All scenes must be saved first for validation to take place");
                return false;
            }
        }

        static bool ValidateCurrentSceneInternal()
        {
            return ValidateWrapper(() =>
                {
                    ZenUnityEditorUtil.ValidateCurrentSceneSetup();
                    ModestTree.Log.Info("All scenes validated successfully");
                });
        }
    }
}
#endif
