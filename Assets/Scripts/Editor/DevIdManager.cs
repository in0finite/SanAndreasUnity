using UnityEditor;

namespace SanAndreasUnity.Editor
{
    [InitializeOnLoad]
    public class DevIdManager
    {
        static DevIdManager()
        {
            DevProfiles.CheckDevProfiles(() => EditorUtility.OpenFolderPanel("Select GTA instalation Path", "", ""));
        }
    }
}