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

        // use it to return to the dashboard, if there is not deleted projects
        private static readonly ProjectFolder TrashPlaceholder = new ProjectFolder { Id = "deleted" };

        public IActorStateManager StateManager { get; }
        public IActorFabric ActorFabric { get; }
        public string CompanyId { get; }

        public Project[] RecentProjects;

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
                Id = CompanyId,
                RootFolder = new ProjectFolder { Id = "my" }
            };
            company.AddOrReplaceUser(new User { Id = company.Id });

            await Task.WhenAll(
                StateManager.AddOrUpdateStateAsync(Keys.Company, company, UpgradeCompany),
                StateManager.TryAddStateAsync(Keys.Counter, new Dictionary<Countable, int>
                {
                    {Countable.Project, 0},
                    {Countable.Folder, 0}
                })
            );
        }

        private Company UpgradeCompany(string id, Company company)
        {
            company.Id = id;

            var owner = company.GetOwner();
            if (owner == null)
            {
                owner = new User
                {
                    Id = id,
                    Name = company.Name,
                    Avatar = company.Logo
                };
                company.AddOrReplaceUser(owner);
            }

            return company;
        }

        public async Task DeleteProject(string projectId)
        {
            var company = await GetCompany();
            var project = company.RootFolder.Projects.SingleOrDefault(x => x.Id == projectId);
            company.RemoveRecentRef(projectId);
            if (project != null) {
                company.RootFolder.Projects.Remove(project);
                if(company.DeletedFolder == null)
                {
                    company.DeletedFolder = new ProjectFolder { Id = "deleted" };
                }

                company.DeletedFolder.Projects.Add(project);

                await this.SaveCompany(company);
            }
            else if(company.DeletedFolder != null)
            {
                project = company.DeletedFolder.Projects.SingleOrDefault(x => x.Id == projectId);
                if (project != null)
                {
                    company.DeletedFolder.Projects.Remove(project);
                    await this.SaveCompany(company);
                }
            }
        }

        public async Task UpdatedRecentRef(string projectId)
        {
            var company = await GetCompany();
            company.AddRecentRef(projectId);
            await this.SaveCompany(company);
        }

        public async Task<Project> CreateProject(string userId, string folderId)
        {
            var company = await GetCompany();
            var folder = company.RootFolder; //find folder here

            if (!company.HasFolderPermission(userId, folder, Permission.CreateProject))
            {
                return null;
            }

            var counter = await StateManager.GetStateAsync<Dictionary<Countable, int>>(Keys.Counter);

            var projectId = ++counter[Countable.Project];
            var project = new Project
            {
                Id = projectId.ToString(),
                Name = "My carbon design"
            };

            if (userId != CompanyId)
            {
                company.PropagateFolderAcls(folder, project);
            }
            folder.Projects.Add(project);

            company.AddRecentRef(project.Id);

            await StateManager.SetStateAsync(Keys.Company, company);
            await StateManager.SetStateAsync(Keys.Counter, counter);

            return project;
        }

        public async Task<Project> DuplicateProject(string userId, string projectId)
        {
            var company = await GetCompany();
            //find folder
            var folder = company.RootFolder;

            if (!company.HasFolderPermission(userId, folder, Permission.CreateProject))
            {
                return null;
            }

            var counter = await StateManager.GetStateAsync<Dictionary<Countable, int>>(Keys.Counter);
            var project = FindProject(company, projectId);

            var newProjectId = ++counter[Countable.Project];
            var newProject = project.Clone(newProjectId.ToString());

            folder.Projects.Add(newProject);
            if (userId != CompanyId)
            {
                company.PropagateFolderAcls(folder, newProject);
            }

            await StateManager.SetStateAsync(Keys.Company, company);
            await StateManager.SetStateAsync(Keys.Counter, counter);

            return newProject;
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

            var deletedFolder = company.DeletedFolder ?? TrashPlaceholder;

            return new List<ProjectFolder> {company.RootFolder, sharedFolder, deletedFolder};
        }

        public async Task<ExternalAcl> ShareProject(string toUserId, string projectId, int permission)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            if (project == null)
            {
                return null;
            }

            var acl = Acl.CreateForProject(toUserId, projectId, permission);
            company.AddOrReplaceAcl(acl);

            var externalAcl = await SendExternalAcl(toUserId, projectId, company.Name, project.Name, project.Avatar);
            await SaveCompany(company);
            return externalAcl;
        }

        private async Task<ExternalAcl> SendExternalAcl(string userId, string projectId, string companyName, string projectName, string projectAvatar)
        {
            var reverseEntry = AclEntry.Create(CompanyId, ResourceType.Project, projectId);
            var externalAcl = ExternalAcl.Create(reverseEntry, companyName, projectName, projectAvatar);
            var externalActor = ActorFabric.GetProxy<ICompanyActor>(userId);
            await externalActor.RegisterExternalAcl(externalAcl);
            return externalAcl;
        }

        public async Task ChangeProjectSettings(string userId, string projectId, ProjectSettings settings)
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
                    externalTasks.Add(SendExternalAcl(acl.Entry.Sid, projectId, company.Name, settings.Name, settings.Avatar));
                }
                await Task.WhenAll(externalTasks);

                project.Name = settings.Name;
                project.Avatar = settings.Avatar;
                await SaveCompany(company);
            }
        }

        public async Task<CompanyInfo> GetCompanyInfo()
        {
            var company = await GetCompany();
            var owner = company.GetOwner();
            return new CompanyInfo
            {
                Name = company.Name,
                Logo = company.Logo,
                Owner = new UserInfo
                {
                    Avatar = owner.Avatar,
                    Email = owner.Email,
                    Name = owner.Name
                }
            };
        }
        public async Task UpdateOwnerInfo(UserInfo info)
        {
            var company = await GetCompany();

            var owner = company.GetOwner();
            owner.Name = info.Name;
            owner.Email = info.Email;
            owner.Avatar = info.Avatar;
            company.AddOrReplaceUser(owner);

            await SaveCompany(company);
        }

        public async Task UpdateCompanyInfo(CompanyInfo info)
        {
            var company = await GetCompany();

            if (company.Name != info.Name)
            {
                var externalTasks = new List<Task>();
                foreach (var userId in company.Acls.Select(x => x.Entry.Sid).Distinct())
                {
                    var actor = ActorFabric.GetProxy<ICompanyActor>(userId);
                    externalTasks.Add(actor.UpdateExternalCompanyName(CompanyId, info.Name));
                }
                await Task.WhenAll(externalTasks);

                company.Name = info.Name;
            }

            var owner = company.GetOwner();
            owner.Name = info.Owner.Name;
            owner.Email = info.Owner.Email;
            owner.Avatar = info.Owner.Avatar;

            await SaveCompany(company);
        }

        //TODO: change to users
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
            if(string.Compare(company.Name, companyName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return company.Id;
            }

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
            return project.MirroringCodes.FirstOrDefault(c=>c.UserId == userId)?.Id;
        }

        public async Task<string> SetProjectMirrorCode(string userId, string projectId, string code)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);
            var oldCode = project.MirroringCodes.FirstOrDefault(c=>c.UserId == userId)?.Id;
            if (code == null)
            {
                var codeToDelete = project.MirroringCodes.FirstOrDefault(c => c.UserId == userId);
                project.MirroringCodes.Remove(codeToDelete);
            }
            else
            {
                project.MirroringCodes.Add(new MirroringCode { Id = code, UserId = userId });
            }
            await SaveCompany(company);

            return oldCode;
        }

        public async Task<IEnumerable<ProjectShareCode>> GetProjectShareCodes(string userId, string projectId)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);
            return project.ShareCodes;
        }

        public async Task AddProjectShareCode(string userId, string projectId, ProjectShareCode code)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);

            project.AddOrReplaceShareCode(code);
            await SaveCompany(company);
        }

        public async Task RemoveProjectShareCode(string userId, string projectId, string codeId)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);

            var code = project.ShareCodes.SingleOrDefault(x => x.Id == codeId);
            if (code != null)
            {
                project.ShareCodes.Remove(code);
            }
            await SaveCompany(company);
        }

        public async Task RemoveProjectShareCodes(string userId, string projectId)
        {
            var company = await GetCompany();
            var project = FindProject(company, projectId);
            ValidateProjectPermission(company, project, userId, Permission.Write);

            project.ShareCodes.Clear();
            await SaveCompany(company);
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


        public async Task<List<Project>> GetRecentProjects()
        {
            var company = await GetCompany();
            return company.GetRecentProjectList();
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
            if (acl == null || !acl.Allows(requested))
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
