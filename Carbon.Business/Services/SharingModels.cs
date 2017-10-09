using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using System.Collections.Generic;

namespace Carbon.Business.Services
{
    public class PublishPageModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public string PageData { get; set; }
        public string CoverUrl { get; set; }
        public Screenshot[] Screenshots { get; set; }
        public ResourceScope Scope { get; set; }
    }

    public class Screenshot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public static class SharingModelExtensions
    {
        public static object ToApiResult(this SharedPage page)
        {
            var screenshotUrls = page.ScreenshotsUrls;
            var screenshotNames = page.ScreenshotNames;
            var screenshotIds = page.ScreenshotIds;

            var apiScreenshots = new List<Screenshot>();
            for (var i = 0; i < screenshotUrls.Count && i < screenshotNames.Count && i < screenshotIds.Count; ++i)
            {
                apiScreenshots.Add(new Screenshot
                {
                    Id = screenshotIds[i],
                    Name = screenshotNames[i],
                    Url = screenshotUrls[i]
                });
            }

            return new
            {
                page.Name,
                page.Description,
                page.AuthorId,
                page.AuthorAvatar,
                page.AuthorName,
                page.CoverUrl,
                page.DataUrl,
                page.GalleryId,
                page.Tags,
                page.TimesUsed,
                page.Scope,
                Screenshots = apiScreenshots
            };
        }
    }
}
