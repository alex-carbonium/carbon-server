using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Owin.Common.WebApi
{
    public class WebApiUnitOfWorkFilterAttribute : ActionFilterAttribute
    {
#if DEBUG
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            BeginRequest(actionContext);
            base.OnActionExecuting(actionContext);            
        }

        private void BeginRequest(HttpActionContext context)
        {
            context.Request.Properties["Carbon.uow"] = context.Request.GetDependencyScope().GetService(typeof(IUnitOfWork));
        }
#endif

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {            
            base.OnActionExecuted(actionExecutedContext);
            EndRequest(actionExecutedContext);
        }

        private void EndRequest(HttpActionExecutedContext context)
        {
            var uow = (IUnitOfWork)context.Request.GetDependencyScope().GetService(typeof(IUnitOfWork));
#if DEBUG
            if (uow != context.Request.Properties["Carbon.uow"])
            {
                throw new InvalidOperationException("Multiple units of work created per request");
            }
#endif

            if (context.Exception == null)
            {
                uow.Commit();
            }
            else
            {
                uow.Rollback();
            }
        }
    }
}