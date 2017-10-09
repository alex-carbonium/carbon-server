using System;
using Microsoft.WindowsAzure.Storage.Table;
using Carbon.Framework.Repositories;
using System.Collections.Generic;

namespace Carbon.Business.CloudDomain
{
    public class SharedPage : TableEntity
    {
        private static readonly string[] EmptyList = new string[0];
        private const char ListSeparatorChar = '\n';
        private const string ListSeparator = "\n";

        public string Tags { get; set; }
        public string Name { get; set; }
        public string AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAvatar { get; set; }
        public string Description { get; set; }
        public string CoverUrl { get; set; }
        public string ScreenshotUrlList { get; set; }
        public string ScreenshotIdList { get; set; }
        public string ScreenshotNameList { get; set; }
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
        public IReadOnlyList<string> ScreenshotsUrls
        {
            get { return ScreenshotUrlList?.Split(ListSeparatorChar) ?? EmptyList; }
            set { ScreenshotUrlList = string.Join(ListSeparator, value); }
        }

        [IgnoreProperty]
        public IReadOnlyList<string> ScreenshotIds
        {
            get { return ScreenshotIdList?.Split(ListSeparatorChar) ?? EmptyList; }
            set { ScreenshotIdList = string.Join(ListSeparator, value); }
        }

        [IgnoreProperty]
        public IReadOnlyList<string> ScreenshotNames
        {
            get { return ScreenshotNameList?.Split(ListSeparatorChar) ?? EmptyList; }
            set { ScreenshotNameList = string.Join(ListSeparator, value); }
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
