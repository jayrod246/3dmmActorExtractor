using Socrates.Collections;
using Socrates.ValueTypes;

namespace Socrates.Chunks
{
    /// <summary>
    /// Represents a Chunk inside a 3DMM file. This is the base class for all chunks, and is abstract.
    /// </summary>
    public abstract class Chunk
    {
        #region Properties
        internal protected ChunkCollection Container { get; internal set; }

        public string String { get; set; }

        public ReferenceCollection References { get; set; }

        public byte Mode { get; set; }

        public Reference[] ReferencedBy => Container?.RetrieveInfo(this).ReferencedBy.ToArray() ?? new Reference[0];
        #endregion

        #region Constructors
        protected Chunk()
        {
            References = new ReferenceCollection(this);
        }
        #endregion

        #region Abstract
        public abstract Quad Quad { get; }
        public abstract uint Id { get; }
        public abstract byte[] SectionData { get; set; }
        #endregion

        #region Overrides
        public sealed override int GetHashCode()
        {
            return GetHashCodeInternal(Quad, Id);
        }

        internal static int GetHashCodeInternal(Quad quad, uint id)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + quad.GetHashCode();
                hash = hash * 31 + id.GetHashCode();
                return hash;
            }
        }
        #endregion
    }
}
