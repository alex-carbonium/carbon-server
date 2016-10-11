using Carbon.Business.Services;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Carbon.Fabric.Common
{
    public class ActorFabric : IActorFabric
    {
        public static readonly ActorFabric Default = new ActorFabric();

        public T GetProxy<T>(string id) where T : IActor
        {
            return ActorProxy.Create<T>(new ActorId(id));
        }
    }
}
