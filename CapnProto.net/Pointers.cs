
namespace CapnProto
{
    using System.Runtime.CompilerServices;

    public interface IPointer
    {
        Pointer Pointer { get; }
    }
    public static class Pointers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid<T>(this T pointer) where T : struct, IPointer
        {
            return pointer.Pointer.IsValid;
        }
    }
}
