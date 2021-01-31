using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Importing.Archive
{
    public class LooseArchive : IArchive
    {
        private struct LooseArchiveEntry
        {
            public readonly string FilePath;
            public readonly string Name;

            public LooseArchiveEntry(string filePath)
            {
                FilePath = filePath;
                Name = Path.GetFileName(filePath);
            }
        }

        private static readonly HashSet<string> _sValidExtensions
            = new HashSet<string> {
                ".txd",
                ".gxt",
                ".col",
                ".dff",
                ".fxp",
                ".ifp",
                ".ide",
                ".ipl",
                ".zon",
                ".img",
                ".dat",
                ".cfg",
            };

        private readonly Dictionary<String, LooseArchiveEntry> _fileDict;
        private readonly Dictionary<String, List<String>> _extDict;

        public int NumLoadedEntries => _fileDict.Count;


        public static LooseArchive Load(string dirPath)
        {
            return new LooseArchive(dirPath);
        }

        private LooseArchive(string dirPath)
        {
            Debug.Log("Loading loose archive: " + dirPath);

            _fileDict = new Dictionary<string, LooseArchiveEntry>(StringComparer.InvariantCultureIgnoreCase);
            _extDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file);

                ext = ext.ToLower();

                if (!_sValidExtensions.Contains(ext)) continue;

                var entry = new LooseArchiveEntry(file);

                if (_fileDict.ContainsKey(entry.Name))
                {
                    Debug.LogWarningFormat("Already loaded {0}", entry.Name);
                    continue;
                }

                //Debug.Log ("Adding loose archive entry: " + entry.FilePath);

                _fileDict.Add(entry.Name, entry);

                if (ext == null) continue;

                if (!_extDict.ContainsKey(ext))
                {
                    _extDict.Add(ext, new List<string>());
                }

                _extDict[ext].Add(entry.Name);
            }
        }

        public IEnumerable<string> GetAllFiles()
        {
            return _fileDict.Keys;
        }

        public IEnumerable<string> GetFileNamesWithExtension(string ext)
        {
            return _extDict.ContainsKey(ext) ? _extDict[ext] : Enumerable.Empty<string>();
        }

        public bool ContainsFile(string name)
        {
            return _fileDict.ContainsKey(name);
        }

        public System.IO.Stream ReadFile(string name)
        {
            return File.OpenRead(_fileDict[name].FilePath);
        }

        public bool GetFilePath(string fileName, ref string filePath)
        {
            if (_fileDict.TryGetValue(fileName, out LooseArchiveEntry entry))
            {
                filePath = entry.FilePath;
                return true;
            }
            return false;
        }

    }
}