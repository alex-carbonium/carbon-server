using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class ProjectLog : TableEntity, IPipe<string>
    {
        private static readonly Random _keyRandomizer = new Random();

        public ProjectLog()
        {
        }

        public ProjectLog(string companyId, string projectId)
        {
            PartitionKey = GeneratePartitionKey(companyId, projectId);
            RowKey = GenerateKey(DateTimeOffset.UtcNow, makeUnique: true);
        }

        public List<string> Primitives { get; set; }
        public string FromVersion { get; set; }
        public string ToVersion { get; set; }
        public string UserId { get; set; }

        public DateTimeOffset GetDateTime()
        {
            var s = RowKey.Substring(0, RowKey.IndexOf("_"));
            return new DateTimeOffset(long.Parse(s), TimeSpan.Zero);
        }

        public override string ToString()
        {
            return string.Format("{0} -> {1}", FromVersion, ToVersion);
        }

        public static string GeneratePartitionKey(string companyId, string projectId)
        {
            return companyId + "-" + projectId;
        }

        public static string GenerateKey(DateTimeOffset time, bool makeUnique = false)
        {
            return string.Format("{0:D21}_{1:D3}", time.UtcTicks, makeUnique ? _keyRandomizer.Next(0, 999) : 0);
        }

        void IPipe<string>.Read(IEnumerable<string> buffers)
        {
            var i = 0;
            foreach (var buffer in buffers)
            {
                if (i == 0)
                {
                    UserId = buffer;
                }
                else if (i == 1)
                {
                    FromVersion = buffer;
                }
                else if (i == 2)
                {
                    ToVersion = buffer;
                }
                else
                {
                    if (Primitives == null)
                    {
                        Primitives = new List<string>();
                    }
                    Primitives.Add(buffer);
                }
                ++i;
            }
        }

        IEnumerable<string> IPipe<string>.Write()
        {
            yield return UserId;
            yield return FromVersion;
            yield return ToVersion;
            if (Primitives != null)
            {
                foreach (var data in Primitives)
                {
                    yield return data;
                }
            }
        }

        public void GetStatistics(out int count, out int max, out int total)
        {
            count = 3;
            max = 0; //does not matter if there are no primitives
            total = 0;

            if (Primitives != null)
            {
                count += Primitives.Count;
                foreach (var primitive in Primitives)
                {
                    max = Math.Max(max, primitive.Length);
                    total += primitive.Length;
                }
            }
        }
    }
}