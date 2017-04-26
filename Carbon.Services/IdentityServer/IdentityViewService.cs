using System.IO;
using System.Threading.Tasks;
using Carbon.Framework;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;
using Carbon.Framework.Logging;

namespace Carbon.Services.IdentityServer
{
    public class IdentityViewService : IViewService
    {
        private readonly ILogger _logger;

        public IdentityViewService(ILogService logService)
        {
            _logger = logService.GetLogger();
        }

        public Task<Stream> Redirect()
        {
            return Task.FromResult<Stream>(new MemoryStream(Defs.Encoding.GetBytes(
                "<script>location.href = '/e/idsrv';</script>")));
        }

        public Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            _logger.Fatal("Unexpected view Login");
            return Redirect();
        }

        public Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            _logger.Fatal("Unexpected view Logout");
            return Redirect();
        }

        public Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            _logger.Fatal("Unexpected view LoggedOut");
            return Redirect();
        }

        public Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            _logger.Fatal("Unexpected view Consent");
            return Redirect();
        }

        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            _logger.Fatal("Unexpected view ClientPermissions");
            return Redirect();
        }

        public Task<Stream> Error(ErrorViewModel model)
        {
            _logger.Fatal(model.ErrorMessage);
            return Redirect();
        }
    }
}