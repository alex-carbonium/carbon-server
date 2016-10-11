using System.Collections.Concurrent;
using System.IO;
using Carbon.Business;

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
            var path = _dataProvider.ResolvePath(@"app\" + name);
            var content = File.ReadAllText(path);

            return content
                .Replace("telemetryKey = ''", "telemetryKey = '" + _appSettings.Azure.TelemetryKey + "'")
                .Replace("appBuild = '1.0.0'", "appBuild = '" + _appSettings.GetDataPackageVersion() + "'")
                .Replace("storage: ''", "storage: '" + _appSettings.GetString("Endpoints", "Storage") + "'")
                .Replace("cdn: ''", "cdn: '" + _appSettings.GetString("Endpoints", "Cdn") + "'");
        }
    }
}