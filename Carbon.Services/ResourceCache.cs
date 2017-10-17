using System.Collections.Concurrent;
using System.IO;
using Carbon.Business;
using Carbon.Framework.Extensions;

namespace Carbon.Services
{
    public class ResourceCache
    {
        private readonly AppSettings _appSettings;
        private readonly DataProvider _dataProvider;
        private readonly ConcurrentDictionary<string, string> _content = new ConcurrentDictionary<string, string>();

        public ResourceCache(AppSettings appSettings, DataProvider dataProvider)
        {
            _appSettings = appSettings;
            _dataProvider = dataProvider;
        }

        public string GetHtmlFile(string file)
        {
            return _content.GetOrAdd(file, ReadFile);
        }

        public void Clear()
        {
            _content.Clear();
        }

        private string ReadFile(string name)
        {
            var path = _dataProvider.ResolvePath(Defs.Packages.Client, name);

#if DEBUG
            if (!File.Exists(path))
            {
                return "File " + name + " is missing in resource cache";
            }
#endif

            var content = File.ReadAllText(path);

            return content
                .Replace("telemetryKey = ''", "telemetryKey = '" + _appSettings.Azure.TelemetryKey + "'")
                .Replace("appBuild = '1.0.0'", "appBuild = '" + _appSettings.GetDataPackageVersion(Defs.Packages.Client) + "'")
                .Replace("storage: ''", "storage: '" + _appSettings.Endpoints.Storage.WithoutScheme() + "'")
                .Replace("cdn: ''", "cdn: '" + _appSettings.Endpoints.Cdn.WithoutScheme() + "'")
                .Replace("error: ''", "error: '" + _appSettings.Endpoints.Error + "'");
        }
    }
}