using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync.Handlers
{
    [PrimitiveHandler(PrimitiveType.ProjectSettingsChange)]
    public class ProjectSettingsChangeHandler : PrimitiveHandler<ProjectSettingsChangePrimitive>
    {
        public override void Apply(ProjectSettingsChangePrimitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
            var operation = scope.Resolve<OperationContext>();
            var actorFabric = scope.Resolve<IActorFabric>();
            var companyActor = actorFabric.GetProxy<ICompanyActor>(projectModel.CompanyId);
            companyActor.ChangeProjectSettings(operation.UserId, projectModel.Id, new ProjectSettings
            {
                Name = primitive.Name,
                Avatar = primitive.Avatar
            });
        }
    }
}
