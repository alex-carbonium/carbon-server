using Ninject.Extensions.Factory;
using System;
using System.Reflection;

namespace Carbon.Framework.Dependencies
{
    public class TypeFinderProvider : StandardInstanceProvider
    {
        private readonly Func<MethodInfo, object[], Type> _typeFinder;

        public TypeFinderProvider(Func<MethodInfo, object[], Type> typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override Type GetType(MethodInfo methodInfo, object[] arguments)
        {
            return _typeFinder(methodInfo, arguments);
        }
    }
}
