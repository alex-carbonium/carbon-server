using System;

namespace Carbon.Business.Sync
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class PrimitiveHandlerAttribute : Attribute
    {
        public PrimitiveHandlerAttribute(PrimitiveType type)
        {
            Type = type;
        }

        public PrimitiveType Type { get; }
    }
}
