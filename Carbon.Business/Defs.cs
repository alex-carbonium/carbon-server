using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Carbon.Business
{
    public class Defs
    {
        public static class Roles
        {
            public const string Administrators = "Administrators";
            public const string Guest = "Guest";
        }

        public static class Config
        {
            public const string QA = "QA";
            public static readonly Version VERSION = typeof(Defs).Assembly.GetName().Version;
            public static readonly JsonSerializerSettings JsonSerializerSettings;

            static Config()
            {
                JsonSerializerSettings = new JsonSerializerSettings();
                JsonSerializerSettings.Converters.Add(new IsoDateTimeConverter());
            }
        }

        public static class Azure
        {
            public const int StoragePortStart = 9100;

#if DEBUG
            public const int TableBatchSize = 1;
            public const int DocumentDbBatchSize = 5;
#else
            public const int TableBatchSize = 100;
            public const int DocumentDbBatchSize = 100;
#endif
        }

        public static class IdentityServer
        {
            public static string PublicSecret = "nopassword";

            public const string AccessTokenKey = "access_token";
            public static readonly Dictionary<string, string> QueryFilter = new Dictionary<string, string> { { AccessTokenKey, "xxx" } };
        }

        public static class Packages
        {
            public const string Client = "Client";
            public const string Data = "Data";
        }
    }
}
