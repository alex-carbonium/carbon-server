namespace Carbon.Business.Domain
{
    public class Acl : IAclEntryContainer
    {
        public AclEntry Entry { get; set; }
        public int Permission { get; set; }        

        public bool Allows(Permission requestedPermission)
        {
            return ((Permission)Permission).HasFlag(requestedPermission);
        }

        public static Acl CreateForProject(string userId, string projectId, int permission)
        {
            return new Acl
            {
                Entry = AclEntry.Create(userId, ResourceType.Project, projectId),
                Permission = permission
            };
        }

        public static Acl CreateForFolder(string userId, string folderId, int permission)
        {
            return new Acl
            {
                Entry = AclEntry.Create(userId, ResourceType.Folder, folderId),
                Permission = permission
            };
        }

        public static Acl CreateForCompany(string userId, string companyId, int permission)
        {
            return new Acl
            {
                Entry = AclEntry.Create(userId, ResourceType.Company, companyId),
                Permission = permission
            };
        }
    }
}
