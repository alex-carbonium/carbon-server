using CommandLine;
using Carbon.Tools.LogAnalysis;

namespace Carbon.Tools
{
    class ToolsProgram
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<LogDownloader.Options, LogPlayer.Options>(args)
                .WithParsed<LogDownloader.Options>(opts => new LogDownloader().DownloadProjectLog(opts).Wait())
                .WithParsed<LogPlayer.Options>(opts => new LogPlayer().ReplayProjectLog(opts).Wait());
        }



    }
}
