using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.Exceptions;
using Carbon.Business.Services;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Carbon.Business.Domain
{    
    public class CompanyActor : ICompanyActor
    {
        private static class Keys
        {
            public const string Company = "company";
            public const string Counter = "counter";
        }        

        private enum Countable
        {
            Project,
            Folder
        }

        public IActorStateManager StateManager { get; }
        public IActorFabric ActorFabric { get; }
        public string CompanyId { get; }

        public CompanyActor(string companyId, IActorFabric actorFabric, IActorStateManager stateManager)
        {
            StateManager = stateManager;
            ActorFabric = actorFabric;
            CompanyId = companyId;            
        }

        public async Task Activate()
        {            
            var company = new Company
            {                
                RootFolder = new ProjectFolder { Id = "my" }
            };

            await Task.WhenAll(
                StateManager.TryAddStateAsync(Keys.Company, company),
                StateManager.TryAddStateAsync(Keys.Counter, new Dictionary<Countable, int>
                {
                    {Countable.Project, 0},
                    {Countable.Folder, 0}
                })
            );
        }

        public async Task<Project> CreateProject(string userId, string folderId)
        {
            var company = await GetCompany();
            var counter = await StateManager.GetStateAsync<Dictionary<Countable, int>>(Keys.Counter);

            var folder = company.RootFolder; //find folder here 

            List<Acl> folderAcls = null;
            if (userId != CompanyId)
            {                                
                folderAcls = company.Acls
                    .Where(x => x.Entry.ResourceType == ResourceType.Folder && x.Entry.ResourceId == folder.Id)
                    .ToList();

                var userAcl = folderAcls.SingleOrDefault(x => x.Entry.Sid == userId);
                if (userAcl == null || !userAcl.Allows(Permission.CreateProject))
                {
                    return null;
                }
            }                        

            var projectId = ++counter[Countable.Project];
            var project = new Project
            {               
                Id = projectId.ToString(),                
                Name = "My awesome app"
            };

            if (folderAcls != null)
            {
                foreach (var acl in folderAcls)
                {
                    company.AddOrReplaceAcl(Acl.CreateForProject(acl.Entry.Sid, project.Id, acl.Permission));
                }
            }            
            folder.Projects.Add(project);

            await StateManager.SetStateAsync(Keys.Company, company);
            await StateManager.SetStateAsync(Keys.Counter, counter);

            return project;
        }                     

        public async Task<int> GetProjectPermission(string userId, string projectId)
        {
            if (userId == CompanyId)
            {
                return (int)Permission.Owner;
            }

            var company = await StateManager.GetStateAsync<Company>(Keys.Company);            
            var acl = company.Acls.SingleOrDefault(x => x.Entry.ResourceType == ResourceType.Project
                                                        && x.Entry.Sid == userId
                                                        && x.Entry.ResourceId == projectId);

            return acl?.Permission ?? (int)Permission.None;
        }

        public async Task<List<ProjectFolder>> GetDashboard(string userId)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Read);

            var sharedFolder = new ProjectFolder
            {
                Id = "shared"
            };

            foreach (var acl in company.ExternalAcls.Where(x => x.Entry.ResourceType == ResourceType.Project))
            {
                var project = new ExternalProject
                {
                    Id = acl.Entry.ResourceId,
                    Name = acl.ResourceName,
                    CompanyId = acl.Entry.Sid,
                    CompanyName = acl.CompanyName
                };
                sharedFolder.Projects.Add(project);
            }            

            return new List<ProjectFolder> {company.RootFolder, sharedFolder};
        }

        public async Task<ExternalAcl> ShareProject(string userId, string projectId, int permission)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            if (project == null)
            {
                return null;
            }

            var acl = Acl.CreateForProject(userId, projectId, permission);
            company.AddOrReplaceAcl(acl);

            var externalAcl = await SendExternalAcl(userId, projectId, company.Name, project.Name);
            await SaveCompany(company);
            return externalAcl;
        }

        private async Task<ExternalAcl> SendExternalAcl(string userId, string projectId, string companyName, string projectName)
        {
            var reverseEntry = AclEntry.Create(CompanyId, ResourceType.Project, projectId);
            var externalAcl = ExternalAcl.Create(reverseEntry, companyName, projectName);
            var externalActor = ActorFabric.GetProxy<ICompanyActor>(userId);
            await externalActor.RegisterExternalAcl(externalAcl);
            return externalAcl;
        }

        public async Task ChangeProjectName(string userId, string projectId, string newName)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);                        
            if (project != null)
            {
                ValidateProjectPermission(company, project, userId, Permission.Write);

                var externalTasks = new List<Task>();
                var externalAcls = company.Acls.Where(x => x.Entry.ResourceType == ResourceType.Project && x.Entry.ResourceId == projectId);
                foreach (var acl in externalAcls)
                {
                    externalTasks.Add(SendExternalAcl(acl.Entry.Sid, projectId, company.Name, newName));
                }
                await Task.WhenAll(externalTasks);

                project.Name = newName;
                await SaveCompany(company);
            }
        }

        public async Task<string> GetCompanyName()
        {
            var company = await GetCompany();
            return company.Name;
        }

        public async Task ChangeCompanyName(string newName)
        {
            var company = await GetCompany();
            if (company.Name != newName)
            {
                var externalTasks = new List<Task>();
                foreach (var userId in company.Acls.Select(x => x.Entry.Sid).Distinct())
                {
                    var actor = ActorFabric.GetProxy<ICompanyActor>(userId);
                    externalTasks.Add(actor.UpdateExternalCompanyName(CompanyId, newName));
                }
                await Task.WhenAll(externalTasks);

                company.Name = newName;
                await SaveCompany(company);
            }
        }

        public async Task RegisterKnownEmail(string userId, string email)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Write);
            var user = company.Users.SingleOrDefault(x => x.Email == email);
            if (user == null)
            {
                user = new User { Email = email };
                company.AddOrReplaceUser(user);
                await SaveCompany(company);
            }
        }

        public async Task RegisterExternalAcl(ExternalAcl acl)
        {
            var company = await GetCompany();
            company.AddOrReplaceExternalAcl(acl);
            await SaveCompany(company);
        }

        public async Task UpdateExternalCompanyName(string companyId, string newName)
        {
            var company = await GetCompany();
            foreach (var acl in company.ExternalAcls.Where(x => x.Entry.Sid == companyId).ToList())
            {
                company.AddOrReplaceExternalAcl(acl.WithCompanyName(newName));
            }
            await SaveCompany(company);
        }

        public async Task<string> ResolveExternalCompanyId(string companyName)
        {
            var company = await GetCompany();
            return company.ExternalAcls
                .SingleOrDefault(x => x.CompanyName == companyName)
                ?.Entry.Sid;
        }

        public async Task UpdateExternalResourceName(string companyName, ResourceType resourceType, string oldName, string newName)
        {
            var company = await GetCompany();
            foreach (var acl in company.ExternalAcls.Where(
                x => x.Entry.ResourceType == resourceType
                     && x.CompanyName == companyName
                     && x.ResourceName == oldName))
            {
                acl.ResourceName = newName;
            }
            await SaveCompany(company);
        }

        public async Task<string> GetProjectMirrorCode(string userId, string projectId)
        {
            var company = await GetCompany();            
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);
            return project.MirroringCode;
        }

        public async Task<string> SetProjectMirrorCode(string userId, string projectId, string code)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);
            var oldCode = project.MirroringCode;
            project.MirroringCode = code;
            await SaveCompany(company);

            return oldCode;
        }

        public async Task<List<CompanyFileInfo>> GetFiles(string userId)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Read);
            return company.Files.ToList();
        }

        public async Task<CompanyFileInfo> GetFile(string userId, string name)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Read);
            var file = company.Files.SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return file;
        }

        public async Task RegisterFile(string userId, CompanyFileInfo file)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Write);
            company.AddOrReplaceFile(file);
            await SaveCompany(company);
        }

        public async Task DeleteFile(string userId, string name)
        {
            var company = await GetCompany();
            ValidateCompanyPermission(company, userId, Permission.Write);
            company.RemoveFile(name);
            await SaveCompany(company);
        }

        protected async Task<Company> GetCompany()
        {
            return await StateManager.GetStateAsync<Company>(Keys.Company);
        }
        protected async Task SaveCompany(Company company)
        {
            await StateManager.SetStateAsync(Keys.Company, company);
        }

        protected IActorStateManager GetStateManager()
        {
            return StateManager;            
        }

        private void ValidateCompanyPermission(Company company, string userId, Permission requested)
        {
            if (userId == CompanyId)
            {
                return;
            }

            var acl = company.Acls.SingleOrDefault(x => x.Entry.ResourceType == ResourceType.Company
                                                     && x.Entry.Sid == userId
                                                     && x.Entry.ResourceId == CompanyId);
            if (acl?.Allows(requested) == false)
            {
                throw new InsufficientPermissionsException(requested);
            }
        }
        private void ValidateProjectPermission(Company company, Project project, string userId, Permission requested)
        {
            if (userId == CompanyId)
            {
                return;
            }

            var acl = company.Acls.SingleOrDefault(x => x.Entry.ResourceType == ResourceType.Project
                                                     && x.Entry.Sid == userId
                                                     && x.Entry.ResourceId == project.Id);
            if (acl?.Allows(requested) == false)
            {
                throw new InsufficientPermissionsException(requested);
            }
        }

        private static Project FindProject(Company company, string projectId)
        {
            //find in folders here
            return company.RootFolder.Projects.SingleOrDefault(x => x.Id == projectId);
        }
    }
}
