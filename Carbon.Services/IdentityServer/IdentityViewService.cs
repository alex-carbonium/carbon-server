using System.IO;
using System.Threading.Tasks;
using Carbon.Framework;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Validation;
using IdentityServer3.Core.ViewModels;

namespace Carbon.Services.IdentityServer
{
    public class IdentityViewService : IViewService
    {
        private readonly ResourceCache _resourceCache;

        public IdentityViewService(ResourceCache resourceCache)
        {
            _resourceCache = resourceCache;
        }

        public Task<Stream> IndexHtml()
        {
            return Task.FromResult<Stream>(new MemoryStream(Defs.Encoding.GetBytes("")));
        }

        public Task<Stream> Login(LoginViewModel model, SignInMessage message)
        {
            return IndexHtml();
        }

        public Task<Stream> Logout(LogoutViewModel model, SignOutMessage message)
        {
            return IndexHtml();
        }

        public Task<Stream> LoggedOut(LoggedOutViewModel model, SignOutMessage message)
        {
            return IndexHtml();
        }

        public Task<Stream> Consent(ConsentViewModel model, ValidatedAuthorizeRequest authorizeRequest)
        {
            return IndexHtml();
        }

        public Task<Stream> ClientPermissions(ClientPermissionsViewModel model)
        {
            return IndexHtml();
        }

        public Task<Stream> Error(ErrorViewModel model)
        {
            return IndexHtml();
        }
    }
}