using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Repositories;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;
using Carbon.Framework.Specifications;

namespace Carbon.Business.Services
{
    public class SharingService
    {
        public const string CharSymbols = "qwrtsdfghjklzxcvbnmQWRTSDFGHJKLZXCVBNM123456789";

        private readonly IRepository<ShareToken> _shareTokenRepository;
        private readonly IPrivateSharedPageRepository _privatePageRepository;
        private readonly IPublicSharedPageRepository _publicPageRepository;
        private readonly IActorFabric _actorFabric;
        private readonly CdnService _cdnService;

        public SharingService(IRepository<ShareToken> shareTokenRepository,
            IPublicSharedPageRepository publicPageRepository,
            IPrivateSharedPageRepository privatePageRepository,
            IActorFabric actorFabric,
            CdnService cdnService)
        {
            _shareTokenRepository = shareTokenRepository;
            _actorFabric = actorFabric;
            _publicPageRepository = publicPageRepository;
            _cdnService = cdnService;
            _privatePageRepository = privatePageRepository;
        }

        public async Task<IEnumerable<ProjectShareCode>> GetShareCodes(string userId, string companyId, string projectId)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            return await actor.GetProjectShareCodes(userId, projectId);
        }

        public async Task<ShareToken> AddShareCode(string userId, string companyId, string projectId, Permission permission, string email = null)
        {
            var token = await GenerateShareToken(companyId, userId, projectId, permission, email);
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);

            await actor.AddProjectShareCode(userId, projectId, new ProjectShareCode { Id = token.PartitionKey, Permission = (int)permission });

