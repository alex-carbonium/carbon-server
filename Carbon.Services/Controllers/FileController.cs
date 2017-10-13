using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Extensions;
using Carbon.Framework.Repositories;
using Carbon.Framework.Util;
using Carbon.Owin.Common.WebApi;
using File = Carbon.Business.CloudDomain.File;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("file")]
    public class FileController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;
        private readonly IRepository<CompanyFile> _companyFileRepository;
        private readonly CdnService _cdnService;

        public class FileContentModel
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }

        public FileController(IActorFabric actorFabric, IRepository<CompanyFile> companyFileRepository, CdnService cdnService)
        {
            _actorFabric = actorFabric;
            _companyFileRepository = companyFileRepository;
            _cdnService = cdnService;
        }

        private string FullFileUrl(string url)
        {
            return AppSettings.Endpoints.File.AddPath(url).WithoutScheme();
        }

        private async Task<IEnumerable> CompanyImages(string companyId)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            var files = await actor.GetFiles(GetUserId());

            return files.OrderByDescending(x => x.ModifiedDateTime).Select(x => ToMetadata(companyId, x));
        }

        [HttpGet, Route("images")]
        public async Task<IHttpActionResult> Images(string companyId)
        {
            return Ok(new { Images = await CompanyImages(companyId) });
        }

        private string ThumbUrl(string url)
        {
            var fileName = Path.GetFileNameWithoutExtension(url);
            var ext = Path.GetExtension(url);
            return url.Substring(0, url.LastIndexOf("/") + 1) + fileName + "_preview" + ext;
        }

        //        [HttpPost]
        //        public virtual ActionResult UploadExportedImage(long folderId, string encodedImage)
        //        {
        //            var name = Guid.NewGuid().ToString();
        //            var company = FindCompany(folderId);
        //
        //            var contents = _projectRendersService.DataFromDataURL(encodedImage);
        //            var companyFileInfo = SaveCompanyImage(company, name, contents);
        //
        //            if (companyFileInfo != null)
        //            {
        //                return Json(new
        //                {
        //                    adapter = "filesystem",
        //                    status = true,
        //                    userIcons = CompanyImages(folderId),
        //                    uploadedFileSrc = FullFileUrl(companyFileInfo.Url)
        //                });
        //            }
        //
        //            return Json(new
        //            {
        //                adapter = "filesystem",
        //                status = false,
        //                error = DictionaryValidator.Errors.ElementAt(0)
        //            });
        //        }

        [HttpPost, Route("uploadUrl")]
        public async Task<IHttpActionResult> UploadUrl(FileContentModel model)
        {
            var split = model.Content.Split(',');
            if (split.Length != 2)
            {
                throw new ArgumentException(nameof(model));
            }

            var companyId = GetUserId();
            var image = Convert.FromBase64String(split[1]);
            var result = await SaveCompanyImage(companyId, model.Name, new MemoryStream(image));
            return Ok(ToMetadata(companyId, result));
        }

        [HttpPost, Route("upload")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                return BadRequest("Wrong mime type");
            }

            var multipart = await Request.Content.ReadAsMultipartAsync();
            if (multipart.Contents.Count == 0)
            {
                return BadRequest("No contents");
            }

            //companyId always comes first
            var companyId = await multipart.Contents[0].ReadAsStringAsync();

            var tasks = new List<Task<CompanyFileInfo>>();
            foreach (var part in multipart.Contents.Skip(1))
            {
                tasks.Add(this.SaveCompanyImage(companyId,
                    part.Headers.ContentDisposition.FileName.Unquote(),
                    await part.ReadAsStreamAsync()));
            }
            await Task.WhenAll(tasks);

            var images = tasks.Where(x => x != null).Select(x => ToMetadata(companyId, x.Result));
            return Ok(new {Images = images});
        }

        [HttpPost, Route("uploadPublicImage")]
        public async Task<IHttpActionResult> UploadPublicImage(FileContentModel model)
        {
            var file = new File("img", Guid.NewGuid().ToString("N") + ".png");
            var url = await _cdnService.UploadImage(file, model.Content);
            return Ok(new {url=url});
        }

        [HttpPost, Route("uploadPublicFile")]
        public async Task<IHttpActionResult> UploadPublicFile(FileContentModel data)
        {
            var file = new File("file", Guid.NewGuid().ToString("N") + ".data");
            var url = await _cdnService.UploadFile(file, data.Content);
            return Ok(new { url = url });
        }

        [HttpPost, Route("delete")]
        public async Task<IHttpActionResult> Delete(string companyId, string name)
        {
            var userId = GetUserId();
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            var file = await actor.GetFile(userId, name);

            if (file != null)
            {
                await Task.WhenAll(
                    actor.DeleteFile(userId, name),
                    _companyFileRepository.DeleteAsync(file.Id),
                    _companyFileRepository.DeleteAsync(ThumbUrl(file.Id)));
            }

            return Success();
        }

        private async Task<CompanyFileInfo> SaveCompanyImage(string companyId, string name, Stream stream)
        {
            var userId = GetUserId();
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            var existingTask = actor.GetFile(userId, name);

            try
            {
                using (var image = Image.FromStream(stream))
                {
                    await existingTask;
                    var existing = existingTask.Result;

                    var fileId = existing?.Id ?? Guid.NewGuid().ToString("N");
                    var companyFile = new CompanyFile(companyId, fileId + ".png");
                    companyFile.AutoDetectContentType();
                    stream.Position = 0;
                    companyFile.ContentStream = stream;
                    var fileInsertTask = _companyFileRepository.InsertAsync(companyFile);

                    var thumbSize = MediaUtil.FitSize(image.Size, new Size(640, 640));

                    var previewFile = new CompanyFile();
                    previewFile.Id = ThumbUrl(companyFile.Id);
                    previewFile.AutoDetectContentType();

                    Task previewInsertTask;
                    using (var previewImage = MediaUtil.Resize(image, thumbSize))
                    using (var thumbStream = new MemoryStream())
                    {
                        previewImage.Save(thumbStream, ImageFormat.Png);
                        thumbStream.Position = 0;
                        previewFile.ContentStream = thumbStream;
                        previewInsertTask = _companyFileRepository.InsertAsync(previewFile);
                    }

                    var companyFileInfo = existing ?? new CompanyFileInfo();
                    companyFileInfo.Id = fileId;
                    companyFileInfo.Name = name;
                    companyFileInfo.Size = stream.Length;
                    companyFileInfo.ModifiedDateTime = DateTime.UtcNow;

                    var metadata = companyFileInfo.GetMetadata();
                    metadata["width"] = image.Size.Width;
                    metadata["height"] = image.Size.Height;
                    metadata["thumbWidth"] = thumbSize.Width;
                    metadata["thumbHeight"] = thumbSize.Height;
                    companyFileInfo.UpdateMetadata(metadata);

                    await Task.WhenAll(actor.RegisterFile(userId, companyFileInfo), fileInsertTask, previewInsertTask);

                    return companyFileInfo;
                }
            }
            catch (Exception ex)
            {
                LogService.GetLogger().Error(ex);
                return null;
            }
        }

        private object ToMetadata(string companyId, CompanyFileInfo x)
        {
            var metadata = x.GetMetadata();
            var url = CompanyFile.FullRelativeUrl(companyId, x.Id + ".png");
            return new
            {
                url = FullFileUrl(url),
                name = x.Name,
                thumbUrl = FullFileUrl(ThumbUrl(url)),
                width = metadata.Value<int>("width"),
                height = metadata.Value<int>("height"),
                thumbWidth = metadata.Value<int>("thumbWidth"),
                thumbHeight = metadata.Value<int>("thumbHeight"),
            };
        }
    }
}