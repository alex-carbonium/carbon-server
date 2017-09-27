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

        Task ChangeProjectSettings(string userId, string projectId, ProjectSettings settings);

        Task<CompanyInfo> GetCompanyInfo();
        Task UpdateCompanyInfo(CompanyInfo info);
        Task UpdateOwnerInfo(UserInfo info);

        Task<ExternalAcl> ShareProject(string toUserId, string projectId, int permission);

        Task RegisterKnownEmail(string userId, string email);
        Task RegisterExternalAcl(ExternalAcl acl);

        Task UpdateExternalCompanyName(string companyId, string newName);
        Task<string> ResolveExternalCompanyId(string companyName);

        Task<string> GetProjectMirrorCode(string userId, string projectId);
        Task<string> SetProjectMirrorCode(string userId, string projectId, string code);

        Task<IEnumerable<ProjectShareCode>> GetProjectShareCodes(string userId, string projectId);
        Task AddProjectShareCode(string userId, string projectId, ProjectShareCode code);
        Task RemoveProjectShareCode(string userId, string projectId, string codeId);
        Task RemoveProjectShareCodes(string userId, string projectId);

        Task<List<CompanyFileInfo>> GetFiles(string userId);
        Task<CompanyFileInfo> GetFile(string userId, string name);
        Task RegisterFile(string userId, CompanyFileInfo file);
        Task DeleteFile(string userId, string name);
    }
}
