using SanAndreasUnity.Importing.Archive;
using SanAndreasUnity.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

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
                Length = reader.ReadUInt32() + 4;
                Name = reader.ReadString(22);
                ModelId = reader.ReadInt16();
                Offset = reader.BaseStream.Position - 28;

                reader.BaseStream.Seek(Offset + Length, SeekOrigin.Begin);
            }

            private CollisionFile Load()
            {
                using (var stream = ArchiveManager.ReadFile(FileName))
                {
                    return new CollisionFile(this, stream);
                }
            }
        }

        private static readonly Dictionary<String, CollisionFileInfo> _sModelNameDict
            = new Dictionary<string, CollisionFileInfo>(StringComparer.InvariantCultureIgnoreCase);

        public static void Load(string fileName)
        {
            var thisFile = new List<CollisionFileInfo>();

            using (var stream = ArchiveManager.ReadFile(fileName))
            {
                var versBuffer = new byte[4];
                var reader = new BinaryReader(stream);
                while (stream.Position < stream.Length && stream.Read(versBuffer, 0, 4) == 4)
                {
                    if (versBuffer.All(x => x == 0)) break;

                    Version version;
                    var versString = Encoding.ASCII.GetString(versBuffer);
                    if (!Enum.TryParse(versString, out version))
                    {
                        if (versString.Substring(0, 3) == "OLL")
                        {
                            // Known problem (size off by one). Attempting to fix by adjusting read pointer...
                            stream.Position -= 1;
                            version = Version.COLL;
                        }
                        else
                        {
                            Debug.LogWarningFormat("Error while reading {0} at 0x{1:x} ({2}%)",
                                fileName, stream.Position - 4, (stream.Position - 4) * 100 / stream.Length);
                        }
                    }

                    var modelInfo = new CollisionFileInfo(reader, fileName, version);
                    thisFile.Add(modelInfo);

                    if (!_sModelNameDict.ContainsKey(modelInfo.Name))
                    {
                        _sModelNameDict.Add(modelInfo.Name, modelInfo);
                    }
                    else
                    {
                        _sModelNameDict[modelInfo.Name] = modelInfo;
                    }
                }
            }
        }

        public static CollisionFile Load(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var version = (Version)Enum.Parse(typeof(Version), reader.ReadString(4));
            var info = new CollisionFileInfo(reader, null, version);

            return new CollisionFile(info, stream);
        }

        public static CollisionFile FromName(String name)
        {
			UnityEngine.Profiling.Profiler.BeginSample ("CollisionFile.FromName()");
			var cf = _sModelNameDict.ContainsKey(name) ? _sModelNameDict[name].Value : null;
			UnityEngine.Profiling.Profiler.EndSample ();
			return cf;
        }

        public readonly string Name;
        public readonly int ModelId;

        public readonly Bounds Bounds;
        public readonly Flags Flags;

        public readonly Sphere[] Spheres;
        public readonly Box[] Boxes;
        public readonly Vertex[] Vertices;
        public readonly FaceGroup[] FaceGroups;
        public readonly Face[] Faces;

        private CollisionFile(CollisionFileInfo info, Stream stream)
        {
            Name = info.Name;
            ModelId = info.ModelId;

            var version = info.Version;

            var reader = new BinaryReader(stream);

            stream.Seek(info.Offset + 28, SeekOrigin.Begin);

            Bounds = new Bounds(reader, version);

            int spheres, boxes, verts, faces, faceGroups;
            long spheresOffset, boxesOffset, vertsOffset,
                facesOffset, faceGroupsOffset;

            switch (version)
            {
                case Version.COLL:
                    {
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
                default:
                    {
                        spheres = reader.ReadUInt16();
                        boxes = reader.ReadUInt16();
                        faces = reader.ReadUInt16();
                        reader.ReadInt16();

                        Flags = (Flags)reader.ReadInt32();

                        spheresOffset = reader.ReadUInt32() + info.Offset;
                        boxesOffset = reader.ReadUInt32() + info.Offset;
                        reader.ReadUInt32();
                        vertsOffset = reader.ReadUInt32() + info.Offset;
                        facesOffset = reader.ReadUInt32() + info.Offset;

                        if (faces > 0 && (Flags & Flags.HasFaceGroups) == Flags.HasFaceGroups)
                        {
                            stream.Seek(facesOffset - 4, SeekOrigin.Begin);
                            faceGroups = reader.ReadInt32();
                            faceGroupsOffset = facesOffset - 4 - FaceGroup.Size * faceGroups;
                        }
                        else
                        {
                            faceGroups = 0;
                            faceGroupsOffset = 0;
                        }

                        verts = -1;

                        break;
                    }
            }

            Spheres = new Sphere[spheres];
            Boxes = new Box[boxes];
            Faces = new Face[faces];
            FaceGroups = new FaceGroup[faceGroups];

            if (spheres > 0)
            {
                stream.Seek(spheresOffset, SeekOrigin.Begin);
                for (var i = 0; i < spheres; ++i)
                {
                    Spheres[i] = new Sphere(reader, version);
                }
            }

            if (boxes > 0)
            {
                stream.Seek(boxesOffset, SeekOrigin.Begin);
                for (var i = 0; i < boxes; ++i)
                {
                    Boxes[i] = new Box(reader, version);
                }
            }

            if (faces > 0)
            {
                stream.Seek(facesOffset, SeekOrigin.Begin);
                for (var i = 0; i < faces; ++i)
                {
                    Faces[i] = new Face(reader, version);
                }

                if (verts == -1)
                {
                    verts = Faces.Max(x => x.GetIndices().Max()) + 1;
                }

                Vertices = new Vertex[verts];

                stream.Seek(vertsOffset, SeekOrigin.Begin);
                for (var i = 0; i < verts; ++i)
                {
                    Vertices[i] = new Vertex(reader, version);
                }

                if (faceGroups > 0)
                {
                    stream.Seek(faceGroupsOffset, SeekOrigin.Begin);
                    for (var i = 0; i < faceGroups; ++i)
                    {
                        FaceGroups[i] = new FaceGroup(reader);
                    }
                }
            }
            else
            {
                Vertices = new Vertex[0];
            }
        }
    }
}