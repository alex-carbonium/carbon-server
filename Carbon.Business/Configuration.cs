using System.Security;

namespace Carbon.Business
{
    public abstract class Configuration
    {
        public abstract string GetString(string section, string parameter);
        public abstract SecureString GetSecureString(string section, string parameter);
        public abstract int GetInt(string section, string parameter);
        public abstract bool GetBoolean(string section, string parameter);       
    }
}
