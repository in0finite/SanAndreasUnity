using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanAndreasUnity.Importing.Archive
{
    public static class ResourceManager
    {
        private static readonly List<ImageArchive> _sLoadedArchives = new List<ImageArchive>();

        public static void LoadArchive(string filePath)
        {
            _sLoadedArchives.Add(ImageArchive.Load(filePath));
        }

        public static bool FileExists(string name)
        {
            return _sLoadedArchives.Any(x => x.ContainsFile(name));
        }

        public static Stream ReadFile(string name)
        {
            var arch = _sLoadedArchives.FirstOrDefault(x => x.ContainsFile(name));
            return arch != null ? arch.ReadFile(name) : null;
        }
    }
}
