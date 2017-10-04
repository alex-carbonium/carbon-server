using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Business.Sync;
using Carbon.Business.Sync.Handlers;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.Framework.Util;

namespace Carbon.Business.Services
{
    public class ProjectModelService
    {
        private readonly IRepository<ProjectModel> _projectModelRepository;
        private readonly ILogService _logService;
        private readonly IActorFabric _actorFabric;
        private readonly PermissionService _permissionService;
        private readonly ActiveProjectTrackingService _trackingService;

        public ProjectModelService(
            IRepository<ProjectModel> projectModelRepository,
            ILogService logService,
            IActorFabric actorFabric,
            PermissionService permissionService,
            ActiveProjectTrackingService trackingService)
        {
            _projectModelRepository = projectModelRepository;
            _logService = logService;
            _actorFabric = actorFabric;
            _permissionService = permissionService;
            _trackingService = trackingService;
        }

        public async Task<ProjectModel> ChangeProjectModel(IDependencyContainer scope, ProjectModelChange change)
        {
            Task<Permission> permissionTask;
            Task<ProjectModel> modelTask;

            if (string.IsNullOrEmpty(change.ModelId))
            {
                modelTask = CreateProjectAndModel(change.UserId, change.CompanyId);
                permissionTask = Task.FromResult(Permission.Write);
            }
            else
            {
                permissionTask = _permissionService.GetProjectPermission(change.UserId, change.CompanyId, change.ModelId);
                modelTask = FindProjectModel(change.CompanyId, change.ModelId);
            }

            var primitivesList = change.EnsurePrimitivesParsed();
            await Task.WhenAll(modelTask, permissionTask);

            var model = modelTask.Result;

            if (primitivesList.Count > 0)
            {
                PrimitiveHandler.ApplyImmediate(primitivesList, model, scope);

                model.Change = change;
                var originalVersion = model.EditVersion;
                await UpdateProjectModel(model, permissionTask.Result);
                var fromVersion = model.PreviousEditVersion;

                if (originalVersion != fromVersion)
                {
                    _logService.GetLogger().Warning($"Server sync conflict. Original: {originalVersion}, fromVersion: {fromVersion}", scope);
                }
            }
            else if (!permissionTask.Result.HasFlag(Permission.Read))
            {
                throw new InsufficientPermissionsException(Permission.Read, permissionTask.Result, change.UserId, change.CompanyId, change.ModelId);
            }

            return model;
        }

        public async Task<ProjectModel> CreateProjectAndModel(string userId, string companyId)
        {
            var companyActor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            var project = await companyActor.CreateProject(userId, folderId: null);

            if (project == null)
            {
                throw new InsufficientPermissionsException(Permission.CreateProject);
            }

            var projectModel = ProjectModel.CreateNew(companyId, project.Id);
            await _projectModelRepository.InsertAsync(projectModel);

            return projectModel;
        }


        public async Task<List<Project>> GetRecentProjects(string companyId)
        {
            var companyActor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            return await companyActor.GetRecentProjects();
        }

        private async Task<ProjectModel> FindProjectModel(string companyId, string projectId)
        {
            var key = GetProjectModelKey(companyId, projectId);
            return await _projectModelRepository.FindByIdAsync(key);
        }

        public async Task UpdateProjectModel(ProjectModel projectModel, Permission permission)
        {
            var requestedPermission = projectModel.Change.GetRequestedPermission();
            if ((requestedPermission & permission) != requestedPermission)
            {
                throw new InsufficientPermissionsException(requestedPermission, permission);
            }
            await _projectModelRepository.UpdateAsync(projectModel);
            await _trackingService.MarkActiveProject(projectModel.CompanyId, projectModel.Id);
        }

        private static dynamic GetProjectModelKey(string companyId, string projectId)
        {
            dynamic key = new ExpandoObject();
            key.CompanyId = companyId;
            key.ProjectId = projectId;
            return key;
        }

        //public AccessibleProject FindProjectByShareCode(User user, string shareCode)
        //{
        //    var spec = Project.FindByShareCodeSpec(shareCode);
        //    var project = _unitOfWork.FindSingleBy(spec);
        //    if (project == null)
        //    {
        //        throw new NonExistingProjectException(string.Format("Project with a share code {0} does not exist", shareCode));
        //    }
        //    if (project.Shareability == ProjectShareability.None)
        //    {
        //        throw new ProjectNotSharedException();
        //    }
        //    ACL acl = null;
        //    if (user != null)
        //    {
        //        acl = _security.GetUserPermission(user, project);
        //        if (acl == null || !acl.Allows(Permission.Read))
        //        {
        //            acl = _security.AssignPermissionForProject(project, user, PredefinedRoles.GuestRole);
        //        }
        //    }
        //    else
        //    {
        //        acl = ACL.CreateForProject(project.Id, Guid.Empty, PredefinedRoles.GuestRole);
        //    }
        //    return new AccessibleProject(project, acl);
        //}

