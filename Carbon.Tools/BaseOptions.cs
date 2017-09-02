using CommandLine;

namespace Carbon.Tools
{
    public class BaseOptions
    {
        [Option('h', "host", Default = "http://dev.carbonium.io")]
        public string Host { get; set; }
    }
}
