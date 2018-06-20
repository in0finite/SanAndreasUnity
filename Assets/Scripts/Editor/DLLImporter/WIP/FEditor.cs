using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

//using UnityEditor.PostProcessing;
using UnityEngine;
using UnityObject = UnityEngine.Object;

public static class FEditor
{
    public static void EditAction(this CheckmarkMenuItem item)
    {
        item.actionDict["edit"]();
    }

    public static List<Enum> EnumGetNonObsoleteValues(this Type type)
    {
        // each enum value has the same position in both values and names arrays
        string[] names = Enum.GetNames(type);
        Enum[] values = Enum.GetValues(type).Cast<Enum>().ToArray();
        var result = new List<Enum>();
        for (int i = 0; i < names.Length; i++)
        {
            var info = type.GetMember(names[i]);
            var attrs = info[0].GetCustomAttributes(typeof(ObsoleteAttribute), false);
            var isObsolete = false;
            foreach (var attr in attrs)
            {
                if (attr is ObsoleteAttribute)
                    isObsolete = true;
            }
            if (!isObsolete)
                result.Add(values[i]);
        }
        return result;
    }

    internal static string GetBuildTargetGroupName(BuildTarget target)
    {
        return GetBuildTargetGroupName(GetBuildTargetGroup(target));
    }

    [FreeFunction]
    internal static extern string GetBuildTargetGroupName(BuildTargetGroup buildTargetGroup);

    [FreeFunction(IsThreadSafe = true)]
    public static extern BuildTargetGroup GetBuildTargetGroup(BuildTarget platform);

    [FreeFunction]
    internal static extern string GetBuildTargetGroupDisplayName(BuildTargetGroup targetPlatformGroup);

    [FreeFunction(IsThreadSafe = true)]
    internal static extern string GetEditorTargetName();

    [FreeFunction("GetBuildTargetUniqueName", IsThreadSafe = true)]
    internal static extern string GetBuildTargetName(BuildTarget targetPlatform);

    [FreeFunction]
    internal static extern BuildTarget GetBuildTargetByName(string platform);

    [NativeMethod("GetCompatibleWithPlatformOrAnyPlatform")]
    extern static internal bool GetCompatibleWithPlatformOrAnyPlatformBuildTarget(this AssetImporter imp, string buildTarget);
}

