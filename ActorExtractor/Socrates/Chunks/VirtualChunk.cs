using System;
using System.Linq;
using System.IO;
using Socrates.ValueTypes;
using Socrates.Compression;
using Socrates.Internal;

namespace Socrates.Chunks
{
    /// <summary>
    /// Represents a Chunk that can be modifed through properties. This class is abstract.
    /// </summary>
    public abstract class VirtualChunk : Chunk
    {
        #region Fields
        private Quad quad;
        private uint id;
        #endregion

        #region Properties
        public sealed override Quad Quad { get { return quad; } }
        public sealed override uint Id { get { return id; } }

        /// <summary>
        /// The MagicNumber to use. 0x03030001 or 0x05050001.
        /// </summary>
        public MagicNumber MagicNumber { get; set; }

        /// <summary>
        /// Overrides the section authored by the VirtualChunk.<para/>(Used when the data can't be parsed.)
        /// </summary>
        public byte[] SectionOverride { get; set; }

        /// <summary>
        /// Returns true when SectionOverride is not null.
        /// </summary>
        public bool HasOverride => SectionOverride != null;

        public override byte[] SectionData
        {
            get { return HasOverride ? SectionOverride : PrepareToGetData(); }
            set { PrepareToSetData(value); }
        }

        protected internal BinaryWriter Writer { get; private set; }
        protected internal BinaryReader Reader { get; private set; }
        #endregion

        #region Constructors
        protected VirtualChunk(uint id)
        {
            this.id = id;
            MagicNumber = MagicNumber.Default;
        }
        #endregion

        private void PrepareToSetData(byte[] value)
        {
            try
            {
                byte[] sectData = value;
                if (Compressor.IsCompressed(sectData))
                    sectData = Compressor.Decompress(sectData);
                using (Reader = new BinaryReader(new MemoryStream(sectData),
                    System.Text.Encoding.GetEncoding(1252)))
                {
                    Read();
                }
            }
            catch (Exception e)
            {
                // Use the original data in case of errors.
                SectionOverride = value;

                // TODO: Rethrow read exception so another part of the app can handle it.
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Reset the Reader.
                Reader = null;
            }
        }

        private byte[] PrepareToGetData()
        {
            try
            {
                using (Writer = new BinaryWriter(new MemoryStream(),
                    System.Text.Encoding.GetEncoding(1252)))
                {
                    Write();
                    return (Writer.BaseStream as MemoryStream).ToArray();
                }
            }
            finally
            {
                // Reset the Writer.
                Writer = null;
            }
        }

        public static T Create<T>(Chunk source) where T : VirtualChunk
        {
            T result = (T)Activator.CreateInstance(typeof(T), source.Id);
            result.quad = source.Quad;
            result.String = source.String;
            result.Mode = source.Mode;
            foreach (var r in source.References)
                result.References.Add(new Reference(r.Quad, r.Id, r.RefId));
            result.SectionData = source.SectionData;
            result.Container = source.Container;
            return result;
        }

        #region Abstract
        protected abstract void Read();
        protected abstract void Write();
        #endregion

        #region Stream Methods & Properties
        protected long StreamLength
        {
            get
            {
                if (Reader != null)
                    return Reader.BaseStream.Length;
                else if (Writer != null)
                    return Writer.BaseStream.Length;
                throw new MissingMemberException("StreamLength may only be used when reading or writing.");
            }

            set
            {
                if (Reader != null)
                    Reader.BaseStream.SetLength(value);
                else if (Writer != null)
                    Writer.BaseStream.SetLength(value);
                else
                    throw new MissingMemberException("StreamLength may only be used when reading or writing.");
            }
        }

        protected long StreamPosition
        {
            get
            {
                if (Reader != null)
                    return Reader.BaseStream.Position;
                else if (Writer != null)
                    return Writer.BaseStream.Position;
                throw new MissingMemberException("StreamPosition may only be used when reading or writing.");
            }
            set
            {
                if (Reader != null)
                    Reader.BaseStream.Position = value;
                else if (Writer != null)
                    Writer.BaseStream.Position = value;
                else
                    throw new MissingMemberException("StreamPosition may only be used when reading or writing.");
            }
        }
        #endregion

        #region Writer Methods
        protected void Write(string value)
        {
            Writer?.Write(value);
        }

        protected void Write(byte value)
        {
            Writer?.Write(value);
        }

        protected void Write(sbyte value)
        {
            Writer?.Write(value);
        }

        protected void Write(byte[] buffer, bool bigEndian = false)
        {
            if (bigEndian)
                buffer = buffer.Reverse().ToArray();
            Writer?.Write(buffer);
        }

        protected void Write(char[] chars, bool bigEndian = false)
        {
            if (bigEndian)
                chars = chars.Reverse().ToArray();
            Writer?.Write(chars);
        }

        protected void Write(ushort value)
        {
            Writer?.Write(value);
        }

        protected void Write(short value)
        {
            Writer?.Write(value);
        }

        protected void Write(uint value)
        {
            Writer?.Write(value);
        }

        protected void Write(int value)
        {
            Writer?.Write(value);
        }

        protected void Write(MagicNumber magicNumber)
        {
            Writer?.Write((uint)magicNumber);
        }

        protected void Write(Vector3 vector3)
        {
            Write((int)(vector3.X * GlobalValues.VECTOR_RESCALE));
            Write((int)(vector3.Y * GlobalValues.VECTOR_RESCALE));
            Write((int)(vector3.Z * GlobalValues.VECTOR_RESCALE));
        }
        #endregion

        #region Reader Methods
        protected string ReadString()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadString();
        }

        protected char ReadChar()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadChar();
        }

        protected char[] ReadChars(int count, bool bigEndian = false)
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            var chars = Reader.ReadChars(count);
            if (bigEndian)
                chars = chars.Reverse().ToArray();
            return chars;
        }

        protected byte ReadByte()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadByte();
        }

        protected sbyte ReadSByte()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadSByte();
        }

        protected byte[] ReadBytes(int count, bool bigEndian = false)
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            var bytes = Reader.ReadBytes(count);
            if (bigEndian)
                bytes = bytes.Reverse().ToArray();
            return bytes;
        }

        protected ushort ReadUInt16()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadUInt16();
        }

        protected short ReadInt16()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadInt16();
        }

        protected uint ReadUInt32()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadUInt32();
        }

        protected int ReadInt32()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadInt32();
        }

        protected ulong ReadUInt64()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadUInt64();
        }

        protected long ReadInt64()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            return Reader.ReadInt64();
        }

        protected MagicNumber ReadMagicNumber()
        {
            if (Reader == null)
                throw new MissingMemberException("The BinaryReader instance is not available.");
            var magicNumber = Reader.ReadUInt32();
            if (magicNumber != 0x03030001 && magicNumber != 0x05050001)
                throw new InvalidDataException($"MagicNumber invalid. Expected 0x03030001 or 0x05050001, got: {magicNumber.ToString("X")}");
            return (MagicNumber)magicNumber;
        }

        protected Vector3 ReadVector3()
        {
            return new Vector3(ReadInt32() / GlobalValues.VECTOR_RESCALE, ReadInt32() / GlobalValues.VECTOR_RESCALE, ReadInt32() / GlobalValues.VECTOR_RESCALE);
        }
        #endregion
    }
}
