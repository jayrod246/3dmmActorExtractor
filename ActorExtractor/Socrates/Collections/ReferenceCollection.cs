using Socrates.Chunks;
using System.Collections.Generic;
using System.Collections;
using System;
using Socrates.ValueTypes;
using System.Linq;

namespace Socrates.Collections
{
    public sealed class ReferenceCollection : IList<Reference>
    {
        private Chunk Instance;

        private ChunkCollection Container => Instance.Container;

        List<Reference> Items { get; }

        internal ReferenceCollection(Chunk chunk)
        {
            Instance = chunk;
            Items = new List<Reference>();
        }

        public void Sort()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Chunk> Dereference()
        {
            if (Container == null)
                return Enumerable.Empty<Chunk>();
            return Items.Select(r => Container.Find(r.Quad, r.Id)).OfType<Chunk>();
        }

        public void Add(Chunk source)
        {
            Add(source, 0);
        }

        public void Add(Chunk source, uint refId)
        {
            Add(new Reference(source.Quad, source.Id, refId));
        }

        private void InsertItem(int index, Reference item)
        {
            int hash = Chunk.GetHashCodeInternal(item.Quad, item.Id);
            ChunkInfo info = null;
            if (Container?.InfoDictionary.TryGetValue(hash, out info) == true)
                info.ReferencedBy.Add(new Reference(Instance.Quad, Instance.Id, item.RefId));
            else if (Container != null)
            {
                Container.InfoDictionary.Add(hash, (info = new ChunkInfo()));
                info.ReferencedBy.Add(new Reference(Instance.Quad, Instance.Id, item.RefId));
            }

            Items.Insert(index, item);
        }

        private void RemoveItem(int index)
        {
            if (index >= Count || index < 0)
                throw new ArgumentOutOfRangeException("index");

            Reference r = Items[index];
            ChunkInfo info = null;
            if (Container?.InfoDictionary.TryGetValue(Chunk.GetHashCodeInternal(r.Quad, r.Id), out info) == true)
                info.ReferencedBy.Remove(new Reference(Instance.Quad, Instance.Id, r.RefId));

            Items.RemoveAt(index);
        }

        private void ClearItems()
        {
            for (int i = Count - 1; i >= 0; i--)
                RemoveItem(i);
        }

        private void SetItem(int index, Reference item)
        {
            RemoveItem(index);
            InsertItem(index, item);
        }

        #region IList Implementation
        public Reference this[int index]
        {
            get
            {
                return ((IList<Reference>)Items)[index];
            }

            set
            {
                SetItem(index, value);
            }
        }

        public int Count
        {
            get
            {
                return ((IList<Reference>)Items).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Reference>)Items).IsReadOnly;
            }
        }

        public void Add(Reference item)
        {
            InsertItem(Count, item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(Reference item)
        {
            return ((IList<Reference>)Items).Contains(item);
        }

        public void CopyTo(Reference[] array, int arrayIndex)
        {
            ((IList<Reference>)Items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Reference> GetEnumerator()
        {
            return ((IList<Reference>)Items).GetEnumerator();
        }

        public int IndexOf(Reference item)
        {
            return ((IList<Reference>)Items).IndexOf(item);
        }

        public void Insert(int index, Reference item)
        {
            InsertItem(index, item);
        }

        public bool Remove(Reference item)
        {
            if (!Contains(item))
                return false;
            RemoveItem(IndexOf(item));
            return true;
        }

        public void RemoveAt(int index)
        {
            RemoveItem(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<Reference>)Items).GetEnumerator();
        }
        #endregion
    }
}
