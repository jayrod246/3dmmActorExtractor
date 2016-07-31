using System.IO;
using Socrates.Attributes;
using System.Linq;

namespace Socrates.Chunks
{
    [SectionName("MBMP")]
    public class Mbmp : VirtualChunk
    {
        #region Properties
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Pixels { get; set; }
        public bool UsesTransparency { get; set; }
        #endregion

        #region Constructors
        public Mbmp(uint id) : base(id)
        {
        }
        #endregion

        protected override void Write()
        {
            Write(MagicNumber);
            Write(0x0); // Junk
            Write(OffsetX);
            Write(OffsetY);
            Write(Width + OffsetX);
            Write(Height + OffsetY);

            Write(0); // FileSize

            var lineLengths = new short[Height];
            StreamPosition += Height * 2;

            byte TransparentIndex = 0x0;

            for (uint datanum = 0; datanum < Height * Width; datanum += (uint)Width)
            {
                int checkline = 0;
                if (UsesTransparency)
                {
                    uint checknum = datanum;
                    while (Pixels[checknum] == TransparentIndex && checkline < Width)
                    {
                        checknum++;
                        checkline++;
                    }
                }
                else
                    checkline = 0;

                if (checkline >= Width)
                    lineLengths[datanum / Width] = 0;
                else
                {
                    uint curdatanum = datanum;
                    uint lastentry = 0;
                    uint linesize = 0;
                    while (curdatanum < (datanum + Width))
                    {
                        uint skip = 0;
                        if (UsesTransparency)
                        {
                            while (Pixels[curdatanum] == TransparentIndex && skip < 255 && skip < Width - lastentry)
                            {
                                curdatanum++;
                                skip++;
                            }
                            if (Width - lastentry == skip)
                                break;
                        }
                        lastentry += skip;

                        uint num = 0;

                        if (UsesTransparency)
                            while (num < 255 && num < Width - lastentry && Pixels[curdatanum + num] != TransparentIndex)
                                num++;
                        else
                            while (num < 255 && num < Width - lastentry)
                                num++;

                        lastentry += num;
                        Write((byte)skip);
                        Write((byte)num);
                        foreach (var obj in Pixels.Skip((int)curdatanum).Take((int)num))
                            Write(obj);
                        curdatanum += num;
                        linesize += num + 2;

                    }
                    lineLengths[datanum / Width] = (short)linesize;
                }
            }

            StreamPosition = 28;
            foreach (var linelength in lineLengths)
                Write(linelength);
            StreamPosition = 24;
            Write((int)StreamLength);
        }

        protected override void Read()
        {
            MagicNumber = ReadMagicNumber();
            if (ReadInt32() != 0)
                throw new InvalidDataException("Invalid MBMP header.");

            OffsetX = ReadInt32();
            OffsetY = ReadInt32();
            Width = ReadInt32() - OffsetX;
            Height = ReadInt32() - OffsetY;

            Pixels = new byte[Width * Height];

            if (ReadUInt32() != StreamLength)
                throw new InvalidDataException("Filelengths don't match.");

            var lineLengths = new short[Height];
            for (int line = 0; line < Height; line++)
            {
                lineLengths[line] = ReadInt16();
            }

            for (int line = 0; line < Height; line++)
            {
                int xpos = 0;
                uint linepos = 0;
                while (linepos < lineLengths[line])
                {
                    int skip = ReadByte();

                    if (skip > 0)
                        UsesTransparency = true;
                    int linesize = ReadByte();
                    if (linesize == -1)
                    {

                    }
                    xpos += skip;
                    linepos += 2;
                    var linedata = ReadBytes(linesize);
                    for (int xx = 0; xx < linesize; xx++)
                    {
                        Pixels[line * Width + xpos] = linedata[xx];
                        xpos++;
                    }
                    linepos += (uint)linesize;
                }
            }
        }
    }
}
