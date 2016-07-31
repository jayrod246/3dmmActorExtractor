using Socrates.Chunks;
using System.Collections.Generic;
using System.Collections;
using System;
using Socrates.ValueTypes;

namespace Socrates.Collections
{
    public class ChunkCollection : IList<Chunk>
    {
        List<Chunk> Items { get; }

        internal protected Dictionary<int, ChunkInfo> InfoDictionary { get; private set; }

        public ChunkCollection()
        {
            Items = new List<Chunk>();
            InfoDictionary = new Dictionary<int, ChunkInfo>();
        }

        protected void InsertItem(int index, Chunk item)
        {
            ChunkInfo info = RetrieveInfo(item);
            if (info.Exists)
                throw new ArgumentException($"Collection already contains {item.Quad}-{item.Id}");
            info.Target = item;

            Items.Insert(index, item);
            item.Container = this;
            foreach (var r in item.References)
                ReferenceAdded(r, item);
        }

        public Chunk Find(Quad quad, uint id)
        {
            ChunkInfo info;
            if (!InfoDictionary.TryGetValue(Chunk.GetHashCodeInternal(quad, id), out info))
                return null;
            return info.Target;
        }

        protected void RemoveItem(int index)
        {
            if (index >= Count || index < 0)
                throw new ArgumentOutOfRangeException("index");
            foreach (var r in Items[index].References)
                ReferenceRemoved(r, Items[index]);
            Items[index].Container = null;
            ChunkInfo info = RetrieveInfo(Items[index]);
            info.Target = null;
            Items.RemoveAt(index);
        }

        protected void ClearItems()
        {
            for (int i = Count - 1; i >= 0; i--)
                RemoveItem(i);
        }

        protected void SetItem(int index, Chunk item)
        {
            var oldItem = Items[index];
            try
            {
                RemoveItem(index);
                InsertItem(index, item);
            }
            catch
            {

                if (!Items.Contains(oldItem))
                    InsertItem(index, oldItem);
                throw;
            }
        }

        private void ReferenceAdded(Reference r, Chunk carrier)
        {
            ChunkInfo info = RetrieveInfo(r);
            info.ReferencedBy.Add(new Reference(carrier.Quad, carrier.Id, r.RefId));
        }

        private void ReferenceRemoved(Reference r, Chunk carrier)
        {
            ChunkInfo info = RetrieveInfo(r);
            info.ReferencedBy.Remove(new Reference(carrier.Quad, carrier.Id, r.RefId));
        }

        internal ChunkInfo RetrieveInfo(Reference item)
        {
            return RetrieveInfo(Chunk.GetHashCodeInternal(item.Quad, item.Id));
        }

        internal ChunkInfo RetrieveInfo(Chunk item)
        {
            return RetrieveInfo(item.GetHashCode());
        }

        internal ChunkInfo RetrieveInfo(int hash)
        {
            ChunkInfo info;
            if (!InfoDictionary.TryGetValue(hash, out info))
                InfoDictionary.Add(hash, (info = new ChunkInfo()));
            return info;
        }

        #region IList Implementation
        public Chunk this[int index]
        {
            get
            {
                return ((IList<Chunk>)Items)[index];
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
                return ((IList<Chunk>)Items).Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IList<Chunk>)Items).IsReadOnly;
            }
        }

        public void Add(Chunk item)
        {
            InsertItem(Count, item);
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(Chunk item)
        {
            return ((IList<Chunk>)Items).Contains(item);
        }

        public void CopyTo(Chunk[] array, int arrayIndex)
        {
            ((IList<Chunk>)Items).CopyTo(array, arrayIndex);
        }

        public IEnumerator<Chunk> GetEnumerator()
        {
            return ((IList<Chunk>)Items).GetEnumerator();
        }

        public int IndexOf(Chunk item)
        {
            return ((IList<Chunk>)Items).IndexOf(item);
        }

        public void Insert(int index, Chunk item)
        {
            InsertItem(index, item);
        }

        public bool Remove(Chunk item)
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
            return ((IList<Chunk>)Items).GetEnumerator();
        }
        #endregion
    }
}
