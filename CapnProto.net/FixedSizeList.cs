
using System;
using System.Collections;
using System.Collections.Generic;
namespace CapnProto
{
    using static ListPointer;

    public struct FixedSizeList<T> : IList<T>, IList, IPointer
    {
        public static explicit operator FixedSizeList<T>(Pointer pointer) { return new FixedSizeList<T>(pointer); }
        public static implicit operator Pointer(FixedSizeList<T> obj) { return obj.pointer; }
        private readonly Pointer pointer;
        private FixedSizeList(Pointer pointer) { this.pointer = pointer; }
        public override bool Equals(object obj)
        {
            return obj is FixedSizeList<T> && ((FixedSizeList<T>)obj).pointer == this.pointer;
        }
        public override int GetHashCode() { return this.pointer.GetHashCode(); }
        public override string ToString() { return pointer.ToString(); }

        public T this[int index]
        {
            get { return TypeAccessor<T>.Instance.GetElement(pointer, index); }
            set { TypeAccessor<T>.Instance.SetElement(pointer, index, value); }
        }
        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (T)value; }
        }

        void IList<T>.Insert(int index, T value) { throw new NotSupportedException(); }
        void IList.Insert(int index, object value) { throw new NotSupportedException(); }
        void IList<T>.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList.Remove(object value) { throw new NotSupportedException(); }
        void IList.RemoveAt(int index) { throw new NotSupportedException(); }
        public int Count() { return pointer.Count(); }

        void ICollection<T>.Add(T item) { throw new NotSupportedException(); }
        int IList.Add(object value) { throw new NotSupportedException(); }
        void ICollection<T>.Clear() { throw new NotSupportedException(); }
        void IList.Clear() { throw new NotSupportedException(); }

        public bool Contains(T value)
        {

            var pointer = this.pointer.Dereference();
            int count = pointer.Count();
            if (count != 0)
            {
                var comparer = EqualityComparer<T>.Default;
                var accessor = TypeAccessor<T>.Instance;
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(accessor.GetElement(pointer, i), value)) return true;
                }
            }
            return false;
        }
        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }
        public int IndexOf(T value)
        {
            var pointer = this.pointer.Dereference();
            int count = pointer.Count();
            if (count != 0)
            {
                var comparer = EqualityComparer<T>.Default;
                var accessor = TypeAccessor<T>.Instance;
                for (int i = 0; i < count; i++)
                {
                    if (comparer.Equals(accessor.GetElement(pointer, i), value)) return i;
                }
            }
            return -1;
        }
        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            var pointer = this.pointer.Dereference();
            int count = pointer.Count();
            if (count != 0)
            {
                var accessor = TypeAccessor<T>.Instance;
                for (int i = 0; i < count; i++)
                {
                    array[arrayIndex++] = accessor.GetElement(pointer, i);
                }
            }
        }
        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            var pointer = this.pointer.Dereference();
            int count = pointer.Count();
            if (count != 0)
            {
                var accessor = TypeAccessor<T>.Instance;
                for (int i = 0; i < count; i++)
                {
                    array.SetValue(accessor.GetElement(pointer, i), arrayIndex++);
                }
            }
        }

        int ICollection<T>.Count { get { return Count(); } }
        int ICollection.Count { get { return Count(); } }

        bool ICollection.IsSynchronized { get { return false; } }
        object ICollection.SyncRoot { get { return null; } }

        bool ICollection<T>.IsReadOnly { get { return TypeAccessor<T>.Instance.IsStruct; } }
        bool IList.IsReadOnly { get { return TypeAccessor<T>.Instance.IsStruct; } }
        bool IList.IsFixedSize { get { return true; } }

        bool ICollection<T>.Remove(T item) { throw new NotSupportedException(); }

        public IEnumerator<T> GetEnumerator()
        {
            var pointer = this.pointer.Dereference();
            int count = pointer.Count();
            if (count != 0)
            {
                var accessor = TypeAccessor<T>.Instance;
                for (int i = 0; i < count; i++)
                    yield return accessor.GetElement(pointer, i);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static FixedSizeList<T> Create(Pointer pointer, int count)
        {
            return TypeAccessor<T>.Instance.CreateList(pointer, count);
        }

        public static FixedSizeList<T> Create(Pointer pointer, IList<T> items)
        {
            var accessor = TypeAccessor<T>.Instance;
            if (accessor.IsStruct) throw new InvalidOperationException("Lists of structs cannot be created retrospectively, since the list *is* the data");

            if (items == null) return default(FixedSizeList<T>);
            if (items.Count == 0) return FixedSizeList<T>.Create(pointer, 0);

            FixedSizeList<T> list = accessor.CreateList(pointer, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                list[i] = items[i];
            }
            return list;
        }
        Pointer IPointer.Pointer { get { return pointer; } }
    }

    public static class FixedSizeList {
        public static System.Collections.IList AsList(this Pointer pointer) {
            var listPointer = pointer.AsListPointer();
            int elementCount = listPointer.ElementType == ElementSize.InlineComposite
                ? throw new NotImplementedException()
                : (int)(listPointer.Pointer.dataWordsAndPointers >> 3);
            switch (listPointer.ElementType) {
            case ElementSize.ZeroByte: return FixedSizeList<DBNull>.Create(pointer, elementCount);
            case ElementSize.EightBytesPointer: return FixedSizeList<Pointer>.Create(pointer, elementCount);
            case ElementSize.OneBit: return FixedSizeList<bool>.Create(pointer, elementCount);
            case ElementSize.OneByte: return FixedSizeList<byte>.Create(pointer, elementCount);
            case ElementSize.TwoBytes: return FixedSizeList<UInt16>.Create(pointer, elementCount);
            case ElementSize.FourBytes: return FixedSizeList<UInt32>.Create(pointer, elementCount);
            case ElementSize.EightBytesNonPointer: return FixedSizeList<UInt64>.Create(pointer, elementCount);
            }

            throw new NotImplementedException();
        }
    }
}
