using System.Collections.Generic;

namespace Carbon.Business.Domain
{
    public class UniqueAclEntryComparer : IEqualityComparer<IAclEntryContainer>
    {
        public static readonly UniqueAclEntryComparer Default = new UniqueAclEntryComparer();

        public bool Equals(IAclEntryContainer x, IAclEntryContainer y)
        {
            if (x == null || y == null || x.Entry == null || y.Entry == null)
            {
                return false;
            }
            return x.Entry.Equals(y.Entry);
        }

        public int GetHashCode(IAclEntryContainer obj)
        {
            if (obj?.Entry == null)
            {
                return 0;
            }
            return obj.Entry.GetHashCode();
        }
    }
}