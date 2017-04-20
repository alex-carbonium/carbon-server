using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Microsoft.AspNet.SignalR;

namespace Carbon.StorageService.SignalR
{
    [Microsoft.AspNet.SignalR.Authorize]
    public class ModelSyncHub : BaseHub
    {
        private readonly ILogService _logService;
        private readonly ProjectModelService _projectModelService;

        protected virtual string GetUserId()
        {
            var userId = Operation.UserId;
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

        public async Task<dynamic> ChangeModel(string companyId, string folderId, string modelId, List<string> primitiveArray, bool returnModel)
        {
            try
            {
                Operation.CompanyId = companyId;
                Operation.ProjectId = modelId;

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
                if (model == null)
                {
                    throw new HubException("appNotFound");
                }

                if (hadNoId || returnModel)
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
                _logService.GetLogger().Fatal(ex, Scope);
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
                _logService.GetLogger().Fatal(ex, Scope);
                throw;
            }
        }
    }
}