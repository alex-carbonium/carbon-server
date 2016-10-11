using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Performance.Plugins
{
    public class RecordPostData : WebTestPlugin
    {
        private string _dir = @"..\..\..\Carbon.Test.Performance\Resources\SignalR\SingleUserRequests";
        private string _urlPattern = "/signalr/send";

        public override void PostRequest(object sender, PostRequestEventArgs e)
        {            
            base.PostRequest(sender, e);
            if (e.Request.Method == "POST"
                && Regex.IsMatch(e.Request.Url, _urlPattern, RegexOptions.IgnoreCase))
            {
                if (!Directory.Exists(_dir))
                {
                    Directory.CreateDirectory(_dir);
                }
                var files = Directory.GetFiles(_dir);
                var max = 1;
                if (files.Length > 0)
                {
                    max = files.Select(x => int.Parse(Path.GetFileNameWithoutExtension(x))).Max() + 1;
                }

                var json = new JObject();
                var body = (FormPostHttpBody)e.Request.Body;
                foreach (var parameter in body.FormPostParameters)
                {
                    json[parameter.Name] = JObject.Parse(parameter.Value);
                }                

                File.WriteAllText(Path.Combine(_dir, max + ".json"), json.ToString(Formatting.Indented), e.Request.Encoding);
            }
        }

        public override void PreWebTest(object sender, PreWebTestEventArgs e)
        {
            base.PreWebTest(sender, e);
            
            if (Directory.Exists(_dir))
            {
                foreach (var file in Directory.GetFiles(_dir, "*.json"))
                {                    
                    File.Delete(file);
                }
            }
        }    
    }
}