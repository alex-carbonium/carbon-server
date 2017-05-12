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

        public AdminController(ProjectModelService projectModelService,
            IRepository<ProjectLog> projectLogRepository,
            IRepository<ProjectState> projectStateRepository)
        {
            _projectModelService = projectModelService;
            _projectLogRepository = projectLogRepository;
            _projectStateRepository = projectStateRepository;
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

        [Route("projectLog")]
        public async Task<HttpResponseMessage> GetProjectLog(string companyId, string modelId)
        {
            var partitionKey = ProjectLog.GeneratePartitionKey(companyId, modelId);

            var rowKey = ProjectLog.GenerateKey(DateTimeOffset.MinValue);
            var spec = new FindByRowKeyRange<ProjectLog>(partitionKey, fromKey: rowKey);
            var log = (await _projectLogRepository.FindAllByAsync(spec)).ToList();

            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(dir);

            var zipFile = Path.Combine(Path.GetTempPath(), $"log_{companyId}_{modelId}.zip");
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
    }
}