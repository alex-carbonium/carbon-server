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

            Add("IdSrv", "ProtectionCertificateThumbprint", "9D08EFD8B5B9ED86CE7E9AAB12B0E4C3FA11AF4F");
            Add("IdSrv", "PrivateKeyReleaseThumbprint", string.Empty);
            Add("IdSrv", "PrivateKeyFile", "idsrv-debug.pfx");
            Add("IdSrv", "PublicKeyFile", "idsrv-debug.cer");
            Add("IdSrv", "PrivateKeyDebugPassword", "PandaIsSafe");
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
