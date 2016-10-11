using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Carbon.Framework.Util;

namespace Carbon.Owin.Common.WebApi
{
    public class NinjectWebApiScope : IDependencyScope
    {
        private readonly IDependencyContainer _container;

        public NinjectWebApiScope(IDependencyContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.ResolveMany(serviceType);
        }
    }
}