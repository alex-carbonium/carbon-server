using System.Collections.Generic;
using System.Security;
using Carbon.Business;

namespace Carbon.Test.Common
{
    public class ConfigurationStub : Configuration
    {
        public Dictionary<string, Dictionary<string, dynamic>> Data { get; }

        public ConfigurationStub()
        {
            Data = new Dictionary<string, Dictionary<string, dynamic>>();
            Add("ConnectionStrings", "nosql", "UseDevelopmentStorage=true");
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
