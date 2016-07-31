using Socrates.Chunks;

namespace Socrates.ValueTypes
{
    public struct Reference
    {
        public Quad Quad { get; set; }
        public uint Id { get; set; }
        public uint RefId { get; set; }

        #region Constructors
        public Reference(Quad quad, uint id, uint refId)
        {
            Quad = quad;
            Id = id;
            RefId = refId;
        }

        public Reference(string quad, uint id, uint refId) : this((Quad)quad, id, refId)
        {
        }

        public Reference(Quad quad, uint id) : this(quad, id, 0)
        {
        }

        public Reference(string quad, uint id) : this((Quad)quad, id, 0)
        {
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"{Quad} - {Id} : {RefId}";
        }
        #endregion
    }
}
