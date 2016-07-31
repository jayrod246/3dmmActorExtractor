using Socrates.Chunks;
using Socrates.ValueTypes;
using System;
using System.IO;
using System.Linq;

namespace Socrates.IO
{
    /// <summary>
    /// The base class for all 3DMM files. This class is abstract.
    /// </summary>
    public abstract class File
    {
        #region Properties
        public string FilePath { get; set; }

        public string FileName
        {
            get { return Path.GetFileNameWithoutExtension(FilePath); }
            set { SetFileName(value); }
        }

        public string FileExt
        {
            get { return Path.GetExtension(FilePath); }
            set { SetFileExtension(value); }
        }

        private void SetFileExtension(string ext)
        {
            FilePath = Path.Combine(Path.GetDirectoryName(FilePath),
                string.Concat(Path.GetFileNameWithoutExtension(FilePath), ext));
        }

        private void SetFileName(string fname)
        {
            fname = fname.TrimStart('.');
            FilePath = Path.Combine(Path.GetDirectoryName(FilePath),
                string.Concat(Path.GetFileNameWithoutExtension(fname), FileExt));
        }

        protected internal BinaryWriter Writer { get; protected set; }
        protected internal BinaryReader Reader { get; protected set; }
        #endregion

        #region Abstract
        protected abstract void Read();
        protected abstract void Write();
        #endregion

        #region Constructors
        protected File(string path)
        {
            if (!string.IsNullOrEmpty(path))
                Load(path);
        }
        #endregion

        public static File Open(string path)
        {
            switch (Path.GetFileName(path))
            {
                default:
                    return new CnFile(path);
            }
        }

        public void Load(string path)
        {
            FilePath = path;
            Load();
        }

        public void Load()
        {
            try
            {
                using (Reader = new BinaryReader(System.IO.File.OpenRead(FilePath)))
                {
                    Read();
                }
            }
            finally
            {
                Reader = null;
            }
        }

        public void SaveAs(string path)
        {
            FilePath = path;
            Save();
        }

        public void Save()
        {
            try
            {
                using (Writer = new BinaryWriter(System.IO.File.OpenWrite(FilePath)))
                {
                    Write();
                }
            }
            finally
            {
                Writer = null;
            }
        }

        public void Read(Stream stream)
        {
            try
            {
                Reader = new BinaryReader(stream);
                Read();
                // No dispose call. Disposing would dispose of the user provided stream as well.
            }
            finally
            {
                Reader = null;
            }
        }

        public void CopyTo(Stream stream)
        {
            try
            {
                Writer = new BinaryWriter(stream);
                Write();
                Writer.Flush(); // Flush, as disposing would dispose of the user provided stream as well.
            }
            finally
            {
                Writer = null;
            }
        }

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
        #endregion

        #region Reader Methods
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
        #endregion
    }
}
