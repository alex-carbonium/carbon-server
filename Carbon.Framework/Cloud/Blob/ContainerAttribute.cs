using System;

namespace Carbon.Framework.Cloud.Blob
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContainerAttribute : Attribute
    {
        public string Name { get; set; }
        public ContainerType Type { get; set; }
    }
}
