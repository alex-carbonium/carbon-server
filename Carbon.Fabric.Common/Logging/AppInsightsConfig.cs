using Carbon.Business;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Fabric.Common.Logging
{
    public static class AppInsightsConfig
    {
        private static readonly TimeSpan PerfCounterInterval = TimeSpan.FromHours(2);

        public static void Configure(string instrumentationKey)
        {
            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new TelemetryInitializer());

            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new BufferingPerfCounterProcessor(next, PerfCounterInterval));
            TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
        }
    }

    public class TelemetryInitializer : ITelemetryInitializer
    {
        private string _env;

        public TelemetryInitializer()
        {
            _env = Environment.GetEnvironmentVariable("Carbon_Env");
            if (string.IsNullOrEmpty(_env))
            {
                _env = "local";
            }
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Component.Version = Defs.Config.VERSION.ToString();
            telemetry.Context.Cloud.RoleInstance = Environment.MachineName;
            telemetry.Context.Cloud.RoleName = _env;
        }
    }

    public class BufferingPerfCounterProcessor : ITelemetryProcessor
    {
        private readonly ITelemetryProcessor _next;
        private readonly ConcurrentDictionary<string, MetricBuffer> _counterTimes = new ConcurrentDictionary<string, MetricBuffer>();
        private readonly TimeSpan _interval;

        public BufferingPerfCounterProcessor(ITelemetryProcessor next, TimeSpan interval)
        {
            _next = next;
            _interval = interval;
        }

        public void Process(ITelemetry item)
        {
            var telemetry = item as MetricTelemetry;
            string isCounter;
            if (telemetry != null
                && telemetry.Properties.TryGetValue("CustomPerfCounter", out isCounter)
                && isCounter == "true")
            {
                MetricBuffer lastBuffer;
                if (!_counterTimes.TryGetValue(telemetry.Name, out lastBuffer))
                {
                    lastBuffer = new MetricBuffer();
                    _counterTimes.TryAdd(telemetry.Name, lastBuffer);
                }
                lastBuffer.Metrics.Add(telemetry);

                if (lastBuffer.IntervalStart.Add(_interval) > DateTime.UtcNow)
                {
                    return;
                }

                lastBuffer.Aggregate(telemetry);
                lastBuffer.Clear();
            }
            _next.Process(item);
        }

        private class MetricBuffer
        {
            public MetricBuffer()
            {
                Metrics = new List<MetricTelemetry>();
                Clear();
            }

            public void Clear()
            {
                Metrics.Clear();
                IntervalStart = DateTime.UtcNow;
            }

            public void Aggregate(MetricTelemetry target)
            {
                target.Count = Metrics.Sum(x => x.Count) ?? 0;
                target.Min = Metrics.Max(x => x.Max) ?? 0;
                target.Max = Metrics.Min(x => x.Max) ?? 0;
                target.Sum = Metrics.Sum(x => x.Sum);
                target.StandardDeviation = Math.Sqrt(Metrics.Average(x => x.StandardDeviation * x.StandardDeviation) ?? 0);
            }

            public DateTime IntervalStart { get; set; }
            public List<MetricTelemetry> Metrics { get; }
        }
    }
}
