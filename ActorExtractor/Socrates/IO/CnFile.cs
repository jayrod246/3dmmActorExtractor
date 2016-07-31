using Socrates.Chunks;
using Socrates.Collections;
using Socrates.Compression;
using Socrates.ValueTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Socrates.IO
{
    /// <summary>
    /// Generic Collections file. Contains 3DMM header information and chunk data.
    /// </summary>
    public class CnFile : File, IList<Chunk>
    {
        #region Fields
        private ChunkCollection collection = new ChunkCollection();
        #endregion

        #region Properties
        public Quad Identifier { get; set; }
        public Quad Signature { get; set; }
        public ushort Unk1 { get; set; }
        public ushort Unk2 { get; set; }
        public MagicNumber MagicNumber { get; set; }
        public uint FileLength { get; private set; }
        public uint IndexOffset { get; private set; }
        public uint IndexLength { get; private set; }
        #endregion

        #region Constructors
        public CnFile() : this("")
        { }

        public CnFile(string path) : base(path)
        {
            Identifier = new Quad("CHN2");
            Signature = new Quad("CHMP");
            Unk1 = 5;
            Unk2 = 4;
        }

        public CnFile(IEnumerable<Chunk> collection) : this("")
        {
            foreach (var chunk in collection)
                this.collection.Add(chunk);
        }
        #endregion

        protected override void Read()
        {
            var start = StreamPosition;

            // Read 3DMM header (128 bytes)
            if (StreamLength - start < 128)
                throw new InvalidDataException("3DMM header couldn't be read; source is not at least 128 bytes in length");
            Identifier = (Quad)ReadChars(4);
            Signature = (Quad)ReadChars(4, true);
            Unk1 = ReadUInt16();
            Unk2 = ReadUInt16();
            MagicNumber = ReadMagicNumber();
            FileLength = ReadUInt32();
            IndexOffset = ReadUInt32();
            IndexLength = ReadUInt32();
            if (ReadUInt32() != FileLength)
                throw new InvalidDataException("Invalid 3DMM header");
            StreamPosition += 96;

            // Read section data into memory.
            int sectionlength = Convert.ToInt32(IndexOffset - (StreamPosition - start));
            var sectdata = ReadBytes(sectionlength);

            // Check if we are at the right offset.
            if (StreamPosition - start != FileLength - IndexLength)
                throw new InvalidDataException("Invalid 3DMM file; index length is not correct");

            // Read Index header (20 bytes)
            if (StreamLength - (StreamPosition - start) < 20)
                throw new InvalidDataException("Index header couldn't be read; source is not at least 20 bytes in length");
            MagicNumber = ReadMagicNumber();
            var EntryCount = ReadUInt32();
            var EntryDataLength = ReadUInt32();
            if (ReadUInt32() != 0xFFFFFFFF || ReadUInt32() != 20)
                throw new InvalidDataException("Invalid Index header");

            // Read entry data into memory.
            var entrydata = ReadBytes((int)EntryDataLength);

            // Create entries and read offsets/lengths.
            var items = new List<CnIndexEntry>();
            for (int n = 0; n < EntryCount; n++)
                items.Add(new CnIndexEntry(ReadUInt32(), ReadUInt32()));

            using (var entryBR = new BinaryReader(new MemoryStream(entrydata), Encoding.GetEncoding(1252)))
            using (var sectBR = new BinaryReader(new MemoryStream(sectdata), Encoding.GetEncoding(1252)))
            {
                // ---------------------------------------
                // ------- Type determining phase. -------
                // ---------------------------------------
                foreach (var item in items)
                {
                    // Move to offset position.
                    entryBR.BaseStream.Position = item.Offset;

                    // Read 4 chars, reverse them, make string. The result is a quad.
                    // Creates an instance of Chunk using the quad as the type.
                    item.Chunk = new SimpleChunk(new string(entryBR.ReadChars(4).Reverse().ToArray()), entryBR.ReadUInt32());

                    // Read in the data for this entry.
                    item.SectionOffset = entryBR.ReadUInt32();
                    item.Chunk.Mode = entryBR.ReadByte();

                    // Mode
                    //  0 - Uncompressed
                    //  2 - Uncompressed Main
                    //  4 - Compressed
                    //  6 - Compressed Main
                    //  18 - ???

                    if (item.Chunk.Mode == 4 || item.Chunk.Mode == 6)
                        item.IsCompressed = true;

                    // Only the first 3 bytes of Section length is stored, so we concatenate a zero at the end and interpret that as UInt32.
                    item.SectionLength = BitConverter.ToUInt32(entryBR.ReadBytes(3).Concat(new byte[] { 0 }).ToArray(), 0);

                    item.ReferenceCount = entryBR.ReadUInt16();
                    item.TimesReferenced = entryBR.ReadUInt16();

                    // References
                    for (int r = 0; r < item.ReferenceCount; r++)
                        item.Chunk.References.Add(
                            new Reference(
                                new string(entryBR.ReadChars(4).Reverse().ToArray()),
                                entryBR.ReadUInt32(),
                                entryBR.ReadUInt32()
                                ));

                    // Checks if a string is present.
                    if (item.Length - (item.ReferenceCount * 12) - 20 > 0)
                    {
                        var strType = entryBR.ReadUInt16();
                        ushort len;
                        switch (strType)
                        {
                            case 0x0303: // Windows-1252 string.
                                item.Chunk.String = entryBR.ReadString();
                                break;
                            case 0x0505: // Unicode string.
                                len = entryBR.ReadUInt16();
                                item.Chunk.String = Encoding.Unicode.GetString(entryBR.ReadBytes(len * 2));
                                break;
                            default: // Do nothing.
                                break;
                        }
                    }

                    // Move position to the section offset in file.
                    sectBR.BaseStream.Position = item.SectionOffset - 128;

                    // Use a local copy of section, in case of errors.
                    byte[] sectData = sectBR.ReadBytes((int)item.SectionLength);

                    item.Chunk.SectionData = sectData;
                }

                collection.Clear();
                var offsetSort = items.OrderBy(e => e.Offset).ToList();
                foreach (var item in offsetSort)
                    collection.Add(item.Chunk);
            }

            // --------------------------------------
            // -------- Error Checking Phase --------
            // --------------------------------------
            foreach (var item in items)
            {
                if (item.Chunk.ReferencedBy.Length != item.TimesReferenced)
                    throw new InvalidDataException("ReferencedBy.Count is incorrect");
                else if (item.Chunk.References.Count != item.ReferenceCount)
                    throw new InvalidDataException("References.Count is incorrect");
            }
        }

        protected override void Write()
        {
            using (var sectBW = new BinaryWriter(new MemoryStream(), Encoding.GetEncoding(1252)))
            using (var entryBW = new BinaryWriter(new MemoryStream(), Encoding.GetEncoding(1252)))
            using (var dirBW = new BinaryWriter(new MemoryStream(), Encoding.GetEncoding(1252)))
            {
                // Encapsulate each chunk with IndexEntry.
                var items = collection.Select(c => new CnIndexEntry() { Chunk = c }).ToList();

                // Sections and entries are sorted by this.
                var sorted = items.OrderBy(e => e.Chunk.Quad).ThenBy(e => e.Chunk.Id);

                foreach (var entry in sorted)
                {
                    entry.SectionOffset = (uint)sectBW.BaseStream.Position + 128;
                    byte[] data = entry.Chunk.SectionData;
                    entry.IsCompressed = Compressor.IsCompressed(data);
                    sectBW.Write(data);
                    entry.SectionLength = (uint)data.Length;
                }

                foreach (var item in items)
                {
                    item.Offset = (uint)entryBW.BaseStream.Position;
                    entryBW.Write(item.Chunk.Quad.ToCharArray().Reverse().ToArray());
                    entryBW.Write(item.Chunk.Id);
                    entryBW.Write(item.SectionOffset);
                    item.TimesReferenced = (ushort)item.Chunk.ReferencedBy.Length;

                    // Auto generate the mode.
                    byte mode = item.IsCompressed ? (byte)4 : (byte)0;
                    if (item.TimesReferenced == 0)
                        mode += 2;
                    if (item.Chunk.Mode != mode && (new List<byte> { 0, 2, 4, 6 }.Exists(b => b == item.Chunk.Mode)))
                        entryBW.Write(mode);
                    else
                        entryBW.Write(item.Chunk.Mode);
                    entryBW.Write(item.SectionLength);
                    entryBW.BaseStream.Position -= 1;
                    entryBW.Write((ushort)item.Chunk.References.Count);
                    entryBW.Write(item.TimesReferenced);

                    // Try to sort the reference collection before writing.
                    //item.Chunk.References.Sort();

                    foreach (var r in item.Chunk.References)
                    {
                        entryBW.Write(r.Quad.ToCharArray().Reverse().ToArray());
                        entryBW.Write(r.Id);
                        entryBW.Write(r.RefId);
                    }

                    item.Length = 20 + (12 * (uint)item.Chunk.References.Count);

                    // Strings
                    int extraBytes = 0;
                    if (!string.IsNullOrEmpty(item.Chunk.String))
                    {
                        var buffer1252 = Encoding.GetEncoding(1252).GetBytes(item.Chunk.String);
                        if (Encoding.GetEncoding(1252).GetString(buffer1252).SequenceEqual(item.Chunk.String))
                        {
                            char strlen = (char)item.Chunk.String.Length;
                            entryBW.Write((ushort)0x0303);
                            entryBW.Write(strlen);
                            entryBW.Write(buffer1252);
                            if ((strlen) % 4 == 0)
                                extraBytes = 0;
                            else
                                extraBytes = 4 - ((strlen) % 4);
                            entryBW.BaseStream.Position += extraBytes;
                            entryBW.Write((char)0);
                            item.Length += (uint)3 + strlen + 1;
                        }
                        else
                        {
                            var bufferUnicode = Encoding.Unicode.GetBytes(item.Chunk.String);
                            var result = Encoding.Unicode.GetString(bufferUnicode);
                            if (!result.SequenceEqual(item.Chunk.String))
                            {
                                // TODO: Show a warning message for incompatible strings.
                            }
                            entryBW.Write((ushort)0x0505);
                            var strlen = (ushort)result.Length;
                            entryBW.Write(strlen);
                            entryBW.Write(bufferUnicode);
                            if ((strlen) % 2 == 0)
                                extraBytes = 4;
                            else
                                extraBytes = 2;
                            entryBW.Write(new byte[extraBytes]);
                            item.Length += (uint)(4 + (strlen * 2) + 2);
                        }
                    }
                }

                foreach (var entry in sorted)
                {
                    dirBW.Write(entry.Offset);
                    dirBW.Write(entry.Length);
                }

                var EntryCount = (uint)collection.Count;
                var EntryDataLength = (uint)entryBW.BaseStream.Length;
                IndexLength = Convert.ToUInt32(20 + EntryDataLength + dirBW.BaseStream.Length);
                FileLength = Convert.ToUInt32(128 + sectBW.BaseStream.Length + IndexLength);
                IndexOffset = Convert.ToUInt32(128 + sectBW.BaseStream.Length);

                // BEGIN File Header
                Write(Identifier.ToCharArray());
                Write(Signature.ToCharArray(), true);
                Write(Unk1);
                Write(Unk2);
                Write(MagicNumber);
                Write(FileLength);
                Write(IndexOffset);
                Write(IndexLength);
                Write(FileLength);
                for (int i = 0; i < 24; i++)
                    Write(0);
                // END File Header

                Write((sectBW.BaseStream as MemoryStream).ToArray());

                // BEGIN Index Header
                Write(MagicNumber);
                Write(EntryCount);
                Write(EntryDataLength);
                Write(0xFFFFFFFF);
                Write(20);
                // END Index Header

                Write((entryBW.BaseStream as MemoryStream).ToArray());
                Write((dirBW.BaseStream as MemoryStream).ToArray());
            }
        }

        public static bool TryPeekQuads(string path, out Quad[] quads)
        {
            var tmp = new CnFile();
            try
            {
                using (tmp.Reader = new BinaryReader(System.IO.File.OpenRead(path)))
                {
                    quads = new Quad[0];
                    if (tmp.StreamLength < 128)
                        return false;
                    tmp.Identifier = (Quad)tmp.ReadChars(4);
                    tmp.Signature = (Quad)tmp.ReadChars(4, true);
                    tmp.Unk1 = tmp.ReadUInt16();
                    tmp.Unk2 = tmp.ReadUInt16();
                    tmp.MagicNumber = tmp.ReadMagicNumber();
                    tmp.FileLength = tmp.ReadUInt32();
                    tmp.IndexOffset = tmp.ReadUInt32();
                    tmp.IndexLength = tmp.ReadUInt32();
                    if (tmp.ReadUInt32() != tmp.FileLength)
                        return false;
                    tmp.StreamPosition = tmp.IndexOffset;

                    // Read Index header (20 bytes)
                    if (tmp.StreamLength - tmp.StreamPosition < 20)
                        return false;
                    tmp.MagicNumber = tmp.ReadMagicNumber();
                    var EntryCount = tmp.ReadUInt32();
                    var EntryDataLength = tmp.ReadUInt32();
                    if (tmp.ReadUInt32() != 0xFFFFFFFF || tmp.ReadUInt32() != 20)
                        throw new InvalidDataException("Invalid Index header");

                    // Read entry data into memory.
                    var entrydata = tmp.ReadBytes((int)EntryDataLength);

                    // Create entries and read offsets/lengths.
                    var items = new List<CnIndexEntry>();
                    for (int n = 0; n < EntryCount; n++)
                        items.Add(new CnIndexEntry(tmp.ReadUInt32(), tmp.ReadUInt32()));

                    var collection = new List<Quad>();
                    using (var entryBR = new BinaryReader(new MemoryStream(entrydata), Encoding.GetEncoding(1252)))
                    {
                        // ---------------------------------------
                        // ------- Type determining phase. -------
                        // ---------------------------------------
                        foreach (var item in items)
                        {
                            // Move to offset position.
                            entryBR.BaseStream.Position = item.Offset;

                            // Read 4 chars, reverse them, make string. The result is a quad.
                            // Creates an instance of Chunk using the quad as the type.
                            collection.Add(new Quad(entryBR.ReadChars(4).Reverse().ToArray()));
                        }

                        quads = collection.Distinct().ToArray();
                        return true;
                    }
                }
            }
            finally
            {
                tmp.Reader = null;
            }
        }

        #region Nested Classes
        private class CnIndexEntry
        {
            public uint Offset { get; set; }
            public uint Length { get; set; }
            public Chunk Chunk { get; set; }
            public uint SectionOffset { get; set; }
            public uint SectionLength { get; set; }
            public ushort TimesReferenced { get; set; }
            public ushort ReferenceCount { get; set; }
            public bool IsCompressed { get; set; }

            public CnIndexEntry()
            {
            }

            public CnIndexEntry(uint offset, uint length)
            {
                Offset = offset;
                Length = length;
            }
        }
        #endregion

        #region IList Implementation
        public Chunk this[int index]
        {
            get
            {
                return ((IList<Chunk>)collection)[index];
            }

            set
            {
                ((IList<Chunk>)collection)[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return ((IList<Chunk>)collection).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Chunk>)collection).IsReadOnly;
            }
        }

        public void Add(Chunk item)
        {
            ((IList<Chunk>)collection).Add(item);
        }

        public void Clear()
        {
            ((IList<Chunk>)collection).Clear();
        }

        public bool Contains(Chunk item)
        {
            return ((IList<Chunk>)collection).Contains(item);
        }

        public void CopyTo(Chunk[] array, int arrayIndex)
        {
            ((IList<Chunk>)collection).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Chunk> GetEnumerator()
        {
            return ((IList<Chunk>)collection).GetEnumerator();
        }

        public int IndexOf(Chunk item)
        {
            return ((IList<Chunk>)collection).IndexOf(item);
        }

        public void Insert(int index, Chunk item)
        {
            ((IList<Chunk>)collection).Insert(index, item);
        }

        public bool Remove(Chunk item)
        {
            return ((IList<Chunk>)collection).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<Chunk>)collection).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Chunk>)collection).GetEnumerator();
        }
        #endregion
    }
}
