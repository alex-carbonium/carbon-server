using Carbon.Framework.Logging;
using Carbon.Framework.Util;
using System;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Threading.Tasks;

namespace Carbon.Fabric.Common.Logging
{
    [EventSource(Name = "Carbon-Common")]
    public sealed class CommonEventSource : EventSource, ILogger
    {
        public static readonly CommonEventSource Current = new CommonEventSource();

        static CommonEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        [NonEvent]
        public void Trace(string message, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonTrace(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: message,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Info(string message, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonInfo(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: message,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Warning(string message, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonWarning(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: message,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Error(string message, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonError(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: message,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Error(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonError(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: ex.ToString(),
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Fatal(string message, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonCritical(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: message,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [NonEvent]
        public void Fatal(Exception ex, IDependencyContainer scope = null, string source = null)
        {
            if (IsEnabled())
            {
                var operation = scope?.TryResolve<OperationContext>();
                var serviceContext = scope?.TryResolve<ServiceContext>();

                CarbonCritical(serviceName: serviceContext?.ServiceName.ToString() ?? string.Empty,
                    replicaOrInstanceId: GetReplicaOrInstanceId(serviceContext) ?? string.Empty,
                    partitionId: serviceContext?.PartitionId.ToString() ?? string.Empty,
                    message: ex.ToString() ?? string.Empty,
                    nodeName: serviceContext?.NodeContext.NodeName ?? string.Empty,
                    source: source ?? string.Empty,
                    companyId: operation?.CompanyId ?? string.Empty,
                    userId: operation?.UserId ?? string.Empty,
                    operationId: operation?.OperationId ?? string.Empty,
                    projectId: operation?.ProjectId ?? string.Empty,
                    sessionId: operation?.SessionId ?? string.Empty);
            }
        }

        [Event(1, Level = EventLevel.Critical, Message = "{0}")]
        private void CarbonCritical(string message, string serviceName, string replicaOrInstanceId, string partitionId,
            string nodeName, string source, string userId, string sessionId, string operationId, string companyId, string projectId)
        {
            WriteEvent(1, message, serviceName, replicaOrInstanceId, partitionId, nodeName, source, userId, sessionId, operationId, companyId, projectId);
        }

        [Event(2, Level = EventLevel.Error, Message = "{0}")]
        private void CarbonError(string message, string serviceName, string replicaOrInstanceId, string partitionId,
            string nodeName, string source, string userId, string sessionId, string operationId, string companyId, string projectId)
        {
            WriteEvent(2, message, serviceName, replicaOrInstanceId, partitionId, nodeName, source, userId, sessionId, operationId, companyId, projectId);
        }

        [Event(3, Level = EventLevel.Warning, Message = "{0}")]
        private void CarbonWarning(string message, string serviceName, string replicaOrInstanceId, string partitionId,
            string nodeName, string source, string userId, string sessionId, string operationId, string companyId, string projectId)
        {
            WriteEvent(3, message, serviceName, replicaOrInstanceId, partitionId, nodeName, source, userId, sessionId, operationId, companyId, projectId);
        }

        [Event(4, Level = EventLevel.Informational, Message = "{0}")]
        private void CarbonInfo(string message, string serviceName, string replicaOrInstanceId, string partitionId,
            string nodeName, string source, string userId, string sessionId, string operationId, string companyId, string projectId)
        {
            WriteEvent(4, message, serviceName, replicaOrInstanceId, partitionId, nodeName, source, userId, sessionId, operationId, companyId, projectId);
        }

        [Event(5, Level = EventLevel.Verbose, Message = "{0}")]
        private void CarbonTrace(string message, string serviceName, string replicaOrInstanceId, string partitionId,
            string nodeName, string source, string userId, string sessionId, string operationId, string companyId, string projectId)
        {
            WriteEvent(5, message, serviceName, replicaOrInstanceId, partitionId, nodeName, source, userId, sessionId, operationId, companyId, projectId);
        }

        private string GetReplicaOrInstanceId(ServiceContext serviceContext)
        {
            if (serviceContext == null)
            {
                return null;
            }

            StatelessServiceContext stateless = serviceContext as StatelessServiceContext;
            if (stateless != null)
            {
                return stateless.InstanceId.ToString();
            }

            StatefulServiceContext stateful = serviceContext as StatefulServiceContext;
            if (stateful != null)
            {
                return stateful.ReplicaId.ToString();
            }

            throw new NotSupportedException("Context type not supported.");
        }
    }
}
