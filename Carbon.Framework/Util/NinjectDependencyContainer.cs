using System;
using System.Collections.Generic;
using System.Reflection;
using Carbon.Framework.Dependencies;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Extensions.NamedScope;
using Ninject.Syntax;

namespace Carbon.Framework.Util
{    
    public class NinjectDependencyContainer : IDependencyContainer
    {
        private const string DEFAULT_SCOPE = "skech_scope";
        private readonly IKernel _kernel;

        public NinjectDependencyContainer(IKernel kernel)
        {
            _kernel = kernel;                        
        }

        public virtual T Resolve<T>()
        {
            return _kernel.Get<T>();
        }

        public virtual object Resolve(Type t)
        {
            return _kernel.Get(t);
        }

        public virtual IEnumerable<object> ResolveMany(Type t)
        {
            return _kernel.GetAll(t);
        }

        public virtual IDependencyContainer RegisterType<TFrom, TTo>() where TTo : TFrom
        {
            Rebind<TFrom>().To<TTo>();
            return this;
        }

        public virtual IDependencyContainer RegisterTypePerWebRequest<TFrom>(Func<TFrom> func)
        {
            Rebind<TFrom>().ToMethod(ctx => func()).InNamedScope(DEFAULT_SCOPE);
            return this;
        }

        public virtual IDependencyContainer RegisterInstance<T>(T instance)
        {
            Rebind<T>().ToConstant(instance);
            return this;
        }

        public virtual IDependencyContainer RegisterTypeSingleton<TFrom, TTo>() where TTo : TFrom
        {
            Rebind<TFrom>().To<TTo>().InSingletonScope();
            return this;
        }

        public virtual IDependencyContainer RegisterTypePerWebRequest<TFrom, TTo>() where TTo : TFrom
        {
            Rebind<TFrom>().To<TTo>().InNamedScope(DEFAULT_SCOPE);
            return this;
        }

        public virtual IDependencyContainer RegisterFactory<T>(Func<MethodInfo, object[], Type> typeFinder = null) where T : class
        {
            IBindingWhenInNamedWithOrOnSyntax<T> binding;
            if (typeFinder != null)
            {
                binding = Rebind<T>().ToFactory(() => new TypeFinderProvider(typeFinder));
            }
            else 
            {
                binding = Rebind<T>().ToFactory();
            }

            binding.DefinesNamedScope(DEFAULT_SCOPE);

            return this;
        }

        public IDependencyContainer RegisterFactorySingleton<T>(Func<MethodInfo, object[], Type> typeFinder = null) where T : class
        {
            IBindingWhenInNamedWithOrOnSyntax<T> binding;
            if (typeFinder != null)
            {
                binding = Rebind<T>().ToFactory(() => new TypeFinderProvider(typeFinder));
            }
            else
            {
                binding = Rebind<T>().ToFactory();
            }
            binding.InSingletonScope();
            return this;
        }

        protected virtual IBindingToSyntax<T> Rebind<T>()
        {           
            return _kernel.Rebind<T>();
        }

        public IDependencyContainer BeginScope()
        {
            var scope = _kernel.CreateNamedScope(DEFAULT_SCOPE);            
            return new NinjectScopedDependencyContainer(_kernel, scope);
        }

        protected IKernel Kernel
        {
            get { return _kernel; }
        }

        public virtual void Dispose()
        {
            _kernel.Dispose();
        }
    }
}