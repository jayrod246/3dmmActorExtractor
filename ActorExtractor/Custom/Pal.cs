using System;
using Socrates.Chunks;
using System.IO;
using System.Windows.Media;
using Socrates.Attributes;

namespace ActorExtractor.Custom
{
    [SectionName("PAL ")]
    public class Pal : VirtualChunk
    {
        public Color[] Colors { get; }

        public Pal(uint id) : base(id)
        {
            Colors = new Color[256];
        }

        protected override void Read()
        {
            if (StreamLength < 22)
                throw new InvalidDataException("PAL file is too short.");
            StreamPosition += 22;
            var count = ReadUInt16();
            for (int i = 0; i < count; i++)
            {
                var color = new Color();
                color.R = ReadByte();
                color.G = ReadByte();
                color.B = ReadByte();
                color.A = 255;
                ReadByte();
                Colors[i] = color;
            }
            for (int i = count; i < 256; i++)
            {
                Colors[i] = new Color();
            }
        }

        protected override void Write()
        {
            throw new NotImplementedException();
        }
    }
}
