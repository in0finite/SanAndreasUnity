using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanAndreasUnity.Importing.RenderWareStream
{
    [SectionType(3)]
    public class Extension : SectionData
    {
        public readonly SectionData[] Sections;

        public Extension(SectionHeader header, Stream stream)
            : base(header, stream)
        {
            var sections = new List<SectionData>();
            while (stream.Position < stream.Length)
            {
                sections.Add(ReadSection<SectionData>(header.GetParent()));
            }

            Sections = sections.ToArray();
        }

        public void ForEach<TSection>(Action<TSection> action)
            where TSection : SectionData
        {
            foreach (var section in Sections.OfType<TSection>())
            {
                action(section);
            }
        }

        public TSection FirstOrDefault<TSection>()
            where TSection : SectionData
        {
            return Sections.OfType<TSection>().FirstOrDefault();
        }
    }
}