using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Cloud;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Business.Services
{
    public class SharingService
    {
        public const string CharSymbols = "qwrtsdfghjklzxcvbnmQWRTSDFGHJKLZXCVBNM123456789";
        private readonly string PublicScopePartition = Guid.Empty.ToString();

        private readonly IRepository<ShareToken> _shareTokenRepository;
        private readonly IRepository<SharedPage> _sharedPageRepository;
        private readonly IActorFabric _actorFabric;
        private readonly ICloudUnitOfWorkFactory _cloudUnitOfWorkFactory;

        public SharingService(IRepository<ShareToken> shareTokenRepository, IRepository<SharedPage> sharedPageRepository, IActorFabric actorFabric, ICloudUnitOfWorkFactory cloudUnitOfWorkFactory)
        {
            _shareTokenRepository = shareTokenRepository;
            _cloudUnitOfWorkFactory = cloudUnitOfWorkFactory;
            _actorFabric = actorFabric;
            _sharedPageRepository = sharedPageRepository;
        }

        public async Task<ShareToken> Invite(string userId, string companyId, string projectId, Permission permission, string email = null)
        {
            var token = await GenerateShareToken(companyId, userId, projectId, permission, email);
            var actor = _actorFabric.GetProxy<ICompanyActor>(companyId);
            if (!string.IsNullOrEmpty(email))
            {
                await actor.RegisterKnownEmail(userId, email);
                //TODO: send mail
            }
            return token;
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
            string imageUri;
            var split = dataUrl.Split(',');
            if (split.Length != 2)
            {
                throw new InvalidEnumArgumentException("dataUrl");
            }
            string previewPicture = split[1];
            using (var uow = _cloudUnitOfWorkFactory.NewUnitOfWork())
            {
                File file = new File("img", id + ".png");
                file.SetContent(Convert.FromBase64String(previewPicture));
                await uow.InsertAsync(file);
                imageUri = file.Uri.AbsoluteUri;
                uow.Commit();
            }

            return imageUri;
        }

        private async Task<string> SaveData(string id, string data)
        {
            string dataUri;
            using (var uow = _cloudUnitOfWorkFactory.NewUnitOfWork())
            {
                File file = new File("data", id + ".json");
                file.SetContent(data);
                await uow.InsertAsync(file);
                dataUri = file.Uri.AbsoluteUri;
                uow.Commit();
            }

            return dataUri;
        }

        public async Task<SharedPage> PublishPage(string userId, string name, string description, string tags, string data, string previewPicture, PublishScope scope)
        {
            var actor = _actorFabric.GetProxy<ICompanyActor>(userId);

            var id = Guid.NewGuid().ToString();

            var imageUri = SaveImage(id, previewPicture);
            var dataUri = SaveData(id, data);

            var partition = scope == PublishScope.Company ? userId : PublicScopePartition;

            SharedPage page = new SharedPage
            {
                PartitionKey = partition,
                RowKey = id,
                ImageUrl = await imageUri,
                DataUrl = await dataUri,
                CreatedByUserId = userId,
                CreatedByUserName = await actor.GetCompanyName(),
                Tags = tags,
                Created = DateTime.UtcNow,
                Description = description,
                Name = name
            };

            await _sharedPageRepository.InsertAsync(page);
            return page;
        }

        public async Task<IQueryable<SharedPage>> SearchResources(string userId, string search, PublishScope scope)
        {
            search = search ?? "";
            if (scope == PublishScope.Public)
            {
                return (await _sharedPageRepository.FindAllByAsync(new FindByPartition<SharedPage>(PublicScopePartition))).Where(p=>
                (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase)!=-1)
                || (!string.IsNullOrEmpty(p.Tags) && p.Tags.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1));
            }
            else
            {
                return (await _sharedPageRepository.FindAllByAsync(new FindByPartition<SharedPage>(userId))).Where(p =>
                (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1)
                || (!string.IsNullOrEmpty(p.Tags) && p.Tags.IndexOf(search, 0, StringComparison.InvariantCultureIgnoreCase) != -1));
            }
        }
    }
}