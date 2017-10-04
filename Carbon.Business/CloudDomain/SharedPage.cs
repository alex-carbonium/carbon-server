using System;
using Microsoft.WindowsAzure.Storage.Table;
using Carbon.Framework.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Carbon.Business.CloudDomain
{
    public class SharedPage : TableEntity
    {
        public string Tags { get; set; }
        public string Name { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        public string Description { get; set; }
        public string CoverUrl { get; set; }
        public string ScreenshotList { get; set; }
        public string DataUrl { get; set; }
        public int TimesUsed { get; set; }
        public int Scope { get; set; }
        public DateTime Created { get; set; }

        [IgnoreProperty]
        public string GalleryId
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        [IgnoreProperty]
        public IEnumerable<string> Screenshots
        {
            get { return ScreenshotList?.Split(',') ?? Enumerable.Empty<string>(); }
            set { ScreenshotList = string.Join(",", value); }
        }

        public static string PageNameToId(string name)
        {
            return ((uint)name.ToLowerInvariant().GetHashCode()).ToString();
        }
    }

    public interface IPrivateSharedPageRepository : IRepository<SharedPage>
    {
    }

    public interface IPublicSharedPageRepository : IRepository<SharedPage>
    {
    }
}
