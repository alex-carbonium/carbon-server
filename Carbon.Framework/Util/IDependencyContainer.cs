using System;
using System.Collections.Generic;
using System.Reflection;

namespace Carbon.Framework.Util
{
    public interface IDependencyContainer : IDisposable
    {
        T Resolve<T>();
        object Resolve(Type t);
        IEnumerable<object> ResolveMany(Type t);

        T TryResolve<T>();

        IDependencyContainer RegisterType<TFrom, TTo>() where TTo : TFrom;
        IDependencyContainer RegisterTypePerWebRequest<TFrom>(Func<TFrom> func);

        IDependencyContainer RegisterInstance<T>(T instance);

        IDependencyContainer RegisterTypeSingleton<TFrom, TTo>() where TTo : TFrom;

        IDependencyContainer RegisterTypePerWebRequest<TFrom, TTo>() where TTo : TFrom;

        IDependencyContainer RegisterFactory<T>(Func<MethodInfo, object[], Type> typeFinder = null) where T : class;
        IDependencyContainer RegisterFactorySingleton<T>(Func<MethodInfo, object[], Type> typeFinder = null) where T : class;

        IDependencyContainer BeginScope();
    }
}
