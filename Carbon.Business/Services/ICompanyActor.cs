using System.Collections.Generic;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Microsoft.ServiceFabric.Actors;

namespace Carbon.Business.Services
{
    public interface ICompanyActor : IActor
    {
        Task<Project> CreateProject(string userId, string folderId);
        Task<int> GetProjectPermission(string userId, string projectId);
        Task<List<ProjectFolder>> GetDashboard(string userId);

        Task ChangeProjectName(string userId, string projectId, string newName);
        Task ChangeCompanyName(string newName);
        Task<string> GetCompanyName();

        Task<ExternalAcl> ShareProject(string userId, string projectId, int permission);

        Task RegisterKnownEmail(string userId, string email);
        Task RegisterExternalAcl(ExternalAcl acl);

        Task UpdateExternalCompanyName(string companyId, string newName);
        Task<string> ResolveExternalCompanyId(string companyName);

        Task<string> GetProjectMirrorCode(string userId, string projectId);
        Task<string> SetProjectMirrorCode(string userId, string projectId, string code);

        Task<List<CompanyFileInfo>> GetFiles(string userId);
        Task<CompanyFileInfo> GetFile(string userId, string name);
        Task RegisterFile(string userId, CompanyFileInfo file);
        Task DeleteFile(string userId, string name);
    }
}
