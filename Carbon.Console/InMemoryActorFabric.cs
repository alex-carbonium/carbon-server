using System;
using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Microsoft.ServiceFabric.Actors;
using Carbon.Framework.Logging;

namespace Carbon.Console
{
    public class InMemoryActorFabric : IActorFabric
    {
        private readonly Dictionary<string, ICompanyActor> _companyActorStore;

        public InMemoryActorFabric()
        {
            _companyActorStore = new Dictionary<string, ICompanyActor>();
        }

        public T GetProxy<T>(string id) where T : IActor
        {
            if (typeof(T) == typeof(ICompanyActor))
            {
                ICompanyActor actor;
                if (!_companyActorStore.TryGetValue(id, out actor))
                {
                    var companyActor = new CompanyActor(id, this, new InMemoryStateManager(), new ConsoleLogger());
#pragma warning disable 4014
                    companyActor.Activate();
#pragma warning restore 4014

                    actor = companyActor;
                    _companyActorStore.Add(id, actor);
                }
                return (T) actor;
            }
            throw new Exception("No stub for actor " + typeof(T).FullName);
        }
    }
}