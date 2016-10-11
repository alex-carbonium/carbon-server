using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Sync;

namespace Carbon.Business.Domain
{
    public class ProjectModelChange
    {
        private List<RawPrimitive> _primitives;

        public string CompanyId { get; set; }        
        public string FolderId { get; set; }
        public string ModelId { get; set; }
        public string UserId { get; set; }

        public List<string> PrimitiveStrings { get; set; }
        public IReadOnlyList<RawPrimitive> Primitives
        {
            get
            {
                if (_primitives == null)
                {
                    _primitives = PrimitiveFactory.CreateMany<RawPrimitive>(PrimitiveStrings).ToList();
                }                
                return _primitives;                
            }
        }

        public IReadOnlyList<RawPrimitive> EnsurePrimitivesParsed()
        {
            return Primitives;
        }

        public Permission GetRequestedPermission()
        {
            if (Primitives == null)
            {
                return Permission.Read;
            }
            return Primitives.Aggregate(Permission.Read, (c, p) => c | p.Type.RequiredPermission());
        }
    }
}