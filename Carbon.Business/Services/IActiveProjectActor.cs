using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Carbon.Business.Services
{
    public interface IActiveProjectActor : IActor
    {
        Task<string> GetMachineName();
    }
}
