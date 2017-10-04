using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;

namespace Carbon.Business.Services
{
    public class ActiveProjectTrackingService
    {
        private static readonly TimeSpan InactivityTimeout = TimeSpan.FromHours(1);

        private readonly ConcurrentDictionary<Tuple<string, string>, Task<ActiveProject>> _cache = new ConcurrentDictionary<Tuple<string, string>, Task<ActiveProject>>();
        private readonly PermissionService _permissionService;
        private readonly IRepository<ActiveProject> _activeProjectRepository;
        private readonly ILogger _logger;

        public ActiveProjectTrackingService(PermissionService permissionService, IRepository<ActiveProject> activeProjectRepository, ILogService logService)
        {
            _permissionService = permissionService;
            _activeProjectRepository = activeProjectRepository;
            _logger = logService.GetLogger();
        }

        public async Task<int> GetConnectionPort(string userId, string companyId, string projectId)
        {
            if (string.IsNullOrEmpty(projectId))
            {
                return GetPortFromMachineName(Environment.MachineName);
            }

            var permission = await _permissionService.GetProjectPermission(userId, companyId, projectId);
            if (!permission.HasFlag(Permission.Read))
            {
                throw new InsufficientPermissionsException(Permission.Read, permission, userId, companyId, projectId);
            }

            var activeProject = await MarkActiveProject(companyId, projectId);
            return GetPortFromMachineName(activeProject.MachineName);
        }

        public async Task<ActiveProject> MarkActiveProject(string companyId, string projectId)
        {
            return await _cache.AddOrUpdate(Tuple.Create(companyId, projectId),
                t => MarkActiveProjectInternal(t.Item1, t.Item2),
                (t, oldValue) => UpdateCachedValue(oldValue));
        }

        private async Task<ActiveProject> UpdateCachedValue(Task<ActiveProject> activeProjectTask)
        {
            var activeProject = await activeProjectTask;
            if (IsInactive(activeProject))
            {
                return await MarkActiveProjectInternal(activeProject.PartitionKey, activeProject.RowKey);
            }
            return activeProject;
        }

        private int GetPortFromMachineName(string machineName)
        {
#if DEBUG
            return Defs.Azure.StoragePortStart;
#else
            if (machineName.Length < 3)
            {
                _logger.Fatal("Unexpected machine name: " + machineName);
                return Defs.Azure.StoragePortStart;
            }

            var lastDigits = machineName.Substring(machineName.Length - 3);
            int port;
            if (!int.TryParse(lastDigits, out port))
            {
                _logger.Fatal("Unexpected machine name: " + machineName);
                return Defs.Azure.StoragePortStart;
            }

            return Defs.Azure.StoragePortStart + port;
#endif
        }

        private async Task<ActiveProject> MarkActiveProjectInternal(string companyId, string projectId)
        {
            var spec = new FindByRowKey<ActiveProject>(companyId, projectId);
            var activeProject = await _activeProjectRepository.FindSingleByAsync(spec);

            if (activeProject == null)
            {
                activeProject = new ActiveProject(companyId, projectId, Environment.MachineName);
                activeProject = await InsertOrGetLatest(activeProject);
            }
            else
            {
                if (IsInactive(activeProject))
                {
                    activeProject.MachineName = Environment.MachineName;
                }
                activeProject.LastAccessed = DateTime.UtcNow;
                try
                {
                    await _activeProjectRepository.UpdateAsync(activeProject);
                }
                catch (UpdateConflictException)
                {
                }
            }

            return activeProject;
        }

        private static bool IsInactive(ActiveProject activeProject)
        {
            return DateTime.UtcNow - activeProject.LastAccessed > InactivityTimeout;
        }

        private async Task<ActiveProject> InsertOrGetLatest(ActiveProject activeProject)
        {
            try
            {
                await _activeProjectRepository.InsertAsync(activeProject);
                return activeProject;
            }
            catch (InsertConflictException)
            {
                var spec = new FindByRowKey<ActiveProject>(activeProject.PartitionKey, activeProject.RowKey);
                return await _activeProjectRepository.FindSingleByAsync(spec);
            }
        }
    }
}
