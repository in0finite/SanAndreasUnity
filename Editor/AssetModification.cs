using System.Linq;
using Facepunch.Networking;

namespace Facepunch.Editor
{
    public class AssetModification : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths.Contains(UnityEditor.EditorApplication.currentScene)) {
                Server.AssignEditorIds();
            }

            return paths;
        }
    }
}
