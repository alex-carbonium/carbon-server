using System;
using Carbon.Framework.Cloud.Blob;

namespace Carbon.Data.UnitOfWorkImpl
{
    public class DomainConfiguration
    {
        public static bool IsStoredInCloudBlob(Type type)
        {
            return typeof(BlobDomainObject).IsAssignableFrom(type);
        }
        public static bool IsStoredInCloud(Type type)
        {
            return IsStoredInCloudBlob(type);
        }
    }
}
