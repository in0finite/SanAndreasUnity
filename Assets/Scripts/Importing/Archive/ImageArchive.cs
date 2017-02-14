using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Archive
{
    public class ImageArchive : IArchive, IDisposable
    {
        private struct ImageArchiveEntry
        {
            public readonly UInt32 Offset;
            public readonly UInt32 Size;
            public UInt32 End { get { return Offset + Size; } }
            public readonly String Name;

            public ImageArchiveEntry(BinaryReader reader)
            {
                Offset = reader.ReadUInt32() << 11;
                var sizeSecond = reader.ReadUInt16();
                var sizeFirst = reader.ReadUInt16();
                Size = (UInt32) ((sizeFirst != 0) ? sizeFirst << 11 : sizeSecond << 11);
                Name = reader.ReadString(24);
            }
        }

        public static ImageArchive Load(String filePath)
        {
			UnityEngine.Debug.Log ("Loading image archive: " + filePath);
            return new ImageArchive(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        private readonly Stream _stream;
        private readonly List<ImageArchiveEntry> _entries;
        private readonly Dictionary<String, ImageArchiveEntry> _fileDict;
        private readonly Dictionary<String, List<String>> _extDict;

        public readonly String Version;
        public readonly UInt32 EntryCount;

        private ImageArchive(Stream stream)
        {
            _stream = stream;

            var reader = new BinaryReader(stream);
            Version = new String(reader.ReadChars(4));
            EntryCount = reader.ReadUInt32();

            _entries = new List<ImageArchiveEntry>();
            _fileDict = new Dictionary<string, ImageArchiveEntry>(StringComparer.InvariantCultureIgnoreCase);
            _extDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            for (var i = 0; i < EntryCount; ++i) {
                var entry = new ImageArchiveEntry(reader);
                _entries.Add(entry);
                _fileDict.Add(entry.Name, entry);

                var ext = Path.GetExtension(entry.Name);
                if (ext == null) continue;

                if (!_extDict.ContainsKey(ext)) {
                    _extDict.Add(ext, new List<string>());
                }

                _extDict[ext].Add(entry.Name);
            }
        }

        public IEnumerable<string> GetFileNamesWithExtension(string ext)
        {
            return _extDict.ContainsKey(ext) ? _extDict[ext] : Enumerable.Empty<string>();
        }

        public bool ContainsFile(string name)
        {
            return _fileDict.ContainsKey(name);
        }

        public string GetFileName(long offset)
        {
            if (_fileDict.Count == 0) return null;
            if (offset < _entries.First().Offset) return null;
            if (offset >= _entries.Last().End) return null;
            return _entries.FirstOrDefault(x => x.Offset <= offset && x.End > offset).Name;
        }

        public Stream ReadFile(String name)
        {
            var entry = _fileDict[name];
            var stream = new FrameStream(_stream, entry.Offset, entry.Size);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
