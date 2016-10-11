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
        Task<List<ProjectFolder>> GetDashboard();        

        Task ChangeProjectName(string projectId, string newName);
        Task ChangeCompanyName(string newName);
        Task<string> GetCompanyName();

        Task<ExternalAcl> ShareProject(string userId, string projectId, int permission);

        Task RegisterKnownEmail(string email);
        Task RegisterExternalAcl(ExternalAcl acl);

        Task UpdateExternalCompanyName(string companyId, string newName);
        Task<string> ResolveExternalCompanyId(string companyName);

        Task<string> GetProjectMirrorCode(string projectId);
        Task<string> SetProjectMirrorCode(string projectId, string code);

        Task<List<CompanyFileInfo>> GetFiles();
        Task<CompanyFileInfo> GetFile(string name);
        Task RegisterFile(CompanyFileInfo file);
        Task DeleteFile(string name);
    }
}
