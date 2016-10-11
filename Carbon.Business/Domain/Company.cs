using System;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Business.Domain
{
    public class Company
    {
        private readonly HashSet<Acl> _acls = new HashSet<Acl>(UniqueAclEntryComparer.Default);
        private readonly HashSet<User> _users = new HashSet<User>();
        private readonly HashSet<ExternalAcl> _externalAcls = new HashSet<ExternalAcl>(UniqueAclEntryComparer.Default);
        private readonly HashSet<CompanyFileInfo> _files = new HashSet<CompanyFileInfo>(CompanyFileInfo.UniqueComparer);

        public Company()
        {            
            //Users = new List<User>();
            //Invitations = new List<CompanyInvitation>();
            //Bills = new List<Bill>();
            //Acls = new List<Acl>();            
            //ProjectFolders = new List<ProjectFolder>();
        }
        
        public string Name { get; set; }        
        public ProjectFolder RootFolder { get; set; }
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
    }
}