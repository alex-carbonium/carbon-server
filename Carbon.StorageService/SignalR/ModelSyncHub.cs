using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Carbon.Business.Domain;
using Carbon.Business.Logging;
using Carbon.Business.Services;
using Carbon.Framework.Logging;

namespace Carbon.StorageService.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize]
    public class ModelSyncHub : BaseHub
    {
        private readonly ILogService _logService;
        private readonly ProjectModelService _projectModelService;        

        protected virtual string GetUserId()
        {
            var userId = IdentityContext.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Unknown user");
            }
            return userId;
        }        

        public ModelSyncHub(ILogService logService, ProjectModelService projectModelService)
        {                      
            _logService = logService;
            _projectModelService = projectModelService;            
        }

        public async Task Join(string companyId, string modelId)
        {                        
            try
            {
                var sid = GetUserId();                
                if (string.IsNullOrEmpty(companyId))
                {
                    companyId = sid;
                }
                var permission = await _projectModelService.GetProjectPermission(sid, companyId, modelId);

                if (permission != Permission.None)
                {                    
                    await Groups.Add(Context.ConnectionId, "Project_" + modelId);
                }                  
            }
            catch (Exception ex)
            {
                _logService.GetLogger(this).Error(ex);
                throw;
            }            
        }

        public async Task<dynamic> ChangeModel(string companyId, string folderId, string modelId, List<string> primitiveArray, bool returnModel)
        {
            try
            {
                var userId = GetUserId();
                var change = new ProjectModelChange
                {                    
                    FolderId = folderId,
                    CompanyId = companyId,
                    ModelId = modelId,
                    UserId = userId,
                    PrimitiveStrings = primitiveArray
                };
                var hadNoId = string.IsNullOrEmpty(modelId);
                var model = await _projectModelService.ChangeProjectModel(Scope, change);
                if (hadNoId)
                {
                    await Groups.Add(Context.ConnectionId, "Project_" + model.Id);
                }

                if (change.PrimitiveStrings != null && change.PrimitiveStrings.Count > 0)
                {
                    Clients.Group("Project_" + modelId)
                        .modelChanged(change.PrimitiveStrings, model.PreviousEditVersion, model.EditVersion);
                }                                

                if (returnModel)
                {
                    return await model.WriteAsync();
                }
                return JsonConvert.SerializeObject(new
                {
                    editVersion = model.EditVersion,
                    id = model.Id
                });
            }
            catch (Exception ex)
            {
                _logService.GetLogger(this).FatalWithContext("Could not apply primitives", Scope,
                    c =>
                    {
                        //TODO: log primitives in separate entity
                        c["projectId"] = modelId;
                        c["exception"] = ex.ToString();
                    });
                throw;
            }                        
        }

        public void NotifyClients(string companyId, string modelId, List<string> messages)
        {
            try
            {
                if (messages.Count > 0)
                {
                    Clients.OthersInGroup("Project_" + modelId)
                        .externalClientNotification(messages);
                }

            }
            catch (Exception ex)
            {
                _logService.GetLogger(this).FatalWithContext("Could not notify clients", Scope,
                    c =>
                    {
                        //TODO: log primitives in separate entity
                        c["projectId"] = modelId;
                        c["exception"] = ex.ToString();
                    });
                throw;
            }
        }
    }
}