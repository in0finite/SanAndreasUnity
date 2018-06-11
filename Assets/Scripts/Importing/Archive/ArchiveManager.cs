using SanAndreasUnity.Importing.RenderWareStream;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Importing.Archive
{
    public interface IArchive
    {
        IEnumerable<string> GetFileNamesWithExtension(string ext);

        bool ContainsFile(string name);

        Stream ReadFile(string name);
    }

    public static class ArchiveManager
    {
        public static string GameDir
        {
            get { return Config.Get<Dictionary<string, string>>(Config.const_dev_profiles).Where(x => x.Key == SystemInfo.deviceUniqueIdentifier).FirstOrDefault().Value; }
        }

        public static string ModelsDir { get { return Path.Combine(GameDir, "models"); } }
        public static string DataDir { get { return Path.Combine(GameDir, "data"); } }

        public static string GetPath(params string[] relative)
        {
            return relative.Aggregate(GameDir, Path.Combine).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        private static readonly List<IArchive> _sLoadedArchives = new List<IArchive>();

        public static LooseArchive LoadLooseArchive(string dirPath)
        {
            var arch = LooseArchive.Load(dirPath);
            _sLoadedArchives.Add(arch);
            return arch;
        }

        public static ImageArchive LoadImageArchive(string filePath)
        {
            var arch = ImageArchive.Load(filePath);
            _sLoadedArchives.Add(arch);
            return arch;
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

        public static TSection ReadFile<TSection>(string name)
            where TSection : SectionData
        {
            using (var stream = ReadFile(name))
            {
                var section = Section<SectionData>.ReadData(stream) as TSection;
                if (section == null)
                {
                    throw new ArgumentException(string.Format("File \"{0}\" is not a {1}!", name, typeof(TSection).Name), "name");
                }

                return section;
            }
        }
    }
}