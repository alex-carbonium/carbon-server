using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sketch.Business.Domain;
using Sketch.Business.Logging;
using Sketch.Business.Services;
using Sketch.Business.Sync;
using Sketch.Business.Sync.Handlers;
using Sketch.Framework.Logging;
using Sketch.Framework.UnitOfWork;

namespace Sketch.StorageService.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize]
    public class DataSyncHub : BaseHub
    {
        public IUserService UserService { get; set; }     
        public IIdentityProvider IdentityProvider { get; set; }        
        public ILogService LogService { get; set; }        
        public IDesignerService DesignerService { get; set; }        
        public IUnitOfWork UnitOfWork { get; set; }
        public ISecurity Security { get; set; }

        protected virtual User GetLoggedInUser()
        {
            return IdentityProvider.GetLoggedInUser(Context.User);
        }

        protected virtual User EnsureUserLoggedIn()
        {
            var user = GetLoggedInUser();
            if (user == null)
            {
                throw new InvalidOperationException("Unknown user");
            }
            return user;
        }        

        public DataSyncHub(IUserService userService, IIdentityProvider identityProvider, 
            ILogService logService, IDesignerService designerService, IUnitOfWork unitOfWork, ISecurity security)
        {
            UserService = userService;
            IdentityProvider = identityProvider;
            LogService = logService;
            DesignerService = designerService;
            UnitOfWork = unitOfWork;
            Security = security;
        }

        public void JoinProjectWithShareCode(string shareCode)
        {
            JoinProject(shareCode: shareCode);
        }

        public void JoinProject(long projectId)
        {                        
            JoinProject(id: projectId);
        }

        private void JoinProject(long? id = null, string shareCode = null)
        {
            try
            {
                var user = EnsureUserLoggedIn();
                
                AccessibleProject accessibleProject;
                if (id.HasValue)
                {
                    accessibleProject = DesignerService.FindAccessibleProject(user, id.Value);
                }
                else if (!string.IsNullOrEmpty(shareCode))
                {
                    accessibleProject = DesignerService.FindProjectByShareCode(user, shareCode);
                }
                else
                {
                    throw new InvalidOperationException("Either projectId or shareCode must be supplied");
                }
                
                if (accessibleProject != null)
                {
                    Groups.Add(Context.ConnectionId, "Project_" + accessibleProject.Project.Id);
                }  
                UnitOfWork.Commit();              
            }
            catch (Exception ex)
            {
                LogService.GetLogger(this).Error(ex);
                throw;
            }
            finally
            {
                UnitOfWork.Dispose();
            }
        }

        public async Task<string> UpdateProjectInitial(long projectId, string[] primitiveArray)
        {
            var primitives = PrimitiveFactory.CreateMany<RawPrimitive>(primitiveArray);
            return await ChangeProject(projectId, primitives, true);
        }

        public async Task<string> UpdateProject(long projectId, string[] primitiveArray, bool returnModel)
        {
            var primitives = PrimitiveFactory.CreateMany<RawPrimitive>(primitiveArray);
            return await ChangeProject(projectId, primitives, returnModel);
        }
        
        private async Task<string> ChangeProject(long projectId, IEnumerable<RawPrimitive> primitives, bool returnModel)
        {            
            try
            {
                var res = "";
                ProjectModel projectModel = null;
                var user = GetLoggedInUser();
                var primitivesList = primitives.ToList(); //TODO: don't wait here with service fabric                         
                var permission = PrimitiveHandler.FindRequiredPermission(primitivesList);
                projectModel = await DesignerService.FindOrCreateProjectModel(user, projectId, permission);

                if (primitivesList.Count > 0)
                {
                    var context = new Lazy<PrimitiveContext>(() => PrimitiveContext.Create(Container));
                    //var fullModelNeeded = map.Any(x => x.Value.RequiresFullModel);
                    //if (fullModelNeeded)
                    //{
                    //    await projectModel.EnsureLoaded();
                    //}
                    var deferred = PrimitiveHandler.ApplyImmediate(primitivesList, projectModel, () => context.Value);
                    projectModel.Primitives = deferred;

                    var originalVersion = projectModel.EditVersion;
                    await DesignerService.UpdateProjectModel(projectModel);
                    var fromVersion = projectModel.PreviousEditVersion;
                    var toVersion = projectModel.EditVersion;
                    
                    if (originalVersion != fromVersion)
                    {
                        LogService.GetLogger(this).WarningWithContext("Server sync conflict", Container, c =>
                        {
                            c["projectId"] = projectId.ToString();
                            c["originalVersion"] = originalVersion;
                            c["fromVersion"] = fromVersion;
                        });
                    }

                    if (context.IsValueCreated && context.Value.ExtraPrimitives != null)
                    {
                        primitivesList.AddRange(context.Value.ExtraPrimitives);
                    }

                    Clients.Group("Project_" + projectId)
                        .projectChanged(projectId, primitivesList, fromVersion, toVersion);
                }

                if (returnModel)
                {
                    res = await projectModel.ToStringCompact();
                }
                else
                {
                    res = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        version = projectModel.EditVersion,
                        id = projectModel.Id
                    });
                }

                UnitOfWork.Commit();
                return res;
            }
            catch (Exception ex)
            {
                LogService.GetLogger(this).FatalWithContext("Could not apply primitives", Container,
                    c =>
                    {
                        //TODO: log primitives in separate entity
                        c["projectId"] = projectId.ToString();                        
                        c["exception"] = ex.ToString();
                    });
                throw;
            }
            finally
            {
                UnitOfWork.Dispose();
            }
        }      
    }
}