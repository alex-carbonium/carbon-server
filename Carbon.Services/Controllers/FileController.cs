using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using Carbon.Framework.Cloud;
using Carbon.Framework.Extensions;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;
using Carbon.Owin.Common.WebApi;
using File = Carbon.Business.CloudDomain.File;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("file")]
    public class FileController : AuthorizedApiController
    {
        private readonly IActorFabric _actorFabric;
        private readonly IRepository<CompanyFile> _fileRepository;
        private readonly ICloudUnitOfWorkFactory _cloudUnitOfWorkFactory;

        public class FileContentModel
        {
            public string Content { get; set; }
        }

        public FileController(IActorFabric actorFabric, IRepository<CompanyFile> fileRepository, ICloudUnitOfWorkFactory cloudUnitOfWorkFactory)
        {
            _actorFabric = actorFabric;
            _fileRepository = fileRepository;
            _cloudUnitOfWorkFactory = cloudUnitOfWorkFactory;
        }

        private string FullFileUrl(string url)
        {
            var storageUri = AppSettings.GetString("Endpoints", "File");
            var uri = new Uri(storageUri, UriKind.Absolute).AddPath(url);
            return uri.AbsoluteUri.Substring(uri.Scheme.Length + 1);
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
        public async Task<IHttpActionResult> UploadPublicImage(FileContentModel data)
        {
            string imageUri;
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            var split = data.Content.Split(',');
            if (split.Length != 2)
            {
                throw new InvalidEnumArgumentException(nameof(data));
            }
            string previewPicture = split[1];
            using (var uow = _cloudUnitOfWorkFactory.NewUnitOfWork())
            {
                File file = new File("img", Guid.NewGuid() + ".png");
                file.SetContent(Convert.FromBase64String(previewPicture));
                await uow.InsertAsync(file);
                imageUri = file.Uri.AbsoluteUri;
                uow.Commit();
            }

            return Ok(new {url=imageUri});
        }

        [HttpPost, Route("uploadPublicFile")]
        public async Task<IHttpActionResult> UploadPublicFile(FileContentModel data)
        {
            string fileUri;
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            using (var uow = _cloudUnitOfWorkFactory.NewUnitOfWork())
            {
                File file = new File("file", Guid.NewGuid() + ".data");
                file.SetContent(data.Content);
                await uow.InsertAsync(file);
                fileUri = file.Uri.AbsoluteUri;
                uow.Commit();
            }

            return Ok(new { url = fileUri });
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
                    _fileRepository.DeleteAsync(file.Id),
                    _fileRepository.DeleteAsync(ThumbUrl(file.Id)));
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
                    var fileInsertTask = _fileRepository.InsertAsync(companyFile);

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
                        previewInsertTask = _fileRepository.InsertAsync(previewFile);
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