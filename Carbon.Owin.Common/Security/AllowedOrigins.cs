using System.Collections.Generic;
using System.Net;

namespace Carbon.Owin.Common.Security
{
    public static class AllowedOrigins
    {
        public static readonly IEnumerable<string> All;
        public static readonly string AllAsList;

        public static IEnumerable<string> GetIPAddress()
        {
            IPHostEntry ipHostInfo = null;

            try
            {
                ipHostInfo = Dns.GetHostEntry("carbonium.local"); // `Dns.Resolve()` method is deprecated.
            }
            catch
            {

            }
            if (ipHostInfo != null)
            {
                foreach (var ipAddress in ipHostInfo.AddressList)
                {
                    yield return ipAddress.ToString();
                }
            }


            yield return "localhost";            
        }

        static AllowedOrigins()
        {
            var allowedDomains = new List<string>
            {
                "carbon-qa.westeurope.cloudapp.azure.com",
                "carbon-prod.westeurope.cloudapp.azure.com",
                "dev.carbonium.io",
                "qa.carbonium.io",
                "prod.carbonium.io",
                "carbonium.io",
                "carbonium.local:8080",
                "carbonium.local:9000",
                "carbonium.local:9100"
            };

#if DEBUG
            foreach (var ip in GetIPAddress())
            {
                allowedDomains.Add(ip + ":8010");
                allowedDomains.Add(ip + ":8080");
                allowedDomains.Add(ip + ":9000");
                allowedDomains.Add(ip + ":9100");
                allowedDomains.Add(ip + ":9010");
            }
#endif

            var all = new List<string>();
            foreach (var domain in allowedDomains)
            {
                all.Add("http://" + domain);
                all.Add("https://" + domain);
            }
            All = all;
            AllAsList = string.Join(",", all.ToArray());
        }
    }
}
