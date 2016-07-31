using Socrates.ValueTypes;

namespace Socrates.Chunks
{
    public class Th : VirtualChunk
    {
        #region Properties
        public uint SourceId { get; set; }
        public Quad SourceQuad { get; set; }
        #endregion

        #region Constructors
        public Th(uint id) : this(id, "", 0)
        {
        }

        public Th(uint id, string sourceQuad, uint sourceId) : base(id)
        {
            SourceQuad = (Quad)sourceQuad;
            SourceId = sourceId;
        }
        #endregion

        protected override void Read()
        {
            MagicNumber = ReadMagicNumber();
            SourceQuad = (Quad)ReadChars(4, true);
            SourceId = ReadUInt32();
        }

        protected override void Write()
        {
            Write(MagicNumber);
            Write(SourceQuad.ToCharArray(), true);
            Write(SourceId);
        }
    }
}