/*public class FEditorGUI : GUIUtility
{
    internal const float kSingleLineHeight = 16;

    private static Hashtable s_TextGUIContents = new Hashtable();
    private static Hashtable s_GUIContents = new Hashtable();

    internal static GUIContent TextContentWithIcon(string textAndTooltip, string icon)
    {
        if (textAndTooltip == null)
            textAndTooltip = "";

        if (icon == null)
            icon = "";

        string key = string.Format("{0}|{1}", textAndTooltip, icon);

        GUIContent gc = (GUIContent)s_TextGUIContents[key];
        if (gc == null)
        {
            string[] strings = GetNameAndTooltipString(textAndTooltip);
            gc = new GUIContent(strings[1]) { image = LoadIconRequired(icon) };

            // We want to catch missing icons so we can fix them (therefore using LoadIconRequired)

            if (strings[2] != null)
            {
                gc.tooltip = strings[2];
            }
            s_TextGUIContents[key] = gc;
        }
        return gc;
    }

    internal static string[] GetNameAndTooltipString(string nameAndTooltip)
    {
        string[] retval = new string[3];

        string[] s1 = nameAndTooltip.Split('|');

        switch (s1.Length)
        {
            case 0:         // Got an empty line... A comment or sth???
                retval[0] = "";
                retval[1] = "";
                break;

            case 1:
                retval[0] = s1[0].Trim();
                retval[1] = retval[0];
                break;

            case 2:
                retval[0] = s1[0].Trim();
                retval[1] = retval[0];
                retval[2] = s1[1].Trim();
                break;

            default:
                Debug.LogError("Error in Tooltips: Too many strings in line beginning with '" + s1[0] + "'");
                break;
        }
        return retval;
    }

    internal static Texture2D LoadIconRequired(string name)
    {
        Texture2D tex = LoadIcon(name);

        //if (!tex)
        //    Debug.LogErrorFormat("Unable to load the icon: '{0}'.\nNote that either full project path should be used (with extension) " +
        //        "or just the icon name if the icon is located in the following location: '{1}' (without extension, since png is assumed)",
        //        name, EditorResources.editorDefaultResourcesPath + EditorResources.iconsPath);

        return tex;
    }

    public static GUIContent TrTextContent(string key, string text, string tooltip, Texture icon)
    {
        GUIContent gc = (GUIContent)s_GUIContents[key];
        if (gc == null)
        {
            gc = new GUIContent(L10n.Tr(text));
            if (tooltip != null)
            {
                gc.tooltip = L10n.Tr(tooltip);
            }
            if (icon != null)
            {
                gc.image = icon;
            }
            s_GUIContents[key] = gc;
        }
        return gc;
    }

    public static GUIContent TrTextContent(string text, string tooltip = null, Texture icon = null)
    {
        string key = string.Format("{0}|{1}", text ?? "", tooltip ?? "");
        return TrTextContent(key, text, tooltip, icon);
    }

    public static GUIContent TrTextContent(string text, string tooltip, string iconName)
    {
        string key = string.Format("{0}|{1}|{2}", text ?? "", tooltip ?? "", iconName ?? "");
        return TrTextContent(key, text, tooltip, LoadIconRequired(iconName));
    }

    public static GUIContent TrTextContent(string text, Texture icon)
    {
        return TrTextContent(text, null, icon);
    }

    public static GUIContent TrTextContentWithIcon(string text, Texture icon)
    {
        return TrTextContent(text, null, icon);
    }

    public static GUIContent TrTextContentWithIcon(string text, string iconName)
    {
        return TrTextContent(text, null, iconName);
    }

    public static GUIContent TrTextContentWithIcon(string text, string tooltip, string iconName)
    {
        return TrTextContent(text, tooltip, iconName);
    }

    public static GUIContent TrTextContentWithIcon(string text, string tooltip, Texture icon)
    {
        return TrTextContent(text, tooltip, icon);
    }

    public static GUIContent TrTextContentWithIcon(string text, string tooltip, MessageType messageType)
    {
        return TrTextContent(text, tooltip, GetHelpIcon(messageType));
    }

    public static GUIContent TrTextContentWithIcon(string text, MessageType messageType)
    {
        return TrTextContentWithIcon(text, null, messageType);
    }

    private static Texture2D s_InfoIcon;
    private static Texture2D s_WarningIcon;
    private static Texture2D s_ErrorIcon;

    internal static Texture2D infoIcon => s_InfoIcon ?? (s_InfoIcon = LoadIcon("console.infoicon"));
    internal static Texture2D warningIcon => s_WarningIcon ?? (s_WarningIcon = LoadIcon("console.warnicon"));
    internal static Texture2D errorIcon => s_ErrorIcon ?? (s_ErrorIcon = LoadIcon("console.erroricon"));

    internal static Texture2D GetHelpIcon(MessageType type)
    {
        switch (type)
        {
            case MessageType.Info:
                return infoIcon;

            case MessageType.Warning:
                return warningIcon;

            case MessageType.Error:
                return errorIcon;
        }
        return null;
    }

    // Automatically loads version of icon that matches current skin.
    // Equivalent to Texture2DNamed in ObjectImages.cpp
    internal static Texture2D LoadIcon(string name)
    {
        return LoadIconForSkin(name, skinIndex);
    }

    internal static Texture2D LoadIconForSkin(string name, int in_SkinIndex)
    {
        if (String.IsNullOrEmpty(name))
            return null;

        if (in_SkinIndex == 0)
            return LoadGeneratedIconOrNormalIcon(name);

        //Remap file name for dark skin
        var newName = "d_" + Path.GetFileName(name);
        var dirName = Path.GetDirectoryName(name);
        if (!String.IsNullOrEmpty(dirName))
            newName = String.Format("{0}/{1}", dirName, newName);

        Texture2D tex = LoadGeneratedIconOrNormalIcon(newName);
        if (!tex)
            tex = LoadGeneratedIconOrNormalIcon(name);
        return tex;
    }

    // Attempts to load a higher resolution icon if needed
    private static Texture2D LoadGeneratedIconOrNormalIcon(string name)
    {
        Texture2D icon = null;
        if (GUIUtility.pixelsPerPoint > 1.0f)
        {
            icon = InnerLoadGeneratedIconOrNormalIcon(name + "@2x");
            if (icon != null)
            {
                icon.pixelsPerPoint = 2.0f;
            }
        }

        if (icon == null)
        {
            icon = InnerLoadGeneratedIconOrNormalIcon(name);
        }

        if (icon != null && !Mathf.Approximately(icon.pixelsPerPoint, GUIUtility.pixelsPerPoint))
        {
            icon.filterMode = FilterMode.Bilinear;
        }

        return icon;
    }

    // Takes a name that already includes d_ if dark skin version is desired.
    // Equivalent to Texture2DSkinNamed in ObjectImages.cpp
    private static Texture2D InnerLoadGeneratedIconOrNormalIcon(string name)
    {
        Texture2D tex = Load(EditorResources.generatedIconsPath + name + ".asset") as Texture2D;

        if (!tex)
        {
            tex = Load(EditorResources.iconsPath + name + ".png") as Texture2D;
        }
        if (!tex)
        {
            tex = Load(name) as Texture2D; // Allow users to specify their own project path to an icon (e.g see EditorWindowTitleAttribute)
        }

        return tex;
    }

    // Load a built-in resource
    public static UnityObject Load(string path)
    {
        return Load(path, typeof(UnityObject));
    }

    private static UnityObject Load(string filename, Type type)
    {
        return AssetDatabase.LoadAssetAtPath(filename, type);
    }
}*/

