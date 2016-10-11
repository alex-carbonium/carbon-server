using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.WebTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Performance.Plugins
{
    [DisplayName("JSON property extractor")]
    public class JsonExtractionRule : ExtractionRule
    {
        public string Expression { get; set; }

        public override void Extract(object sender, ExtractionEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Response.BodyString))
            {
                e.Success = false;
                e.Message = "Response is empty";
                return;
            }
            var json = e.Response.BodyString;
            var obj = (JObject)JsonConvert.DeserializeObject(json);
            var token = obj.SelectToken(Expression);
            if (token == null)
            {
                e.Success = false;
                e.Message = String.Format("Expression {0} not found in json {1}", Expression, json);
                return;
            }

            e.WebTest.Context.Add(ContextParameterName, token.Value<Object>().ToString());
            e.Success = true;            
       }

    }
}
