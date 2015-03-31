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
                Offset = reader.BaseStream.Position;
                Length = reader.ReadUInt32() + 4;
                Name = reader.ReadString(22);
                ModelId = reader.ReadInt16();
            }

            private CollisionFile Load()
            {
                return new CollisionFile(this);
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

        private CollisionFileInfo _info;

        public string Name { get { return _info.Name; } }
        public int ModelId { get { return _info.ModelId; } }

        public readonly Bounds Bounds;
        public readonly Flags Flags;

        public readonly Sphere[] Spheres;
        public readonly Box[] Boxes;
        public readonly Vertex[] Vertices;
        public readonly FaceGroup[] FaceGroups;
        public readonly Face[] Faces;

        private CollisionFile(CollisionFileInfo info)
        {
            _info = info;

            var version = info.Version;

            using (var stream = ResourceManager.ReadFile(info.FileName))
            using (var reader = new BinaryReader(stream)) {
                stream.Seek(info.Offset + 20, SeekOrigin.Begin);

                Bounds = new Bounds(reader, version);

                int spheres, boxes, verts, faces, faceGroups;
                long spheresOffset, boxesOffset, vertsOffset,
                    facesOffset, faceGroupsOffset;

                switch (version) {
                    case Version.COLL: {
                        spheres = reader.ReadInt32();
                        spheresOffset = stream.Position;
                        stream.Seek(spheres * Sphere.Size, SeekOrigin.Current);

                        reader.ReadInt32();

                        boxes = reader.ReadInt32();
                        boxesOffset = stream.Position;
                        stream.Seek(boxes * Box.Size, SeekOrigin.Current);

                        verts = reader.ReadInt32();
                        vertsOffset = stream.Position;
                        stream.Seek(verts * Vertex.SizeV1, SeekOrigin.Current);

                        faces = reader.ReadInt32();
                        facesOffset = stream.Position;

                        faceGroups = 0;
                        faceGroupsOffset = 0;

                        break;
                    }
                    default: {
                        spheres = reader.ReadUInt16();
                        boxes = reader.ReadUInt16();
                        faces = reader.ReadUInt16();
                        reader.ReadInt16();

                        Flags = (Flags) reader.ReadInt32();

                        spheresOffset = reader.ReadUInt32() + info.Offset;
                        boxesOffset = reader.ReadUInt32() + info.Offset;
                        reader.ReadUInt32();
                        vertsOffset = reader.ReadUInt32() + info.Offset;
                        facesOffset = reader.ReadUInt32() + info.Offset;

                        stream.Seek(facesOffset - 4, SeekOrigin.Current);
                        faceGroups = reader.ReadInt32();
                        faceGroupsOffset = facesOffset - 4 - FaceGroup.Size * faceGroups;

                        verts = (int) (faceGroupsOffset - vertsOffset) / Vertex.Size;

                        break;
                    }
                }

                Spheres = new Sphere[spheres];
                Boxes = new Box[boxes];
                Vertices = new Vertex[verts];
                Faces = new Face[faces];
                FaceGroups = new FaceGroup[faceGroups];

                if (spheres > 0) {
                    stream.Seek(spheresOffset, SeekOrigin.Begin);
                    for (var i = 0; i < spheres; ++i) {
                        Spheres[i] = new Sphere(reader, version);
                    }
                }

                if (boxes > 0) {
                    stream.Seek(boxesOffset, SeekOrigin.Begin);
                    for (var i = 0; i < boxes; ++i) {
                        Boxes[i] = new Box(reader, version);
                    }
                }

                if (verts > 0) {
                    stream.Seek(vertsOffset, SeekOrigin.Begin);
                    for (var i = 0; i < verts; ++i) {
                        Vertices[i] = new Vertex(reader, version);
                    }
                }

                if (faces > 0) {
                    stream.Seek(facesOffset, SeekOrigin.Begin);
                    for (var i = 0; i < faces; ++i) {
                        Faces[i] = new Face(reader, version);
                    }
                }

                if (faceGroups > 0) {
                    stream.Seek(faceGroupsOffset, SeekOrigin.Begin);
                    for (var i = 0; i < faceGroups; ++i) {
                        FaceGroups[i] = new FaceGroup(reader);
                    }
                }
            }
        }
    }
}
