using Carbon.Business.CloudDomain;
using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Tools.LogAnalysis
{
    public class LogStatistics
    {
        [Verb("logStats")]
        public class Options
        {
            [Option('c', "companyId", Required = true)]
            public string CompanyId { get; set; }

            [Option('m', "modelId", Required = true)]
            public string ModelId { get; set; }

            [Option('t', "targetFolder", Default = ".")]
            public string TargetFolder { get; set; }
        }

        public async Task Generate(Options options)
        {
            var dir = LogDownloader.GetModelDir(options.TargetFolder, options.CompanyId, options.ModelId);
            if (!Directory.Exists(dir))
            {
                System.Console.WriteLine("Could not find model directory " + dir);
                return;
            }

            var logs = Directory.GetFiles(dir, "P_*.json")
                .Select(x =>
                {
                    var json = JObject.Parse(System.IO.File.ReadAllText(x));
                    var log = new ProjectLog();
                    log.PartitionKey = json.Value<string>("PartitionKey");
                    log.RowKey = json.Value<string>("RowKey");
                    log.FromVersion = json.Value<string>("FromVersion");
                    log.ToVersion = json.Value<string>("ToVersion");
                    log.UserId = json.Value<string>("UserId");
                    log.Primitives = new List<string> { json.Value<JArray>("Primitives").First().ToString() };
                    return log;
                });

            var buckets = new Dictionary<DateTime, int>();
            var baseDate = new DateTime(1970, 1, 1);
            foreach (var log in logs)
            {
                var primitive = JObject.Parse(log.Primitives.First());
                var time = primitive.Value<long>("time");
                var dateTime = baseDate + new TimeSpan(time * 10000);
                var bucket = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);

                int times;
                buckets.TryGetValue(bucket, out times);
                ++times;
                buckets[bucket] = times;
            }

            var savesPerHour = 400d;
            var hours = 0d;
            var sorted = buckets.OrderBy(x => x.Key);
            var stats = new StringBuilder();
            foreach (var pair in sorted)
            {
                stats.Append(pair.Key.ToString("yyyy-MM-dd HH:mm:ss"));
                stats.Append(",");
                stats.Append(pair.Value);
                stats.Append(Environment.NewLine);

                hours += Math.Min(pair.Value / savesPerHour, 1);
            }

            System.IO.File.WriteAllText(Path.Combine(dir, "Stats.csv"), stats.ToString());

            var summary = new StringBuilder();
            summary.Append("Total hours,");
            summary.Append(hours);
            stats.Append(Environment.NewLine);
            System.IO.File.WriteAllText(Path.Combine(dir, "StatsSummary.csv"), summary.ToString());
        }
    }
}
