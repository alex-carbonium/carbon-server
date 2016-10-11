using System;

namespace Carbon.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class LengthAttribute : Attribute
    {
        public int Length { get; private set; }
        public bool MaxPossible { get; set; }

        public LengthAttribute()
        {
        }
        public LengthAttribute(int length)
        {
            Length = length;
        }
    }
}