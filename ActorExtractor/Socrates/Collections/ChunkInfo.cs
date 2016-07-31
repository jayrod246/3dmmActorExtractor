using Socrates.Chunks;
using Socrates.ValueTypes;
using System.Collections.Generic;

namespace Socrates.Collections
{
    public class ChunkInfo
    {
        public List<Reference> ReferencedBy { get; }

        public Chunk Target { get; set; }

        public bool Exists => Target != null;

        public ChunkInfo(Chunk target = null)
        {
            Target = target;
            ReferencedBy = new List<Reference>();
        }
    }
}