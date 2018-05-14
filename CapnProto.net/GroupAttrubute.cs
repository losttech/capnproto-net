using System;

namespace CapnProto
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    [System.ComponentModel.ImmutableObject(true)]
    public sealed class GroupAttribute : Attribute
    {
    }
}
