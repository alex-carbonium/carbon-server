using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business;
using Carbon.Framework.Extensions;
using Carbon.Owin.Common.Security;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace Carbon.Services.IdentityServer
{
    public class IdentityClientStore : InMemoryClientStore
    {
        private static readonly TimeSpan AccessTokenLifeTime = TimeSpan.FromHours(2);

        public static List<Client> All()
        {
            var clients = new List<Client>
            {
                new Client
                {
                    ClientName = "Access token renewal client",
                    Enabled = true,
                    ClientId = "renew",
                    Flow = Flows.Implicit,
                    RequireConsent = false,
                    AllowAccessToAllScopes = true,

                    AccessTokenLifetime = (int) AccessTokenLifeTime.TotalSeconds
                },

                new Client
                {
                    Enabled = true,
                    ClientName = "Authentication client",
                    ClientId = "auth",
                    Flow = Flows.ResourceOwner,
                    ClientSecrets = new List<Secret>
                    {
                        new Secret(Defs.IdentityServer.PublicSecret.Sha512())
                    },
                    RequireConsent = false,
                    AllowAccessToAllScopes = true,

                    AccessTokenLifetime = (int) AccessTokenLifeTime.TotalSeconds
               }
            };

            const string redirectPath = "/a/renew";
            foreach (var client in clients.Where(client => client.Flow == Flows.Implicit))
            {
                client.RedirectUris = new List<string>();
                foreach (var origin in AllowedOrigins.All)
                {
                    client.RedirectUris.Add(new Uri(origin).AddPath(redirectPath).ToString());
                }
            }

            return clients;
        }

        public IdentityClientStore() : base(All())
        {
        }
    }
}