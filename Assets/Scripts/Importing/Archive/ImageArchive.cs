using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Archive
{
    public class ImageArchive : IDisposable
    {
        private struct ImageArchiveEntry
        {
            public readonly UInt32 Offset;
            public readonly UInt32 Size;
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
            return new ImageArchive(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        private readonly Stream _stream;
        private readonly Dictionary<String, ImageArchiveEntry> _fileDict;
        private readonly Dictionary<String, List<String>> _extDict;

        public readonly String Version;
        public readonly UInt32 Length;

        private ImageArchive(Stream stream)
        {
            _stream = stream;

            var reader = new BinaryReader(stream);
            Version = new String(reader.ReadChars(4));
            Length = reader.ReadUInt32();

            _fileDict = new Dictionary<string, ImageArchiveEntry>(StringComparer.InvariantCultureIgnoreCase);
            _extDict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

            for (var i = 0; i < Length; ++i) {
                var entry = new ImageArchiveEntry(reader);
                _fileDict.Add(entry.Name, entry);

                var ext = Path.GetExtension(entry.Name);
                if (ext == null) continue;

                if (!_extDict.ContainsKey(ext)) {
                    _extDict.Add(ext, new List<string>());
                }

                _extDict[ext].Add(entry.Name);
            }
        }

        public IEnumerable<String> GetFileNamesWithExtension(String ext)
        {
            return _extDict.ContainsKey(ext) ? _extDict[ext] : Enumerable.Empty<String>();
        }

        public bool ContainsFile(String name)
        {
            return _fileDict.ContainsKey(name);
        }

        public FrameStream ReadFile(String name)
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
