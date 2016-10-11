using Ninject;
using Ninject.Extensions.NamedScope;
using System;
using System.Collections.Generic;
using Ninject.Syntax;

namespace Carbon.Framework.Util
{
    public class NinjectScopedDependencyContainer : NinjectDependencyContainer
    {
        private readonly NamedScope _scope;

        public NinjectScopedDependencyContainer(IKernel kernel, NamedScope scope)
            : base(kernel)
        {
            _scope = scope;
        }

        protected override IBindingToSyntax<T> Rebind<T>()
        {
            throw new InvalidOperationException("Binding on the scope is not allowed");
        }

        public override object Resolve(Type t)
        {
            return _scope.Get(t);
        }

        public override T Resolve<T>()
        {
            return _scope.Get<T>();
        }

        public override IEnumerable<object> ResolveMany(Type t)
        {
            return _scope.GetAll(t);
        }

        public override void Dispose()
        {
            _scope.Dispose();
        }
    }    
}
