using System.Threading.Tasks;
using Carbon.Business.Domain;

namespace Carbon.Business.Services
{
    public class PermissionService
    {
        private readonly IActorFabric _actorFabric;

        public PermissionService(IActorFabric actorFabric)
        {
            _actorFabric = actorFabric;
        }

        public async Task<Permission> GetProjectPermission(string userId, string companyId, string projectId)
        {
            if (userId == companyId)
            {
                return Permission.Owner;
            }

            //TODO: add caching
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            return (Permission)await actor.GetProjectPermission(userId, projectId);
        }
    }
}
