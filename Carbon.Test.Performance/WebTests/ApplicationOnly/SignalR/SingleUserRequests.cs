using System.Collections.Generic;
using System.IO;
using Carbon.Test.Performance.Plugins;
using Microsoft.VisualStudio.TestTools.WebTesting;

namespace Carbon.Test.Performance.WebTests.ApplicationOnly.SignalR
{
    public class SingleUserRequests : WebTest
    {
        public override IEnumerator<WebTestRequest> GetRequestEnumerator()
        {
            var player = new SignalRPlayer();
            player.CookieContainer = Context.CookieContainer;
            player.PlayRequests(Context["TargetServer"] + "/signalr",
                Path.Combine(Context["$DeploymentDirectory"].ToString(), @"..\..\..\Sketch.Test.Performance\Resources\SignalR\SingleUserRequests"),
                (m, args) =>
                {
                    if (m == "joinProject" || m == "changeProject")
                    {
                        args[0] = Context["ProjectId"].ToString();
                    }
                    return args;
                });

            return new List<WebTestRequest>().GetEnumerator();
        }
}
}
