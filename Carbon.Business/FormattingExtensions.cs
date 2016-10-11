using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Carbon.Business
{
    public static class FormattingExtensions
    {
        public static string ToDisplayString(this DateTime dateTime)
        {
            return dateTime.ToString("MMMM dd, yyyy");
        }
        public static string ToDisplayCurrencyString(this double amount)
        {
            return string.Format("${0} USD", amount);
        }
        public static string ToDisplayCurrencyString(this int amount)
        {
            return ((double)amount).ToDisplayCurrencyString();
        }
    }

    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostJsonAsync(this HttpClient client, Uri uri, JObject json)
        {
            return client.PostAsync(uri, new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
        }
    }
}

