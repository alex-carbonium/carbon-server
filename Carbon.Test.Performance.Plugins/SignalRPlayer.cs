using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Performance.Plugins
{
    public class SignalRPlayer
    {
        public void PlayRequests(string url, string recordingDirectory, Func<string, JArray, JArray> argumentsTransformer = null)
        {
            using (var hubConnection = new HubConnection(url))
            {
                hubConnection.CookieContainer = CookieContainer;

                var files = Directory.GetFiles(recordingDirectory)
                    .OrderBy(x => x, new FileNameComparer());
                
                var proxies = new Dictionary<string, IHubProxy>();
                var actions = new List<dynamic>(); 
                foreach (var file in files)
                {
                    var contents = File.ReadAllText(file);
                    var json = JObject.Parse(contents);
                    var hubName = json["data"].Value<string>("H");

                    IHubProxy proxy;
                    if (!proxies.TryGetValue(hubName, out proxy))
                    {
                        proxy = hubConnection.CreateHubProxy(hubName);
                        proxies[hubName] = proxy;
                    }

                    dynamic action = new ExpandoObject();
                    action.Proxy = proxy;
                    action.Method = json["data"].Value<string>("M");
                    var arguments = json["data"].Value<JArray>("A");
                    if (argumentsTransformer != null)
                    {
                        arguments = argumentsTransformer(action.Method, arguments);
                    }
                    action.Arguments = arguments.OfType<object>().ToArray();
                    actions.Add(action);
                }

                hubConnection.Start().Wait();

                foreach (var action in actions)
                {
                    action.Proxy.Invoke(action.Method, action.Arguments).Wait();
                }
            }            
        }

        public CookieContainer CookieContainer { get; set; }

        private class FileNameComparer : Comparer<string>
        {
            public override int Compare(string x, string y)
            {
                var value1 = int.Parse(Path.GetFileNameWithoutExtension(x));
                var value2 = int.Parse(Path.GetFileNameWithoutExtension(y));
                return value1.CompareTo(value2);
            }
        }
    }
}