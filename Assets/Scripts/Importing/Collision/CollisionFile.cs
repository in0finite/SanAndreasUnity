using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Importing.Collision
{
    public class CollisionFile
    {
        private class CollisionFileInfo
        {
            private CollisionFile _value;

            public readonly String FileName;
            public readonly Version Version;
            public readonly long Offset;
            public readonly long Length;
            public readonly string Name;
            public readonly int ModelId;

            public CollisionFile Value { get { return _value ?? (_value = Load()); } }

            public CollisionFileInfo(BinaryReader reader, String fileName, Version version)
            {
                FileName = fileName;
                Version = version;
                Length = reader.ReadUInt32();
                Offset = reader.BaseStream.Position;
                Name = reader.ReadString(22);
                ModelId = reader.ReadInt16();
            }

            private CollisionFile Load()
            {
                throw new NotImplementedException();
            }
        }

        private static readonly Dictionary<String, CollisionFileInfo> _sModelNameDict
            = new Dictionary<string, CollisionFileInfo>(StringComparer.InvariantCultureIgnoreCase);
        private static readonly Dictionary<int, CollisionFileInfo> _sModelIdDict
            = new Dictionary<int, CollisionFileInfo>();

        public static void Load(string fileName)
        {
            using (var stream = ResourceManager.ReadFile(fileName)) {
                var versBuffer = new byte[4];
                var reader = new BinaryReader(stream);
                while (stream.Read(versBuffer, 0, 4) == 4) {
                    var version = (Version) Enum.Parse(typeof (Version), Encoding.ASCII.GetString(versBuffer));
                    var modelInfo = new CollisionFileInfo(reader, fileName, version);
                    _sModelNameDict.Add(modelInfo.Name, modelInfo);
                    _sModelIdDict.Add(modelInfo.ModelId, modelInfo);
                }
            }
        }

        public static CollisionFile FromName(String name)
        {
            return _sModelNameDict.ContainsKey(name) ? _sModelNameDict[name].Value : null;
        }

        public static CollisionFile FromId(int id)
        {
            return _sModelIdDict.ContainsKey(id) ? _sModelIdDict[id].Value : null;
        }
    }
}
