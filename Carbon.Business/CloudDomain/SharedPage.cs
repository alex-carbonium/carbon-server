using System;
using Microsoft.WindowsAzure.Storage.Table;
using Carbon.Framework.Repositories;

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
        public string DataUrl { get; set; }
        public int TimesUsed { get; set; }
        public int TimesView { get; set; }
        public int Scope { get; set; }
        public DateTime Created { get; set; }

        [IgnoreProperty]
        public string GalleryId
        {
            get { return RowKey; }
            set { RowKey = value; }
        }

        public static string PageNameToId(string name)
        {
            return name.ToLowerInvariant().GetHashCode().ToString();
        }
    }

    public interface IPrivateSharedPageRepository : IRepository<SharedPage>
    {
    }

    public interface IPublicSharedPageRepository : IRepository<SharedPage>
    {
    }
}