internal interface IPluginImporterExtension
{
    // Functions use by DLLFileWrapperInspector
    void ResetValues(DLLFileWrapperInspector inspector);

    bool HasModified(DLLFileWrapperInspector inspector);

    void Apply(DLLFileWrapperInspector inspector);

    void OnEnable(DLLFileWrapperInspector inspector);

    void OnDisable(DLLFileWrapperInspector inspector);

    void OnPlatformSettingsGUI(DLLFileWrapperInspector inspector);

    // Called before building the player, checks if plugins don't overwrite each other
    string CalculateFinalPluginPath(string buildTargetName, PluginImporter imp);

    bool CheckFileCollisions(string buildTargetName);
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
[VisibleToOtherModules]
internal class VisibleToOtherModulesAttribute : Attribute
{
    // This attributes controls visibility of internal types and members to other modules.
    // See https://confluence.hq.unity3d.com/display/DEV/Modular+UnityEngine+managed+assemblies+setup for details.
    public VisibleToOtherModulesAttribute()
    {
    }

    public VisibleToOtherModulesAttribute(params string[] modules)
    {
    }
}

internal interface IBindingsAttribute
{
}

internal interface IBindingsNameProviderAttribute : IBindingsAttribute
{
    string Name { get; set; }
}

internal interface IBindingsHeaderProviderAttribute : IBindingsAttribute
{
    string Header { get; set; }
}

internal interface IBindingsIsThreadSafeProviderAttribute : IBindingsAttribute
{
    bool IsThreadSafe { get; set; }
}

internal interface IBindingsIsFreeFunctionProviderAttribute : IBindingsAttribute
{
    bool IsFreeFunction { get; set; }
    bool HasExplicitThis { get; set; }
}

internal interface IBindingsThrowsProviderAttribute : IBindingsAttribute
{
    bool ThrowsException { get; set; }
}

internal interface IBindingsWritableSelfProviderAttribute : IBindingsAttribute
{
    bool WritableSelf { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
[VisibleToOtherModules]
public class FreeFunctionAttribute : NativeMethodAttribute
{
    public FreeFunctionAttribute()
    {
        IsFreeFunction = true;
    }

    public FreeFunctionAttribute(string name) : base(name, true)
    {
    }

    public FreeFunctionAttribute(string name, bool isThreadSafe) : base(name, true, isThreadSafe)
    {
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
[VisibleToOtherModules]
public class NativeMethodAttribute : Attribute, IBindingsNameProviderAttribute, IBindingsIsThreadSafeProviderAttribute, IBindingsIsFreeFunctionProviderAttribute, IBindingsThrowsProviderAttribute
{
    public string Name { get; set; }
    public bool IsThreadSafe { get; set; }
    public bool IsFreeFunction { get; set; }
    public bool ThrowsException { get; set; }
    public bool HasExplicitThis { get; set; }
    public bool WritableSelf { get; set; }

    public NativeMethodAttribute()
    {
    }

    public NativeMethodAttribute(string name)
    {
        if (name == null) throw new ArgumentNullException("name");
        if (name == "") throw new ArgumentException("name cannot be empty", "name");

        Name = name;
    }

    public NativeMethodAttribute(string name, bool isFreeFunction) : this(name)
    {
        IsFreeFunction = isFreeFunction;
    }

    public NativeMethodAttribute(string name, bool isFreeFunction, bool isThreadSafe) : this(name, isFreeFunction)
    {
        IsThreadSafe = isThreadSafe;
    }

    public NativeMethodAttribute(string name, bool isFreeFunction, bool isThreadSafe, bool throws) : this(name, isFreeFunction, isThreadSafe)
    {
        ThrowsException = throws;
    }
}

public static class ModuleManager
{
    /*private static void LoadUnityExtensions()
    {
        foreach (Unity.DataContract.PackageInfo extension in s_PackageManager.unityExtensions)
        {
            if (EnableLogging)
                Console.WriteLine("Setting {0} v{1} for Unity v{2} to {3}", extension.name, extension.version, extension.unityVersion, extension.basePath);
            foreach (var file in extension.files.Where(f => f.Value.type == PackageFileType.Dll))
            {
                string fullPath = Paths.NormalizePath(Path.Combine(extension.basePath, file.Key));
                if (!File.Exists(fullPath))
                    Debug.LogWarningFormat("Missing assembly \t{0} for {1}. Extension support may be incomplete.", file.Key, extension.name);
                else
                {
                    bool isExtension = !String.IsNullOrEmpty(file.Value.guid);
                    if (EnableLogging)
                        Console.WriteLine("  {0} ({1}) GUID: {2}",
                            file.Key,
                            isExtension ? "Extension" : "Custom",
                            file.Value.guid);
                    if (isExtension)
                        InternalEditorUtility.RegisterExtensionDll(fullPath.Replace('\\', '/'), file.Value.guid);
                    else
                        InternalEditorUtility.RegisterPrecompiledAssembly(Path.GetFileName(fullPath), fullPath);
                }
            }
            s_PackageManager.LoadPackage(extension);
        }

        internal static void InitializeModuleManager()
        {
            if (s_PackageManager == null)
            {
                RegisterPackageManager();
                if (s_PackageManager != null)
                    LoadUnityExtensions();
                else
                    Debug.LogError("Failed to load package manager");
            }
        }

        private static void RegisterPlatformSupportModules()
        {
            if (s_PlatformModules != null)
            {
                //Console.WriteLine("Modules already registered, not loading");
                return;
            }
            //Console.WriteLine("Registering platform support modules:");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            s_PlatformModules = RegisterModulesFromLoadedAssemblies<IPlatformSupportModule>(RegisterPlatformSupportModulesFromAssembly).ToList();

            stopwatch.Stop();
            //Console.WriteLine("Registered platform support modules in: " + stopwatch.Elapsed.TotalSeconds + "s.");
        }

    internal static IEnumerable<IPlatformSupportModule> platformSupportModules
    {
        get
        {
            InitializeModuleManager();
            if (s_PlatformModules == null)
                RegisterPlatformSupportModules();
            return s_PlatformModules;
        }
    }*/

    // This is for the smooth transition to future generic target names without subtargets
    // This has to match IPlatformSupportModule.TargetName - not sure how this improves modularity...
    // ADD_NEW_PLATFORM_HERE
    /*internal static string GetTargetStringFromBuildTarget(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.iOS: return "iOS";
            case BuildTarget.tvOS: return "tvOS";
            case BuildTarget.XboxOne: return "XboxOne";
            case BuildTarget.WSAPlayer: return "Metro";
            case BuildTarget.PSP2: return "PSP2";
            case BuildTarget.PS4: return "PS4";
            case BuildTarget.WebGL: return "WebGL";
            case BuildTarget.Android: return "Android";
            case BuildTarget.N3DS: return "N3DS";
            case BuildTarget.Switch: return "Switch";
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                return "LinuxStandalone";

            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "WindowsStandalone";

            case BuildTarget.StandaloneOSX:
            // Deprecated
#pragma warning disable 612, 618
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
#pragma warning restore 612, 618
                return "OSXStandalone";

            default: return null;
        }
    }

    // This is for the smooth transition to future generic target names without subtargets
    internal static string GetTargetStringFromBuildTargetGroup(BuildTargetGroup target)
    {
        // ADD_NEW_PLATFORM_HERE
        switch (target)
        {
            case BuildTargetGroup.iOS: return "iOS";
            case BuildTargetGroup.tvOS: return "tvOS";
            case BuildTargetGroup.XboxOne: return "XboxOne";
            case BuildTargetGroup.WSA: return "Metro";
            case BuildTargetGroup.PSP2: return "PSP2";
            case BuildTargetGroup.PS4: return "PS4";
            case BuildTargetGroup.WebGL: return "WebGL";
            case BuildTargetGroup.Android: return "Android";
            case BuildTargetGroup.N3DS: return "N3DS";
            case BuildTargetGroup.Facebook: return "Facebook";
            case BuildTargetGroup.Switch: return "Switch";
            default: return null;
        }
    }

    internal static IPluginImporterExtension GetPluginImporterExtension(string target)
    {
        if (target == null)
            return null;

        foreach (var module in platformSupportModules)
        {
            if (module.TargetName == target)
                return module.CreatePluginImporterExtension();
        }

        return null;
    }

    internal static IPluginImporterExtension GetPluginImporterExtension(BuildTargetGroup target)
    {
        return GetPluginImporterExtension(GetTargetStringFromBuildTargetGroup(target));
    }*/
}

/*internal interface IPlatformSupportModule
{
    /// Returns name identifying a target, for ex., Metro, note this name should match prefix
    /// for extension module UnityEditor.Metro.Extensions.dll, UnityEditor.Metro.Extensions.Native.dll
    string TargetName { get; }

    /// Returns the filename of jam which should be executed when you're recompiling extensions
    /// from Editor using CTRL + L shortcut, for ex., WP8EditorExtensions, MetroEditorExtensions, etc
    string JamTarget { get; }

    /// Returns an array of native libraries that are required by the extension and must be loaded
    /// by the editor.
    ///
    /// NOTE: If two different platform extensions return a native library with a same file name
    /// (regardless of the path), then only first one will be loaded. This is due to the fact that
    /// some platforms may require same native library, but we must ship a copy with both platforms,
    /// since our modularization and platform installers don't support shared stuff.
    string[] NativeLibraries { get; }

    /// Returns an array of assemblies that should be referenced by user's scripts. These will be
    /// referenced by editor scripts, and game scripts running in editor. Used to export additional
    /// platform specific editor API.
    string[] AssemblyReferencesForUserScripts { get; }

    // Returns an array of assemblies that should be included into C# project as references.
    // This is different from AssemblyReferencesForUserScripts by that most assembly references
    // are internal and not added to the C# project. On the other hand, certain assemblies
    // contain public API, and thus should be added to C# project.
    string[] AssemblyReferencesForEditorCsharpProject { get; }

    /// A human friendly version (eg. an incrementing number on each release) of the platform extension. Null/Empty if none available
    string ExtensionVersion { get; }

    // Names of displays to show in GameView and Camera inspector if the platform supports multiple displays. Return null if default names should be used.
    GUIContent[] GetDisplayNames();

    IPluginImporterExtension CreatePluginImporterExtension();

    // Called when build target supplied by this module is activated in the editor.
    //
    // NOTE: Keep in mind that due domain reloads and the way unity builds, calls on OnActive
    //     and OnDeactivate will be forced even if current build target isn't being changed.
    //
    // PERFORMANCE: This method will be called each time user starts the game, so use this
    //     only for lightweight code, like registering to events, etc.
    //
    // Currently (de)activation happens when:
    //     * User switches build target.
    //     * User runs build for current target.
    //     * Build is run through scripting API.
    //     * Scripts are recompiled and reloaded (due user's change, forced reimport, etc).
    //     * User clicks play in editor.
    //     * ... and possibly more I'm not aware of.
    void OnActivate();

    // Called when build target supplied by this module is deactivated in the editor.
    //
    // NOTE: Keep in mind that due domain reloads and the way unity builds, calls on OnActive
    //     and OnDeactivate will be forced even if current build target isn't being changed.
    //
    // PERFORMANCE: This method will be called each time user starts the game, so use this
    //     only for lightweight code, like unregistering from events, etc.
    //
    // For more info see OnActivate().
    void OnDeactivate();

    // Called when extension is loaded, on editor start or domain reload.
    //
    // PERFORMANCE: This will be called for all available platform extensions during each
    //     domain reload, including each time user starts the game, so use this only for
    //     lightweight code.
    void OnLoad();

    // Called when extension is unloaded, when editor is exited or before domain reload.
    //
    // PERFORMANCE: This will be called for all available platform extensions during each
    //     domain reload, including each time user starts the game, so use this only for
    //     lightweight code.
    void OnUnload();
}*/