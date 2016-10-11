using System;

namespace Carbon.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class UniqueAttribute : Attribute
    {
        public UniqueAttribute()
        {            
        }

        public UniqueAttribute(string name)
        {
            Name = name;    
        }

        public string Name { get; private set; }
    }
}
