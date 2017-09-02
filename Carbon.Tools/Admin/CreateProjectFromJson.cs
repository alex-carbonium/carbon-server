using Carbon.Tools.OAuth;
using CommandLine;
using System.Threading.Tasks;

namespace Carbon.Tools.Admin
{
    public class CreateProjectFromJson
    {
        [Verb("createProjectFromJson")]
        public class Options : BaseOptions
        {
            [Option('f', "file")]
            public string File { get; set; }
        }

        public async Task Run(Options options)
        {
            var client = new CarbonClient(options.Host);
            using (var response = await client.CreateProjectFromJsonAsync(options.File))
            {
                System.Console.WriteLine(response.StatusCode);
            }
        }
    }
}
