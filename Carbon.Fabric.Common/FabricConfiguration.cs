using System.Fabric;
using System.Security;
using Carbon.Business;
using Carbon.Framework.Extensions;

namespace Carbon.Fabric.Common
{
    public class FabricConfiguration : Configuration
    {
        private readonly ConfigurationPackage _package;

        public FabricConfiguration(ServiceContext context)
        {
            _package = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
        }

        public override string GetString(string section, string parameter)
        {
            var setting = _package.Settings.Sections[section].Parameters[parameter];
            return setting.IsEncrypted ? setting.DecryptValue().ToPlainText() : setting.Value;
        }

        public override SecureString GetSecureString(string section, string parameter)
        {
            return _package.Settings.Sections[section].Parameters[parameter].DecryptValue();
        }

        public override int GetInt(string section, string parameter)
        {
            return int.Parse(GetString(section, parameter));
        }

        public override bool GetBoolean(string section, string parameter)
        {
            return bool.Parse(GetString(section, parameter));
        }
    }
}
