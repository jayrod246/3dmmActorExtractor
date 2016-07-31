using Socrates.ValueTypes;
using System;

namespace Socrates.Chunks
{
    public sealed class SimpleChunk : Chunk
    {
        #region Fields
        private readonly Quad quad;
        private readonly uint id;
        private byte[] sectData;
        #endregion

        #region Properties
        public override Quad Quad { get { return quad; } }
        public override uint Id { get { return id; } }
        public override byte[] SectionData
        {
            get { return sectData; }
            set { sectData = value; }
        }
        #endregion

        #region Constructors
        public SimpleChunk(string quad, uint id) : this(quad, id, new byte[0])
        { }

        public SimpleChunk(string quad, uint id, byte[] sectData)
        {
            if (quad == null)
                throw new ArgumentNullException(quad);
            this.quad = (Quad)quad;
            this.id = id;
            SectionData = sectData;
        }
        #endregion
    }
}
