using System.Collections.Generic;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Fabric.Common;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Carbon.CompanyActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class FabricCompanyActor : Actor, ICompanyActor
    {
        public FabricCompanyActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        public Business.Domain.CompanyActor Impl { get; set; }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            Impl = new Business.Domain.CompanyActor(Id.GetStringId(), ActorFabric.Default, StateManager);
            await Impl.Activate();
        }

        public Task<Project> CreateProject(string userId, string folderId)
        {
            return Impl.CreateProject(userId, folderId);
        }                     

        public Task<int> GetProjectPermission(string userId, string projectId)
        {
            return Impl.GetProjectPermission(userId, projectId);
        }

        public Task<List<ProjectFolder>> GetDashboard()
        {
            return Impl.GetDashboard();
        }

        public Task<string> GetCompanyName()
        {
            return Impl.GetCompanyName();
        }

        public Task<ExternalAcl> ShareProject(string userId, string projectId, int permission)
        {
            return Impl.ShareProject(userId, projectId, permission);
        }

        public Task ChangeProjectName(string projectId, string newName)
        {
            return Impl.ChangeProjectName(projectId, newName);
        }        

        public Task ChangeCompanyName(string newName)
        {
            return Impl.ChangeCompanyName(newName);
        }

        public Task RegisterKnownEmail(string email)
        {
            return Impl.RegisterKnownEmail(email);
        }

        public Task RegisterExternalAcl(ExternalAcl acl)
        {
            return Impl.RegisterExternalAcl(acl);
        }

        public Task UpdateExternalCompanyName(string companyId, string newName)
        {
            return Impl.UpdateExternalCompanyName(companyId, newName);
        }

        public Task<string> ResolveExternalCompanyId(string companyName)
        {
            return Impl.ResolveExternalCompanyId(companyName);
        }

        public Task<string> GetProjectMirrorCode(string projectId)
        {
            return Impl.GetProjectMirrorCode(projectId);
        }

        public Task<string> SetProjectMirrorCode(string projectId, string code)
        {
            return Impl.SetProjectMirrorCode(projectId, code);
        }

        public Task<List<CompanyFileInfo>> GetFiles()
        {
            return Impl.GetFiles();
        }

        public Task<CompanyFileInfo> GetFile(string name)
        {
            return Impl.GetFile(name);
        }

        public Task RegisterFile(CompanyFileInfo file)
        {
            return Impl.RegisterFile(file);
        }

        public Task DeleteFile(string name)
        {
            return Impl.DeleteFile(name);
        }

        public Task UpdateExternalResourceName(string companyName, ResourceType resourceType, string oldName, string newName)
        {
            return Impl.UpdateExternalResourceName(companyName, resourceType, oldName, newName);
        }
    }
}
