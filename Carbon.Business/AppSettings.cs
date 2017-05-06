using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Web.Hosting;

namespace Carbon.Business
{
    public class AppSettings
    {
        private readonly Configuration _configuration;
        private readonly DataProvider _dataProvider;

        public AppSettings(Configuration configuration, DataProvider dataProvider)
        {
            _configuration = configuration;
            _dataProvider = dataProvider;
            Subscription = new SubscriptionConfiguration(configuration, "Subscription");
            Survey = new SurveyConfiguration(configuration, "Survey");
            IdServer = new IdServerConfig(configuration, "IdSrv");
            IdClient = new IdClientConfig(configuration, "IdSrv");
            Azure = new AzureConfig(configuration, "Azure");
        }

        public string GetString(string section, string parameter)
        {
            return _configuration.GetString(section, parameter);
        }

        public string GetDataPackageVersion(string name)
        {
            return _dataProvider.GetPackageVersion(name);
        }

        public virtual SecureString TestSecret => _configuration.GetSecureString("General", "TestSecret");
        public virtual string SiteHost => _configuration.GetString("General", "SiteHost");

        public virtual SubscriptionConfiguration Subscription { get; }
        public virtual SurveyConfiguration Survey { get; }
        public virtual IdServerConfig IdServer { get; }
        public virtual IdClientConfig IdClient { get; }
        public virtual AzureConfig Azure { get; }

        public string ResolvePath(string packageName, string path)
        {
            return _dataProvider.ResolvePath(packageName, path);
        }

        public class SubscriptionConfiguration
        {
            private readonly Configuration _configuration;
            private readonly string _section;

            public SubscriptionConfiguration()
            {
            }
            public SubscriptionConfiguration(Configuration configuration, string section)
            {
                _configuration = configuration;
                _section = section;
            }

            public virtual int DiscountMonths => _configuration.GetInt(_section, "DiscountMonths");
            public virtual int DiscountedPricePro => _configuration.GetInt(_section, "DiscountedPricePro");
            public virtual int TrialDays => _configuration.GetInt(_section, "TrialDays");
            public virtual int PricePro => _configuration.GetInt(_section, "PricePro");
            public virtual int PriceProPlus => _configuration.GetInt(_section, "PriceProPlus");
            public virtual bool DiscountActive => _configuration.GetBoolean(_section, "DiscountActive");
        }

        public class SurveyConfiguration
        {
            private readonly Configuration _configuration;
            private readonly string _section;

            public SurveyConfiguration(Configuration configuration, string section)
            {
                _configuration = configuration;
                _section = section;
            }

            public string Id => _configuration.GetString(_section, "Id");
        }

        public class IdServerConfig
        {
            private readonly Configuration _configuration;
            private readonly string _section;

            public IdServerConfig(Configuration configuration, string section)
            {
                _configuration = configuration;
                _section = section;
            }

            public string PrivateKeyFile => _configuration.GetString(_section, "PrivateKeyFile");
            public string PrivateKeyDebugPassword => _configuration.GetString(_section, "PrivateKeyDebugPassword");
            public string PrivateKeyReleaseThumbprint => _configuration.GetString(_section, "PrivateKeyReleaseThumbprint");
            public string ProtectionCertificateThumbprint => _configuration.GetString(_section, "ProtectionCertificateThumbprint");
        }

        public class AzureConfig
        {
            private readonly Configuration _configuration;
            private readonly string _section;

            public AzureConfig(Configuration configuration, string section)
            {
                _configuration = configuration;
                _section = section;
            }

            public string TelemetryKey => _configuration.GetString(_section, "TelemetryKey");
        }

        public class IdClientConfig
        {
            private readonly Configuration _configuration;
            private readonly string _section;

            public IdClientConfig(Configuration configuration, string section)
            {
                _configuration = configuration;
                _section = section;
            }

            public string PublicKeyFile => _configuration.GetString(_section, "PublicKeyFile");
        }

        public virtual string GetConnectionString(string name)
        {
            return _configuration.GetString("ConnectionStrings", name);
        }

        public virtual string GetPhysicalPath(string virtualOrPhysicalPath)
        {
            if (virtualOrPhysicalPath[0] == '/' || virtualOrPhysicalPath[0] == '~')
            {
                return HostingEnvironment.MapPath(virtualOrPhysicalPath);
            }
            return virtualOrPhysicalPath;
        }

        public virtual IEnumerable<string> GetDirectoryFiles(string dir, bool relativeFileNames = false, bool webSlashes = false)
        {
            var files = new List<string>();
            var physicalDir = GetPhysicalPath(dir);
            foreach (var file in Directory.GetFiles(physicalDir, "*.*", SearchOption.AllDirectories))
            {
                var name = file;
                if (relativeFileNames)
                {
                    name = name.Replace(physicalDir + "\\", string.Empty);
                }
                if (webSlashes)
                {
                    name = name.Replace("\\", "/");
                }
                files.Add(name);
            }
            return files;
        }
    }
}
