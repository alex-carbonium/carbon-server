using CommandLine;
using Carbon.Tools.LogAnalysis;
using Carbon.Tools.Admin;

namespace Carbon.Tools
{
    class ToolsProgram
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<LogDownloader.Options, LogPlayer.Options, CreateProjectFromJson.Options>(args)
                .WithParsed<LogDownloader.Options>(opts => new LogDownloader().DownloadProjectLog(opts).Wait())
                .WithParsed<LogPlayer.Options>(opts => new LogPlayer().ReplayProjectLog(opts).Wait())
                .WithParsed<CreateProjectFromJson.Options>(opts => new CreateProjectFromJson().Run(opts).Wait());
        }
    }
}
