using System;

namespace Carbon.Business.Domain
{
    public interface IAclEntryContainer
    {
        AclEntry Entry { get; set; }
    }

    public class AclEntry : IEquatable<AclEntry>
    {
        public string Sid { get; set; }
        public ResourceType ResourceType { get; set; }
        public string ResourceId { get; set; }

        public static AclEntry Create(string userId, ResourceType resourceType, string resourceId)
        {
            return new AclEntry {Sid = userId, ResourceType = resourceType, ResourceId = resourceId};
        }

        public bool Equals(AclEntry other)
        {
            if (other == null)
            {
                return false;
            }
            return Sid == other.Sid
                   && ResourceType == other.ResourceType
                   && ResourceId == other.ResourceId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AclEntry);
        }

        public override int GetHashCode()
        {
            var hashCode = (int)ResourceType;
            hashCode = (hashCode * 397) ^ ResourceId.GetHashCode();
            hashCode = (hashCode * 397) ^ Sid.GetHashCode();
            return hashCode;
        }
    }
}
