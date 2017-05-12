using Carbon.Business;
using IdentityModel.Client;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Carbon.Tools.OAuth
{
    public class CarbonClient
    {
        private readonly TokenCache _cache;
        private readonly string _host;

        public CarbonClient(string host)
        {
            _cache = new TokenCache();
            _host = host;
        }

        public async Task<HttpResponseMessage> GetProjectLogAsync(string companyId, string modelId)
        {
            return await GetAsync($"/api/admin/projectLog?companyId={companyId}&modelId={modelId}");
        }

        private async Task<HttpResponseMessage> GetAsync(string uri)
        {
            return await SendAsync(() => new HttpRequestMessage(HttpMethod.Get, new Uri(uri, UriKind.Relative)));
        }

        private async Task<HttpResponseMessage> PostAsync(string uri, dynamic data)
        {
            return await SendAsync(() =>
            {
                var message = new HttpRequestMessage(HttpMethod.Post, new Uri(uri, UriKind.Relative));
                message.Content = new StringContent(JsonConvert.SerializeObject(data));
                return message;
            });
        }

        private async Task<HttpResponseMessage> SendAsync(Func<HttpRequestMessage> messageFunc)
        {
            var client = GetAuthorizedClient();

            var response = await client.SendAsync(messageFunc());
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _cache.Clear();

                client = GetAuthorizedClient();
                response = await client.SendAsync(messageFunc());
            }

            response.EnsureSuccessStatusCode();

            return response;
        }

        private HttpClient GetAuthorizedClient()
        {
            var token = GetAccessToken();

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_host);
            httpClient.SetBearerToken(token);

            return httpClient;
        }

        private string GetAccessToken()
        {
            var token = _cache.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                do
                {
                    System.Console.WriteLine("Your email:");
                    var email = System.Console.ReadLine();

                    System.Console.WriteLine("Your password:");
                    var password = ConsoleExtensions.ReadPassword();

                    var tokenClient = new TokenClient(
                        new Uri(new Uri(_host), "/idsrv/connect/token").AbsoluteUri,
                        Defs.IdentityServer.AuthClient,
                        Defs.IdentityServer.PublicSecret);

                    var response = tokenClient.RequestResourceOwnerPasswordAsync(email, password, "account").Result;
                    if (response.IsError)
                    {
                        System.Console.WriteLine("Could not obtain access token: " + response.Error);
                    }
                    else
                    {
                        token = response.AccessToken;
                    }
                } while (string.IsNullOrEmpty(token));

                _cache.SaveToken(token);
            }

            return token;
        }
    }
}
