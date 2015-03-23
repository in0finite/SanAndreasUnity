using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Archive
{
    public class ImageArchive
    {
        private struct ImageArchiveEntry
        {
            public readonly UInt32 Offset;
            public readonly UInt32 Size;
            public readonly String Name;

            public ImageArchiveEntry(Stream stream)
            {
                var reader = new BinaryReader(stream);
                Offset = reader.ReadUInt32() << 11;
                var sizeSecond = reader.ReadUInt16();
                var sizeFirst = reader.ReadUInt16();
                Size = (UInt32) ((sizeFirst != 0) ? sizeFirst << 11 : sizeSecond << 11);
                Name = new String(reader.ReadChars(24)).TrimNullChars();
            }
        }

        public static ImageArchive Load(String filePath)
        {
            return new ImageArchive(new FileStream(filePath, FileMode.Open, FileAccess.Read));
        }

        private readonly Stream _stream;
        private readonly Dictionary<String, ImageArchiveEntry> _fileDict;

        public readonly String Version;
        public readonly UInt32 Length;

        public ImageArchive(Stream stream)
        {
            _stream = stream;

            var reader = new BinaryReader(stream);
            Version = new String(reader.ReadChars(4));
            Length = reader.ReadUInt32();

            _fileDict = new Dictionary<string, ImageArchiveEntry>();

            for (var i = 0; i < Length; ++i) {
                var entry = new ImageArchiveEntry(stream);
                _fileDict.Add(entry.Name, entry);
            }

            var keys = new List<string>();
            keys.AddRange(_fileDict.Keys.Where(x => x.EndsWith(".txd") && x.Contains("wat")));
        }

        public bool ContainsFile(String name)
        {
            return _fileDict.ContainsKey(name);
        }

        public FrameStream ReadFile(String name)
        {
            var entry = _fileDict[name];
            var stream = new FrameStream(_stream, entry.Offset, entry.Size);

            return stream;
        }
    }
}
