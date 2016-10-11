using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Carbon.Framework.Logging
{
    public class TelemetryLogger : Logger
    {
        private readonly Lazy<TelemetryClient> _client = new Lazy<TelemetryClient>(CrateClient);

        public TelemetryLogger(string name)
        {
            Name = name;            
        }

        private static TelemetryClient CrateClient()
        {            
            return new TelemetryClient(TelemetryConfiguration.Active);            
        }

        public override void Debug(string message, params object[] parameters)
        {
#if DEBUG
            Log(TraceEventType.Verbose, message, parameters);
#endif
        }

        public override void Info(string message, params object[] parameters)
        {
            Log(TraceEventType.Information, message, parameters);
        }

        public override void Info(string message, IDictionary<string, string> context)
        {
            Log(TraceEventType.Information, message, context: context);
        }

        public override void Warning(string message, params object[] parameters)
        {
            Log(TraceEventType.Warning, message, parameters);
        }
        public override void Warning(string message, IDictionary<string, string> context)
        {
            Log(TraceEventType.Warning, message, context: context);
        }

        public override void Error(string message, params object[] parameters)
        {
            Log(TraceEventType.Error, message, parameters);
        }

        public override void Error(string message, IDictionary<string, string> context)
        {
            Log(TraceEventType.Error, message, context: context);    
        }

        public override void Error(string message, Exception ex, IDictionary<string, string> context)
        {
            Log(TraceEventType.Error, message, exception: ex, context: context);    
        }

        public override void Error(Exception ex, IDictionary<string, string> context)
        {
            Log(TraceEventType.Error, ex.Message, exception: ex, context: context);
        }

        public override void Fatal(Exception ex)
        {
            Log(TraceEventType.Critical, ex.Message, exception: ex);
        }

        public override void Fatal(string message)
        {
            Log(TraceEventType.Critical, message);
        }

        public override void Fatal(string message, params object[] parameters)
        {
            Log(TraceEventType.Critical, message, parameters);
        }

        public override void Fatal(string message, IDictionary<string, string> context)
        {
            Log(TraceEventType.Critical, message, context: context);    
        }

        public override void Trace(string message)
        {
            Log(TraceEventType.Verbose, message);
        }

        public override void Trace(string message, params object[] parameters)
        {
            Log(TraceEventType.Verbose, message, parameters);
        }

        public override void Trace(string message, IDictionary<string, string> context, params object[] parameters)
        {
            Log(TraceEventType.Verbose, message, parameters, context: context);
        }

        private void Log(TraceEventType level, string message, object[] parameters = null, Exception exception = null, IDictionary<string, string> context = null)
        {
            if (level == TraceEventType.Warning || level == TraceEventType.Error || level == TraceEventType.Critical)
            {
                System.Diagnostics.Debug.WriteLine(message != null ? string.Format(message, parameters ?? new object[0]) : exception?.ToString());
            }

            if (context != null)
            {
                string userId;
                if (context.TryGetValue("userId", out userId))
                {
                    _client.Value.Context.User.Id = userId;
                    context.Remove("userId");
                }
                context["logLevel"] = level.ToString();
            }
            if (exception != null)
            {                
                _client.Value.TrackException(exception, context);                
            }
            else
            {
                var formattedMessage = parameters != null && parameters.Length > 0 ? string.Format(message, parameters) : message;
                _client.Value.TrackTrace(formattedMessage, context);
            }            
        }

        public override void Event(string name, IDictionary<string, string> properties)
        {                    
            _client.Value.TrackEvent(name, properties);
        }

        public override string Name { get; }
    }
}