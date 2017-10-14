using Carbon.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Business.Domain
{
    public class Company : IDomainObject<string>
    {
        private readonly HashSet<Acl> _acls = new HashSet<Acl>(UniqueAclEntryComparer.Default);
        private readonly HashSet<User> _users = new HashSet<User>(User.UniqueComparer);
        private readonly HashSet<ExternalAcl> _externalAcls = new HashSet<ExternalAcl>(UniqueAclEntryComparer.Default);
        private readonly HashSet<CompanyFileInfo> _files = new HashSet<CompanyFileInfo>(CompanyFileInfo.UniqueComparer);

        public Company()
        {
            //Invitations = new List<CompanyInvitation>();
            //Bills = new List<Bill>();
            //Acls = new List<Acl>();
            //ProjectFolders = new List<ProjectFolder>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public ProjectFolder RootFolder { get; set; }
        public ProjectFolder DeletedFolder { get; set; }

        private const int RecentCount = 4;
        public string[] RecentProjects { get; set; }
        //public string WebsiteUrl { get; set; }

        public ICollection<User> Users => _users;
        public ICollection<Acl> Acls => _acls;
        public ICollection<ExternalAcl> ExternalAcls => _externalAcls;
        public ICollection<CompanyFileInfo> Files => _files;

        //public IList<CompanyInvitation> Invitations { get; set; }
        //public IList<ProjectFolder> ProjectFolders { get; set; }
        //public IList<Bill> Bills { get; set; }

        //public CompanyInvitation AddInvitation(string email, string code = "", bool purchased = false, Permission asRole = Permission.Read)
        //{
        //    var invitation = new CompanyInvitation {Email = email, Purchased = purchased, Code = code, AsRole = asRole};
        //    Invitations.Add(invitation);
        //    return invitation;
        //}

        //public ProjectFolder AddFolder(string name)
        //{
        //    var folder = new ProjectFolder();
        //    folder.Name = name;
        //    ProjectFolders.Add(folder);
        //    return folder;
        //}

        //public Permission GetUserPermission(User user)
        //{
        //    if(user == null)
        //    {
        //        return Permission.None;
        //    }
        //    var acl = this.Acls.FirstOrDefault(a => a.Sid == user.Id);
        //    return acl == null ? Permission.None : acl.Permission;
        //}

        //public long GetSizeOfFiles()
        //{
        //    return Files.Sum(x => x.Size);
        //}

        public void AddOrReplaceAcl(Acl acl)
        {
            _acls.Remove(acl);
            _acls.Add(acl);
        }

        public void AddOrReplaceExternalAcl(ExternalAcl acl)
        {
            _externalAcls.Remove(acl);
            _externalAcls.Add(acl);
        }

        public void AddOrReplaceUser(User user)
        {
            _users.Remove(user);
            _users.Add(user);
        }

        public IEnumerable<Acl> FindFolderAcls(ProjectFolder folder)
        {
            return Acls.Where(x => x.Entry.ResourceType == ResourceType.Folder && x.Entry.ResourceId == folder.Id);
        }

        public bool HasFolderPermission(string userId, ProjectFolder folder, Permission permission)
        {
            if (userId != Id)
            {
                var acls = FindFolderAcls(folder);
                var userAcl = acls.SingleOrDefault(x => x.Entry.Sid == userId);
                if (userAcl == null || !userAcl.Allows(Permission.CreateProject))
                {
                    return false;
                }
            }

            return true;
        }

        public void PropagateFolderAcls(ProjectFolder folder, Project project)
        {
            var folderAcls = FindFolderAcls(folder);
            foreach (var acl in folderAcls)
            {
                AddOrReplaceAcl(Acl.CreateForProject(acl.Entry.Sid, project.Id, acl.Permission));
            }
        }

        public void AddOrReplaceFile(CompanyFileInfo file)
        {
            _files.Remove(file);
            _files.Add(file);
        }
        public void RemoveFile(string name)
        {
            var file = _files.SingleOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (file != null)
            {
                _files.Remove(file);
            }
        }

        public User GetOwner()
        {
            return _users.SingleOrDefault(x => x.Id == Id);
        }

        public void RemoveRecentRef(string projectId)
        {
            if (this.RecentProjects == null)
            {
                return;
            }

            var index = -1;
            for (var i = 0; i < RecentCount; ++i)
            {
                if (this.RecentProjects[i] == projectId)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                return;
            }

            for (; index < RecentCount; ++index)
            {
                this.RecentProjects[index] = this.RecentProjects[index + 1]; // we always should have count of item as RecentCount +1, where the last item is null always.
            }
        }

        public void AddRecentRef(string projectId)
        {
            if (this.RecentProjects == null || this.RecentProjects.Length != RecentCount + 1)
            {
                this.RecentProjects = new string[RecentCount + 1];
            }

            this.RemoveRecentRef(projectId);
            for (var i = RecentCount - 1; i >= 1; --i)
            {
                this.RecentProjects[i] = this.RecentProjects[i - 1];
            }

            this.RecentProjects[RecentCount] = null;
            this.RecentProjects[0] = projectId;
        }

        public List<Project> GetRecentProjectList()
        {
            if (this.RecentProjects == null)
            {
                return new List<Project>();
            }

            return this.RecentProjects.Select(projectId =>
            {
                return this.RootFolder.Projects.SingleOrDefault(project => project.Id == projectId);
            }).Where(p => p != null).ToList();
        }
    }
}