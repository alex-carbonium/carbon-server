using Carbon.Business;
using System.Collections.Generic;
using System.Security;

namespace Carbon.Console
{
    public class InMemoryConfiguration : Configuration
    {
        public Dictionary<string, Dictionary<string, dynamic>> Data { get; }

        public InMemoryConfiguration()
        {
            Data = new Dictionary<string, Dictionary<string, dynamic>>();
            Add("ConnectionStrings", "nosql", "UseDevelopmentStorage=true");
            Add("ConnectionStrings", "jobs", "UseDevelopmentStorage=true");
            Add("ConnectionStrings", "cdn", "UseDevelopmentStorage=true");

            Add("IdSrv", "ProtectionCertificateThumbprint", "9D08EFD8B5B9ED86CE7E9AAB12B0E4C3FA11AF4F");
            Add("IdSrv", "PrivateKeyReleaseThumbprint", string.Empty);
            Add("IdSrv", "PrivateKeyFile", "idsrv-debug.pfx");
            Add("IdSrv", "PublicKeyFile", "idsrv-debug.cer");
            Add("IdSrv", "PrivateKeyDebugPassword", "PandaIsSafe");
            Add("IdSrv", "GoogleAppId", "766646497455-56q8aqr57clr1trokg9jdsjvtp21snd2.apps.googleusercontent.com");
            Add("IdSrv", "GoogleAppSecret", "Dxa229t8IE7gkbexoUGTgEIy");
            Add("IdSrv", "FacebookAppId", "661220964075977");
            Add("IdSrv", "FacebookAppSecret", "d59746e4dd040775d4c93e651481862c");
            Add("IdSrv", "TwitterAppId", "GNOKs0T9fbaL9rJQvYKAlIVfG");
            Add("IdSrv", "TwitterAppSecret", "E01mTEVPzZkxc92km8akyRZ4B531UB9kAH9hGOGUwhG88o23Pk");
            Add("IdSrv", "MicrosoftAppId", "2c4a8f6b-0cde-4134-aaa5-dc6552704576");
            Add("IdSrv", "MicrosoftAppSecret", "9XRPhRumaFC7POzD6e7j2m8");

            Add("Azure", "TelemetryKey", ""/*"6ad84643-9530-4367-a698-6dde4567f0cd"*/);
            Add("Endpoints", "Storage", "http://localhost:9100");
            Add("Endpoints", "File", "http://127.0.0.1:10000/devstoreaccount1");
            Add("Endpoints", "Cdn", "http://127.0.0.1:10000/devstoreaccount1");

            Add("General", "SiteHost", "http://localhost:8080");
        }

        public void Add(string section, string parameter, dynamic value)
        {
            Dictionary<string, dynamic> values;
            if (!Data.TryGetValue(section, out values))
            {
                values = new Dictionary<string, dynamic>();
                Data.Add(section, values);
            }
            values[parameter] = value;
        }

        public override string GetString(string section, string parameter)
        {
            return Data[section][parameter];
        }

        public override SecureString GetSecureString(string section, string parameter)
        {
            return Data[section][parameter];
        }

        public override int GetInt(string section, string parameter)
        {
            return Data[section][parameter];
        }

        public override bool GetBoolean(string section, string parameter)
        {
            return Data[section][parameter];
        }
    }
}
