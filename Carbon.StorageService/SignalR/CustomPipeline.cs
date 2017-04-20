using System;
using System.Reflection;
using Microsoft.AspNet.SignalR.Hubs;
using Carbon.Framework.Logging;
using Carbon.Owin.Common.Security;

namespace Carbon.StorageService.SignalR
{
    public class CustomPipeline : HubPipelineModule
    {
        private readonly ILogger _logger;

        public CustomPipeline(ILogService logService)
        {
            _logger = logService.GetLogger();
        }

        protected override bool OnBeforeIncoming(IHubIncomingInvokerContext context)
        {
            var hub = (BaseHub)context.Hub;
            hub.Operation.SetUserId(context.Hub.Context.Request.User);
            return base.OnBeforeIncoming(context);
        }

        protected override void OnIncomingError(ExceptionContext exceptionContext, IHubIncomingInvokerContext invokerContext)
        {
            var exception = exceptionContext.Error;
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            else if (exception is AggregateException)
            {
                exception = exception.InnerException;
            }
            _logger.Error(exception, ((BaseHub)invokerContext.Hub).Scope);
        }
    }
}