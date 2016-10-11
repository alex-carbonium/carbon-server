using System.Linq;
using Carbon.Business.Domain;
using Carbon.Framework.Repositories;

namespace Carbon.Test.Common
{
    public class ProjectModelRepositoryStub : InMemoryRepository<ProjectModel>
    {
        public override ProjectModel FindById(dynamic key, bool lockForUpdate = false)
        {
            return Store.SingleOrDefault(x => x.Id == key.ProjectId.ToString());
        }
    }
}
