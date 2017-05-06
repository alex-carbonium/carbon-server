using System.Threading.Tasks;

namespace Carbon.Business.Services
{
    public interface IMailService
    {
        Task Send(string to, string template, dynamic model);
    }
}