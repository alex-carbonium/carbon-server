using System.Collections.Generic;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Fabric.Common;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Carbon.Framework.Logging;
using Carbon.Fabric.Common.Logging;

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
            using (var operation = OperationContext.GetForUser(Id.ToString()))
            {
                CommonEventSource.Current.Info("Actor activated: " + Id.ToString(), source: "FabricCompanyActor");
            }

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

        public Task<List<ProjectFolder>> GetDashboard(string userId)
        {
            return Impl.GetDashboard(userId);
        }

        public Task<CompanyInfo> GetCompanyInfo()
        {
            return Impl.GetCompanyInfo();
        }

        public Task UpdateOwnerInfo(UserInfo info)
        {
            return Impl.UpdateOwnerInfo(info);
        }

        public Task<ExternalAcl> ShareProject(string userId, string projectId, int permission)
        {
            return Impl.ShareProject(userId, projectId, permission);
        }

        public Task ChangeProjectSettings(string userId, string projectId, ProjectSettings settings)
        {
            return Impl.ChangeProjectSettings(userId, projectId, settings);
        }

        public Task UpdateCompanyInfo(CompanyInfo info)
        {
            return Impl.UpdateCompanyInfo(info);
        }

        public Task RegisterKnownEmail(string userId, string email)
        {
            return Impl.RegisterKnownEmail(userId, email);
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

        public Task<string> GetProjectMirrorCode(string userId, string projectId)
        {
            return Impl.GetProjectMirrorCode(userId, projectId);
        }

        public Task<string> SetProjectMirrorCode(string userId, string projectId, string code)
        {
            return Impl.SetProjectMirrorCode(userId, projectId, code);
        }

        public Task<List<CompanyFileInfo>> GetFiles(string userId)
        {
            return Impl.GetFiles(userId);
        }

        public Task<CompanyFileInfo> GetFile(string userId, string name)
        {
            return Impl.GetFile(userId, name);
        }

        public Task RegisterFile(string userId, CompanyFileInfo file)
        {
            return Impl.RegisterFile(userId, file);
        }

        public Task DeleteFile(string userId, string name)
        {
            return Impl.DeleteFile(userId, name);
        }

        public Task UpdateExternalResourceName(string companyName, ResourceType resourceType, string oldName, string newName)
        {
            return Impl.UpdateExternalResourceName(companyName, resourceType, oldName, newName);
        }
    }
}
