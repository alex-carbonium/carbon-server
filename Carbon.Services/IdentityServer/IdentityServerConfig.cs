using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Carbon.Business;
using Owin;
using Carbon.Framework.Logging;
using Carbon.Framework.Util;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Logging;
using IdentityServer3.Core.Services;
using AuthenticationOptions = IdentityServer3.Core.Configuration.AuthenticationOptions;
using CookieOptions = IdentityServer3.Core.Configuration.CookieOptions;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
//using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Carbon.Business.Domain;
using Carbon.Business.Services;
//using Microsoft.Owin.Security.Twitter;

namespace Carbon.Services.IdentityServer
{
    public static class IdentityServerConfig
    {
        public static X509Certificate2 SigningCertificate { get; private set; }
        public static X509Certificate2 ProtectionCertificate { get; private set; }
        public static IdentityServerOptions Options { get; private set; }

        public static void Configure(IAppBuilder app, IDependencyContainer container, AppSettings appSettings)
        {
            var logService = container.Resolve<ILogService>();

            LogProvider.SetCurrentLogProvider(new LogProviderAdapter(logService));

            ProtectionCertificate = FindCertificate(appSettings.IdServer.ProtectionCertificateThumbprint);
            if (!string.IsNullOrEmpty(appSettings.IdServer.PrivateKeyReleaseThumbprint))
            {
                SigningCertificate = FindCertificate(appSettings.IdServer.PrivateKeyReleaseThumbprint);
            }
            else
            {
                SigningCertificate = new X509Certificate2(
                    appSettings.ResolvePath(Defs.Packages.Data, appSettings.IdServer.PrivateKeyFile),
                    appSettings.IdServer.PrivateKeyDebugPassword,
                    X509KeyStorageFlags.MachineKeySet);
            }

            var tokenProvider = new DataProtectorTokenProvider<ApplicationUser>(new IdentityDataProtector(ProtectionCertificate));

            var factory = new IdentityServerServiceFactory();
            factory.ScopeStore = new Registration<IScopeStore>(new IdentityScopeStore());
            factory.ClientStore = new Registration<IClientStore>(new IdentityClientStore());
            factory.UserService = new Registration<IUserService>(x => IdentityUserService.Create(appSettings, tokenProvider, container.Resolve<AccountService>()));
            factory.ViewService = new Registration<IViewService>(new IdentityViewService(logService));

            factory.CorsPolicyService = new Registration<ICorsPolicyService>(new CorsPolicyService());

            Options = new IdentityServerOptions
            {
                IssuerUri = "https://ppanda",
                SiteName = "Carbonium",
                RequireSsl = false,
                SigningCertificate = SigningCertificate,
                Factory = factory,
                DataProtector = new X509CertificateDataProtector(ProtectionCertificate),
                CspOptions = new CspOptions { Enabled = false },
                Endpoints = new EndpointOptions
                {
                    EnableCheckSessionEndpoint = false,
                    EnableEndSessionEndpoint = false
                },
                AuthenticationOptions = new AuthenticationOptions
                {
                    IdentityProviders = (idsrvApp, signInAsType) => ConfigureSocialIdentityProviders(idsrvApp, signInAsType, appSettings),
                    CookieOptions = new CookieOptions
                    {
                        SlidingExpiration = true,
                        ExpireTimeSpan = TimeSpan.FromDays(7),
                        SecureMode = CookieSecureMode.SameAsRequest,
                        IsPersistent = true
                    }
                }
            };

            app.UseIdentityServer(Options);

            container.RegisterInstance<IUserTokenProvider<ApplicationUser, string>>(tokenProvider);
        }

        public static X509Certificate2 FindCertificate(string certificateThumbprint)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.OfType<X509Certificate2>()
                .Single(x => x.Thumbprint == certificateThumbprint);
            store.Close();
            return cert;
        }

        public static void ConfigureSocialIdentityProviders(IAppBuilder app, string signInAsType, AppSettings appSettings)
        {
            var google = new GoogleOAuth2AuthenticationOptions
            {
                AuthenticationType = Enum.GetName(typeof(RegistrationType), RegistrationType.Google),
                SignInAsAuthenticationType = signInAsType,
                ClientId = appSettings.GetString("IdSrv", "GoogleAppId"),
                ClientSecret = appSettings.GetString("IdSrv", "GoogleAppSecret")
            };
            google.Scope.Add("email");
            app.UseGoogleAuthentication(google);

            //    var fb = new FacebookAuthenticationOptions
            //    {
            //        AuthenticationType = Enum.GetName(typeof(RegistrationType), RegistrationType.Facebook),
            //        SignInAsAuthenticationType = signInAsType,
            //        AppId = appSettings.facebookAppID.ToString(),
            //        AppSecret = appSettings.facebookAppSecret
            //    };
            //    fb.Scope.Add("email");
            //    app.UseFacebookAuthentication(fb);

            //    var twitter = new TwitterAuthenticationOptions
            //    {
            //        AuthenticationType = Enum.GetName(typeof(RegistrationType), RegistrationType.Twitter),
            //        SignInAsAuthenticationType = signInAsType,
            //        ConsumerKey = appSettings.twitterConsumerKey,
            //        ConsumerSecret = appSettings.twitterConsumerSecret
            //    };
            //    app.UseTwitterAuthentication(twitter);
        }
    }
}