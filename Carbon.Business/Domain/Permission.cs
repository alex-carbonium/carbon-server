using System;
namespace Carbon.Business.Domain
{
    [Flags]
    public enum Permission : int
    {        
        None = 0,
        //0x00000010
        //Save = 0x00000002,
        ChangeStatus = 0x00000001,
        Read = 0x00000002,
        Write = 0x00000004 | Read,
        CreateProject = 0x00000008,
        DeleteProject = 0x00000010,
        //0x00000060
        ReadComments =  0x00000020,
        WriteComments = 0x00000040 | ReadComments,
        DeleteComments =  0x00000060 | WriteComments,
        ManageComments = ReadComments | WriteComments | DeleteComments,
        ManagePublishing = 0x00000080,
        Export = 0x00000100,
        Share = 0x00000200,
        CreateNewFolder = 0x00000400,
        DeleteFolder = 0x00000800 | DeleteProject,
        EditFolder = 0x00001000,
        ManageFolder = CreateNewFolder | EditFolder | DeleteFolder,
        ManageSecurity = 0x00002000,
        NonRestrictedExperience = 0x00004000,        
        Admin = 0x0FFFFFFF,
        Owner = 0x1FFFFFFF
    }    

    public class PredefinedRoles
    {
        public const Permission OwnerRole = Permission.Owner;
        public const Permission AdminRole = Permission.Admin;
        
        public const Permission ViewOnly = Permission.Read;

        public const Permission Comment = Permission.WriteComments |
                                          Permission.Read |
                                          Permission.ChangeStatus;

        public const Permission Edit = Permission.Write |
                                       Permission.ManageComments |
                                       Permission.ChangeStatus |
                                       Permission.Export |
                                       Permission.Share;
    }
}
