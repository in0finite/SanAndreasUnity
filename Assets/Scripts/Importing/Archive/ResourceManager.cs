using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Importing.Sections;

namespace SanAndreasUnity.Importing.Archive
{
    public static class ResourceManager
    {
        public const string GameDir = @"C:\Program Files (x86)\Steam\SteamApps\common\Grand Theft Auto San Andreas";

        public static string ModelsDir { get { return Path.Combine(GameDir, "models"); } }
        public static string DataDir { get { return Path.Combine(GameDir, "data"); } }

        public static string GetPath(params string[] relative)
        {
            return relative.Aggregate(GameDir, Path.Combine);
        }

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
            if (arch == null) throw new FileNotFoundException(name);
            return arch.ReadFile(name);
        }

        internal static TSection ReadFile<TSection>(string name)
            where TSection : SectionData
        {
            using (var stream = ReadFile(name)) {
                var section = Section<SectionData>.ReadData(stream) as TSection;
                if (section == null) {
                    throw new ArgumentException(string.Format("File \"{0}\" is not a {1}!", name, typeof(TSection).Name), "name");
                }

                return section;
            }
        }
    }
}
