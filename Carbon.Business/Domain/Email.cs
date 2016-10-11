using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;

namespace Carbon.Business.Domain
{
    public class Email
    {
        public Email()
        {
        }
        public Email(string to, string templateName)
            : this()
        {
            To = to;
            TemplateName = templateName;
        }

        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string ReplyTo { get; set; }
        public string To { get; set; }        
        public string TemplateName { get; set; }
        public dynamic Model { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public void AddRecepient(string address)
        {
            if (string.IsNullOrEmpty(To))
            {
                To = address;
            }
            else
            {
                To = "," + address;
            }
        }        

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static T FromString<T>(string s) where T : Email
        {
            var email = JsonConvert.DeserializeObject<T>(s);

            var modelJson = JsonConvert.SerializeObject(email.Model);
            email.Model = JsonConvert.DeserializeObject<ExpandoObject>(modelJson);

            return email;
        }
    }

    public class BulkEmail : Email
    {
        public long CampaignId { get; set; }
        public int MessageCount { get; set; }
        public bool TestMode { get; set; }
    }
}