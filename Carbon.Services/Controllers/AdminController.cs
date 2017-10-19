using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Carbon.Business;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Owin.Common.WebApi;
using Carbon.Framework.Repositories;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using System;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using Newtonsoft.Json.Linq;
using Carbon.Business.Sync;
using Carbon.Business.Domain.DataTree;

namespace Carbon.Services.Controllers
{
    [RoutePrefix("admin")]
#if !DEBUG
    [Authorize(Roles = Defs.Roles.Administrators)]
#endif
    public class AdminController : AuthorizedApiController
    {
        private readonly ProjectModelService _projectModelService;
        private readonly IRepository<ProjectLog> _projectLogRepository;
        private readonly IRepository<ProjectState> _projectStateRepository;
        private readonly IRepository<ProjectModel> _projectModelRepository;
        private readonly IRepository<ProjectSnapshot> _snapshotRepository;
        private readonly IActorFabric _actorFabric;

        public AdminController(ProjectModelService projectModelService,
            IRepository<ProjectLog> projectLogRepository,
            IRepository<ProjectModel> projectModelRepository,
            IRepository<ProjectSnapshot> snapshotRepository,
            IRepository<ProjectState> projectStateRepository,
            IActorFabric actorFabric)
        {
            _projectLogRepository = projectLogRepository;
            _projectStateRepository = projectStateRepository;
            _projectModelService = projectModelService;
            _projectModelRepository = projectModelRepository;
            _snapshotRepository = snapshotRepository;
            _actorFabric = actorFabric;
        }

        [Route("projectModel")]
        public async Task<HttpResponseMessage> GetProjectModel(string companyId, string modelId)
        {
            var change = new ProjectModelChange
            {
                CompanyId = companyId,
                ModelId = modelId,
                UserId = GetUserId(),
                PrimitiveStrings = new List<string>()
            };
            var model = await _projectModelService.ChangeProjectModel(Scope, change);
            await model.EnsureLoaded();

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(model.Write(Formatting.Indented));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            return response;
        }

        [Route("projectLog"), FileResponse("application/zip")]
        public async Task<HttpResponseMessage> GetProjectLog(string companyId, string modelId)
        {
            var partitionKey = ProjectLog.GeneratePartitionKey(companyId, modelId);

            var rowKey = ProjectLog.GenerateKey(DateTimeOffset.MinValue);
            var spec = new FindByRowKeyRange<ProjectLog>(partitionKey, fromKey: rowKey);
            var log = (await _projectLogRepository.FindAllByAsync(spec)).ToList();

            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);

            var resultDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(resultDir);

            var zipFile = Path.Combine(resultDir,  $"log_{companyId}_{modelId}.zip");
            try
            {
                foreach (var entry in log)
                {
                    using (var stream = System.IO.File.Create(Path.Combine(dir, $"P_{entry.ToVersion}.json")))
                    using (var writer = new StreamWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(JsonConvert.SerializeObject(new
                        {
                            entry.PartitionKey,
                            entry.RowKey,
                            entry.FromVersion,
                            entry.ToVersion,
                            entry.UserId,
                            Primitives = new JArray(entry.Primitives.Select(JObject.Parse))
                        }, Formatting.Indented));
                    }
                }

                var state = await _projectStateRepository.FindSingleByAsync(new FindByRowKey<ProjectState>(companyId, modelId));
                using (var stream = System.IO.File.Create(Path.Combine(dir, "State.json")))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(JsonConvert.SerializeObject(new
                    {
                        state.InitialVersion,
                        state.EditVersion,
                        state.TimesSaved
                    }, Formatting.Indented));
                }

                ZipFile.CreateFromDirectory(dir, zipFile, CompressionLevel.Fastest, false);
            }
            finally
            {
                Directory.Delete(dir, true);
            }

            var zipStream = new FileStream(zipFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            return ZippedContent(new StreamContent(zipStream), Path.GetFileName(zipFile));
        }

        [Route("projectFromJson"), HttpPost, FileRequest("multipart/form-data")]
        public async Task<IHttpActionResult> CreateProjectFromJson()
        {
            var multipart = await Request.Content.ReadAsMultipartAsync();
            if (multipart.Contents.Count != 1)
            {
                return BadRequest("Single file expected");
            }

            var userId = GetUserId();
            var file = await multipart.Contents[0].ReadAsStringAsync();
            var model = await _projectModelService.CreateProjectAndModel(userId, userId);
            var latestSnapshot = await _snapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(model.CompanyId, model.Id));

            var id = model.Id;
            model.Read(file);
            model.Id = id;

            using (var stream = model.ToStream())
            {
                latestSnapshot.ContentStream = stream;
                await _snapshotRepository.UpdateAsync(latestSnapshot);
            }

            var actor = _actorFabric.GetProxy<ICompanyActor>(userId);
            var info = await actor.GetCompanyInfo();

            return Ok(new { id, link = $"{Request.RequestUri.GetLeftPart(UriPartial.Authority)}/app/@{info.Name}/id" });
        }
    }
}