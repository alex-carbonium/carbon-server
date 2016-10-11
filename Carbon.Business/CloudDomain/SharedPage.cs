using System;
using Carbon.Business.Domain;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudDomain
{
    public class SharedPage : TableEntity
    {       
        public string Tags { get; set; }
        public string Name { get; set; }
        public string CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string DataUrl { get; set; }
        public int TimesUsed { get; set; }
        public int TimesView { get; set; }
        public DateTime Created { get; set; }
    }
}
