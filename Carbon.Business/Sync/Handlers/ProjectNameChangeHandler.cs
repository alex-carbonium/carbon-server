using Carbon.Business.Domain;
using Carbon.Business.Services;

namespace Carbon.Business.Sync.Handlers
{
    [PrimitiveHandler(PrimitiveType.ProjectNameChange)]
    public class ProjectNameChangeHandler : PrimitiveHandler<ProjectNameChangePrimitive>
    {        
        public override void Apply(ProjectNameChangePrimitive primitive, ProjectModel projectModel, PrimitiveContext context)
        {
            var actorFabric = context.Scope.Resolve<IActorFabric>();
            var companyActor = actorFabric.GetProxy<ICompanyActor>(projectModel.CompanyId);
            companyActor.ChangeProjectName(projectModel.Id, primitive.NewName);
        }
    }
}
