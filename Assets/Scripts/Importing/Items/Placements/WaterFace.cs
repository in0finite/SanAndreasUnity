using System;

namespace SanAndreasUnity.Importing.Items.Placements
{
    [Flags]
    public enum WaterFlags
    {
        None = 0,
        Visible = 1,
        Shallow = 2
    }

    public class WaterFace : Placement
    {
        public class Vertex
        {
            public readonly UnityEngine.Vector3 Position;
            public readonly UnityEngine.Vector2 CurrentSpeed;
            public readonly float WaveHeight;

            public Vertex(WaterFace face, int offset)
            {
                Position = new UnityEngine.Vector3(
                    face.GetSingle(offset + 0),
                    face.GetSingle(offset + 2),
                    face.GetSingle(offset + 1));

                CurrentSpeed = new UnityEngine.Vector2(
                    face.GetSingle(offset + 3),
                    face.GetSingle(offset + 4));

                WaveHeight = face.GetSingle(offset + 6);
            }
        }

        public readonly Vertex[] Vertices;
        public readonly WaterFlags Flags;

        public WaterFace(string line)
            : base(line, false)
        {
            var vertCount = (Parts - 1) / 7;
            Vertices = new Vertex[vertCount];

            for (var i = 0; i < vertCount; ++i)
            {
                Vertices[i] = new Vertex(this, i * 7);
            }

            Flags = (WaterFlags)GetInt(Parts - 1);
        }
    }
}