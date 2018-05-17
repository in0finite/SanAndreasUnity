using SanAndreasUnity.Importing.Collision;
using System.IO;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(0x0253F2FA)]
    public class CollisionModel : SectionData
    {
        public readonly CollisionFile Collision;

        public CollisionModel(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            Collision = CollisionFile.Load(stream);
        }
    }
}