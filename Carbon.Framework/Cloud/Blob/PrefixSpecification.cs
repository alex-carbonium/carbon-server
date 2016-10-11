using System.Linq;
using Carbon.Framework.Specifications;

namespace Carbon.Framework.Cloud.Blob
{
    public class PrefixSpecification<T> : ISpecification<T>
        where T : BlobDomainObject
    {
        public PrefixSpecification(string prefix)
        {
            Prefix = prefix;
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            return query;
        }

        public bool Cacheable { get; set; }

        public string Prefix { get; set; }
    }
}
