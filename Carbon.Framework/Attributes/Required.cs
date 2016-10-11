using System;

namespace Carbon.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class Required : Attribute
    {
    }
}
