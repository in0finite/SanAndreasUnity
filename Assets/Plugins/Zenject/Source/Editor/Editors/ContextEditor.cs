namespace Zenject
{
    public class ContextEditor : UnityInspectorListEditor
    {
        protected override string[] PropertyNames
        {
            get
            {
                return new string[]
                {
                    "_installers",
                    "_installerPrefabs",
                    "_scriptableObjectInstallers",
                };
            }
        }

        protected override string[] PropertyDisplayNames
        {
            get
            {
                return new string[]
                {
                    "Installers",
                    "Prefab Installers",
                    "Scriptable Object Installers",
                };
            }
        }

        protected override string[] PropertyDescriptions
        {
            get
            {
                return new string[]
                {
                    "Drag any MonoInstallers that you have added to your Scene Hierarchy here.",
                    "Drag any prefabs that contain a MonoInstaller on them here",
                    "Drag any assets in your Project that implement ScriptableObjectInstaller here",
                };
            }
        }
    }
}