        public void DeleteProject(User user, Project project)
        {
//            _security.AssertProjectPermission(project, user, Permission.DeleteProject);
//            var comments = _unitOfWork.FindAllBy(Comment.FindByProjectSpec(project.Id));
//            _unitOfWork.DeleteAll(comments);
//            _unitOfWork.Delete(project);
        }

        //public async Task<byte[]> ExportProjectPackage(User user, long projectId)
        //{
        //    var projectModel = await FindProjectModel(user, projectId, Permission.Export);
        //    return await ExportProjectPackage(projectModel);
        //}

        //private async Task<byte[]> ExportProjectPackage(ProjectModel projectModel)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var zip = Package.Open(stream, FileMode.Create))
        //        {
        //            var projectFile = await GetProjectFileContent(projectModel);
        //            const string destFilename = ".\\" + ProjectDescriptorName;
        //            var uri = PackUriHelper.CreatePartUri(new Uri(destFilename, UriKind.Relative));
        //            if (zip.PartExists(uri))
        //            {
        //                zip.DeletePart(uri);
        //            }
        //            var part = zip.CreatePart(uri, "", CompressionOption.NotCompressed);
        //            using (var dest = part.GetStream())
        //            using (var writer = new StreamWriter(dest))
        //            {
        //                writer.Write(projectFile);
        //                writer.Flush();
        //            }
        //        }
        //        return stream.ToArray();
        //    }
        //}

//        public async Task<ProjectModel> ImportProject(User user, string data)
//        {
//            var info = (JObject)JsonConvert.DeserializeObject(data);
//            var descriptor = new MetaProjectModel
//                                 {
//                                     Name = info.Value<string>("name"),
//                                     Theme = info.Value<string>("theme"),
//                                     Data = info["contents"].ToString(),
//                                     ProjectType = info.Value<string>("type")
//                                 };
//
//            var model = await SaveProjectModel(user, descriptor);
//
//            _unitOfWork.Update(model.Project);
//
//            return model;
//        }

        //public async Task<ProjectModel> ImportProjectPackage(User user, Stream stream)
        //{
        //    ProjectModel projectModel = null;
        //    using (var package = Package.Open(stream))
        //    {
        //        foreach (var part in package.GetParts())
        //        {
        //            var path = part.Uri.ToString();
        //            if (Path.GetFileName(path) == ProjectDescriptorName)
        //            {
        //                using (var streamReader = new StreamReader(part.GetStream(), Encoding))
        //                {
        //                    var content = streamReader.ReadToEnd();
        //                    projectModel = await ImportProject(user, content);
        //                }
        //            }
        //        }
        //    }

        //    if (projectModel == null)
        //    {
        //        throw new Exception(string.Format("User {0} cannot import project", user.Id));
        //    }

        //    return projectModel;
        //}

        //public async Task<ProjectModel> DuplicateProject(User user, long projectId, long? folderId = null)
        //{
        //    var model = await FindProjectModel(user, projectId, Permission.Read);
        //    var meta = await MetaProjectModel.CreateFromProjectModel(model);

        //    meta.Name = "Copy of " + model.Project.Name;
        //    if (folderId.HasValue)
        //    {
        //        meta.FolderId = folderId.Value;
        //    }

        //    var clonedData = await DuplicateProject(user, meta);
        //    clonedData.Project.Name = meta.Name;
        //    clonedData.Project.Shareability = ProjectShareability.None;

        //    return clonedData;
        //}

        //public ProjectFolder AddNewFolder(User user, long parentId, string name)
        //{
        //    var parentFolder = _unitOfWork.FindById<ProjectFolder>(parentId);
        //    _security.AssertFolderPermission(parentId, user.SID, Permission.CreateNewFolder);
        //    var folder = new ProjectFolder {Name = name, Company = parentFolder.Company, Parent = parentFolder};
        //    _unitOfWork.Insert(folder);
        //    foreach (var acl in parentFolder.Acls)
        //    {
        //        _unitOfWork.Insert(ACL.CreateForFolder(folder.Id, acl.SID, acl.Permission));
        //    }

        //    return folder;
        //}

        //private void DeleteFolder(User user, ProjectFolder folder)
        //{
        //    _security.AssertFolderPermission(folder.Id, user.SID, Permission.DeleteFolder);
        //    _unitOfWork.Delete(folder);
        //}

        //public void DeleteFolder(User user, long folderId)
        //{
        //    var folder = _unitOfWork.FindById<ProjectFolder>(folderId);
        //    this.DeleteFolder(user, folder);
        //}

        //public void RenameFolder(User user, long folderId, string newName)
        //{
        //    var folder = _unitOfWork.FindById<ProjectFolder>(folderId);
        //    _security.AssertFolderPermission(folderId, user.SID, Permission.EditFolder);

        //    folder.Name = newName;

        //    _unitOfWork.Update(folder);
        //}
    }
}