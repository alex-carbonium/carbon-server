using System;
using Microsoft.VisualStudio.TestTools.LoadTesting;

namespace Carbon.Test.Performance.Plugins
{
    public class LoadTestConfig : ILoadTestPlugin
    {
        public void Initialize(LoadTest test)
        {
            if (test.RunSettings.Name.Equals("Local", StringComparison.OrdinalIgnoreCase))
            {
                test.Context["TargetServer"] = "http://localhost:8010";
                test.Context["CDNServer"] = "http://localhost:8010";
            }
            else
            {
                test.Context["TargetServer"] = "http://qa.carbonium.io";
                test.Context["CDNServer"] = "http://az291014.vo.msecnd.net/cdn/1.4.169.0";
            }
        }
    }
}
