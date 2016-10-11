using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Business.CloudDomain
{
    public class PublicResource : TableEntity
    {
        public PublicResource()
        {

        }

        public PublicResource(string key, string name, string projectType, string theme, ResourceType resourceType)
        {
            PartitionKey = resourceType.ToString();
            RowKey  = key;
            Name = name;
            ResourceType = resourceType;
            ProjectType = projectType;
            Theme = theme;
        }
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string ProjectType { get; set; }
        public string Theme { get; set; }
        public DateTime PublishingDate { get; set; }
        public ResourceType ResourceType { get; set; }
        public double AverageRating { get; set; }
        public double RatingCount { get; set; }

        public void Rate(int rating)
        {
            if (rating < 1)
                rating = 1;
            else if (rating > 5)
                rating = 5;

            AverageRating = (AverageRating * RatingCount + rating) / ++RatingCount;
        }
    }
}
