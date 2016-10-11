using Microsoft.ServiceFabric.Actors;

namespace Carbon.Business.Services
{
    public interface IActorFabric
    {
        T GetProxy<T>(string id) where T : IActor;
    }
}