            if (!string.IsNullOrEmpty(email))
            {
                await actor.RegisterKnownEmail(userId, email);
                //TODO: send mail
            }
            return token;
        }

        public async Task RemoveShareCode(string userId, string companyId, string projectId, string code)
        {
            var token = await _shareTokenRepository.FindSingleByAsync(new FindByRowKey<ShareToken>(code, code));
            if (token != null)
            {
                await _shareTokenRepository.DeleteAsync(token);
            }

            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            await actor.RemoveProjectShareCode(userId, projectId, code);
        }

        public async Task RemoveShareCodes(string userId, string companyId, string projectId)
        {
            var codes = await GetShareCodes(userId, companyId, projectId);
            var tasks = new List<Task>();
            foreach (var code in codes)
            {
                var spec = new FindByRowKey<ShareToken>(code.Id, code.Id);
                tasks.Add(_shareTokenRepository.FindSingleByAsync(spec)
                    .ContinueWith(tokenTask =>
                    {
                        if (tokenTask.Result != null)
                        {
                            return _shareTokenRepository.DeleteAsync(tokenTask.Result);
                        }
                        return tokenTask;
                    }));
            }

            await Task.WhenAll(tasks);

            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            await actor.RemoveProjectShareCodes(userId, projectId);
        }

        private async Task<ShareToken> GenerateShareToken(string companyId, string userId, string projectId, Permission permission, string email = null)
        {
            ShareToken token = null;
            for (var i = 0; i < 10; ++i)
            {
                token = new ShareToken
                {
                    CompanyId = companyId,
                    ProjectId = projectId,
                    Permission = (int)permission,
                    Email = email,
                    CreatedByUserId = userId
                };
                token.SetCode(GenerateShareCode());

                try
                {
                    await _shareTokenRepository.InsertAsync(token);
                    break;
                }
                catch (InsertConflictException)
                {
                    token = null;
                }
            }

            if (token == null)
            {
                throw new Exception("Could not generate share code");
            }
            return token;
        }

        public async Task DisableMirroring(string userId, string companyId, string projectId)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);

            var oldCode = await actor.SetProjectMirrorCode(userId, projectId, null);

            if (string.IsNullOrEmpty(oldCode))
            {
                return;
            }

            var token = await _shareTokenRepository.FindSingleByAsync(new FindByRowKey<ShareToken>(oldCode, oldCode));
            if (token != null)
            {
                await _shareTokenRepository.DeleteAsync(token);
            }
        }

        public async Task<string> GenerateMirroringToken(string userId, string companyId, string projectId, bool enable)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);

            var code = await actor.GetProjectMirrorCode(userId, projectId);
            if (code != null)
            {
                return code;
            }

            if (!enable)
            {
                return null;
            }

            ShareToken token = null;

            for (var i = 0; i < 10; ++i)
            {
                token = new ShareToken
                {
                    CompanyId = companyId,
                    ProjectId = projectId,
                    Permission = (int)Permission.Read,
                    CreatedByUserId = userId
                };
                token.SetCode(GenerateShareCode());

                try
                {
                    await _shareTokenRepository.InsertAsync(token);
                    break;
                }
                catch (InsertConflictException)
                {
                    token = null;
                }
            }

            if (token == null)
            {
                throw new Exception("Could not generate share code");
            }

            await actor.SetProjectMirrorCode(userId, projectId, token.RowKey);

            return token.RowKey;
        }

        private string GenerateShareCode()
        {
            var result = new List<char>();
            var x = Math.Abs(Guid.NewGuid().GetHashCode());
            var b = CharSymbols.Length;
            do
            {
                var v = (int)(x % b);
                result.Add(CharSymbols[v]);

                x = x / b;
            } while (x > 0);

            return new string(result.ToArray());
        }

        private static Project FindProject(Company company, string projectId)
        {
            //find in folders here
            return company.RootFolder.Projects.SingleOrDefault(x => x.Id == projectId);
        }

        public async Task<Tuple<ExternalAcl, string>> UseCode(string requestingUserId, string code)
        {
            var token = await _shareTokenRepository.FindSingleByAsync(new FindByRowKey<ShareToken>(code, code));
            if (token == null)
            {
                return null;
            }

            var actor = _actorFabric.GetProxy<ICompanyActor>(token.CompanyId);
            var permission = token.Permission;

            var aclTask = actor.ShareProject(requestingUserId, token.ProjectId, permission);

            ++token.TimesUsed;
            var updateToken = _shareTokenRepository.UpdateAsync(token);

            await Task.WhenAll(aclTask, updateToken);
            return Tuple.Create(aclTask.Result, token.CreatedByUserId);
        }

        private async Task<string> SaveImage(string id, string dataUrl)
        {
            var file = new File("img", id + ".png");
            return await _cdnService.UploadImage(file, dataUrl);
        }

        private async Task<string> SaveData(string id, string data)
        {
            File file = new File("data", id + ".json");
            return await _cdnService.UploadFile(file, data);
        }

        public async Task<SharedPage> GetPageSetup(string userId, string pageId)
        {
            var publicPageTask = FindPageById(ResourceScope.Public, userId, pageId);
            var privatePageTask = FindPageById(ResourceScope.Company, userId, pageId);

            await Task.WhenAll(publicPageTask, privatePageTask);
            return privatePageTask.Result ?? publicPageTask.Result;
        }

        public async Task<ResourceNameStatus> ValidatePageName(ResourceScope scope, string userId, string name)
        {
            var pageId = SharedPage.PageNameToId(name);
            var page = await FindPageById(scope, userId: userId, pageId: pageId);

            if (page == null)
            {
                return ResourceNameStatus.Available;
            }
            if (page.AuthorId == userId)
            {
                return ResourceNameStatus.CanOverride;
            }
            return ResourceNameStatus.Taken;
        }

        public async Task<SharedPage> PublishPage(string userId, PublishPageModel model)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(userId);

            var id = SharedPage.PageNameToId(model.Name);
            var uniqueId = Guid.NewGuid().ToString("N");
            var imageUri = SaveImage(uniqueId, model.CoverUrl);
            var dataUri = SaveData(uniqueId, UpdatePageJson(id, model.Name, model.PageData));

            string partition;
            IRepository<SharedPage> repo;
            GetRepoAndPartition(model.Scope, userId, id, out partition, out repo);

            var status = await ValidatePageName(model.Scope, userId, model.Name);
            Task<int[]> deleteTask = Task.FromResult(new int[1]);
            if (status == ResourceNameStatus.CanOverride)
            {
                deleteTask = Task.WhenAll(
                    DeletePageById(ResourceScope.Company, userId, id),
                    DeletePageById(ResourceScope.Public, userId, id));
            }
            var companyInfo = actor.GetCompanyInfo();
            await Task.WhenAll(imageUri, dataUri, companyInfo, deleteTask);

            var page = new SharedPage
            {
                PartitionKey = partition,
                RowKey = id,
                CoverUrl = imageUri.Result,
                ScreenshotsUrls = model.Screenshots.Select(x => x.Url).ToArray(),
                ScreenshotIds = model.Screenshots.Select(x => x.Id).ToArray(),
                ScreenshotNames = model.Screenshots.Select(x => x.Name).ToArray(),
                DataUrl = dataUri.Result,
                AuthorId = userId,
                AuthorName = companyInfo.Result.Name,
                AuthorAvatar = companyInfo.Result.Logo ?? companyInfo.Result.Owner.Avatar,
                Tags = model.Tags,
                Created = DateTime.UtcNow,
                Description = model.Description,
                Name = model.Name,
                Scope = (int)model.Scope,
                TimesUsed = deleteTask.Result.Max()
            };

            await repo.InsertAsync(page);
            return page;
        }

        public async Task TrackPrivatePageUsed(string companyId, string pageId)
        {
            var spec = new FindByRowKey<SharedPage>(companyId, pageId);

            await TrackPageUsed(_privatePageRepository, spec);
        }

        public async Task TrackPublicPageUsed(string pageId)
        {
            var spec = new FindByRowKey<SharedPage>(pageId, pageId);

            await TrackPageUsed(_publicPageRepository, spec);
        }

        public async Task TrackPageUsed(IRepository<SharedPage> repo, ISpecification<SharedPage> spec)
        {
            var page = await repo.FindSingleByAsync(spec);
            if (page != null)
            {
                for (var i = 0; i < 10; ++i)
                {
                    try
                    {
                        page.TimesUsed += 1;
                        await repo.UpdateAsync(page);
                        break;
                    }
                    catch (UpdateConflictException)
                    {
                        page = await repo.FindSingleByAsync(spec);
                    }
                }
            }
        }

        private string UpdatePageJson(string id, string name, string data)
        {
            var obj = JObject.Parse(data);
            var pageProps = obj["page"]["props"];
            pageProps["id"] = id;
            pageProps["name"] = name;
            pageProps["galleryId"] = id;
            return obj.ToString();
        }

        private void GetRepoAndPartition(ResourceScope scope, string userId, string id, out string partition, out IRepository<SharedPage> repo)
        {
            if (scope == ResourceScope.Company)
            {
                partition = userId;
                repo = _privatePageRepository;
            }
            else
            {
                partition = id;
                repo = _publicPageRepository;
            }
        }

        private async Task<SharedPage> FindPageById(ResourceScope scope, string userId, string pageId)
        {
            string partition;
            IRepository<SharedPage> repo;
            GetRepoAndPartition(scope, userId, pageId, out partition, out repo);

            var spec = new FindByRowKey<SharedPage>(partition, pageId);
            return await repo.FindSingleByAsync(spec);
        }

        private async Task<int> DeletePageById(ResourceScope scope, string userId, string pageId)
        {
            var timesUsed = 0;
            string partition;
            IRepository<SharedPage> repo;
            GetRepoAndPartition(scope, userId, pageId, out partition, out repo);

            var spec = new FindByRowKey<SharedPage>(partition, pageId);
            var page = await repo.FindSingleByAsync(spec);
            if (page != null)
            {
                timesUsed = page.TimesUsed;
                await repo.DeleteAsync(page);
            }
            return timesUsed;
        }

        public async Task<IQueryable<SharedPage>> SearchCompanyResources(string userId, string search)
        {
            search = search ?? "";
            return (await _privatePageRepository.FindAllByAsync(new FindByPartition<SharedPage>(userId)))
                .ToList().AsQueryable()
                .Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                    || (!string.IsNullOrEmpty(p.Tags) && p.Tags.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1));
        }

        public async Task<IQueryable<SharedPage>> SearchPublicResources(string search)
        {
            search = search ?? "";
            search = search.Trim();

            Expression<Func<SharedPage, bool>> searchPredicate;
            if(search.StartsWith("tags:", StringComparison.InvariantCultureIgnoreCase))
            {
                search = search.Substring(5);
                searchPredicate = p => !string.IsNullOrEmpty(p.Tags) && p.Tags.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            else if (search.StartsWith("name:", StringComparison.InvariantCultureIgnoreCase))
            {
                search = search.Substring(5);
                searchPredicate = p => !string.IsNullOrEmpty(p.Name) && p.Tags.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            else if (search.StartsWith("description:", StringComparison.InvariantCultureIgnoreCase))
            {
                search = search.Substring(5);
                searchPredicate = p => !string.IsNullOrEmpty(p.Description) && p.Tags.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;
            }
            else
            {
                searchPredicate = p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                    || (!string.IsNullOrEmpty(p.Tags) && p.Tags.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1);
            }

            // TODO: fix this big performance issue
            return (await _publicPageRepository.FindAllAsync())
                .ToList().AsQueryable()
                .Where(searchPredicate);
        }

        public async Task<SharedPage> SearchPublicResourceById(string id)
        {
            // TODO: fix this perf issue
            return (await _publicPageRepository.FindAllAsync())
                .ToList().AsQueryable().FirstOrDefault(p => p.GalleryId == id);
        }
    }
}