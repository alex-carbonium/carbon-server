using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;

namespace Carbon.Business.Sync.Handlers
{
    [PrimitiveHandler(PrimitiveType.ProjectNameChange)]
    public class ProjectNameChangeHandler : PrimitiveHandler<ProjectNameChangePrimitive>
    {
        public override void Apply(ProjectNameChangePrimitive primitive, ProjectModel projectModel, IDependencyContainer scope)
        {
            var operation = scope.Resolve<OperationContext>();
            var actorFabric = scope.Resolve<IActorFabric>();
            var companyActor = actorFabric.GetProxy<ICompanyActor>(projectModel.CompanyId);
            companyActor.ChangeProjectName(operation.UserId, projectModel.Id, primitive.NewName);
        }
    }
}
