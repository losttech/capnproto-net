namespace CapnProto
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public struct ListPointer: IPointer
    {
        internal ListPointer(Pointer pointer) {
            this.Pointer = pointer;
        }
        public Pointer Pointer { get; }

        public ElementSize ElementType => (ElementSize)(this.Pointer.dataWordsAndPointers & 7);
    }

    public static class ListPointerExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ListPointer AsListPointer(this Pointer pointer) {

            if (!pointer.IsValid() || !pointer.IsList())
                throw new ArgumentException(message: "Expected: valid list pointer", paramName: nameof(pointer));

            return new ListPointer(pointer);
        }
    }
}
