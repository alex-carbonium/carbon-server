using Carbon.Tools.OAuth;
using CommandLine;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Carbon.Tools.LogAnalysis
{
    public class LogDownloader
    {
        [Verb("downloadLog")]
        public class Options
        {
            [Option('c', "companyId", Required = true)]
            public string CompanyId { get; set; }

            [Option('m', "modelId", Required = true)]
            public string ModelId { get; set; }

            [Option('h', "host", Default = "http://dev.carbonium.io")]
            public string Host { get; set; }

            [Option('t', "targetFolder", Default = ".")]
            public string TargetFolder { get; set; }
        }

        public async Task DownloadProjectLog(Options options)
        {
            var client = new CarbonClient(options.Host);
            using (var response = await client.GetProjectLogAsync(options.CompanyId, options.ModelId))
            {
                string dir = GetModelDir(options.TargetFolder, options.CompanyId, options.ModelId);
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
                Directory.CreateDirectory(dir);

                var zipFileName = Path.Combine(dir, response.Content.Headers.ContentDisposition.FileName);
                try
                {
                    using (var source = await response.Content.ReadAsStreamAsync())
                    using (var target = System.IO.File.Create(zipFileName))
                    {
                        await source.CopyToAsync(target);
                    }

                    ZipFile.ExtractToDirectory(zipFileName, dir);
                }
                finally
                {
                    if (File.Exists(zipFileName))
                    {
                        File.Delete(zipFileName);
                    }
                }
            }
        }

        public static string GetModelDir(string targetFolder, string companyId, string modelId)
        {
            return Path.Combine(targetFolder, $"{companyId}_{modelId}");
        }
    }
}
