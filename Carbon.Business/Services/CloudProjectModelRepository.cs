using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Domain.DataTree;
using Carbon.Business.Exceptions;
using Carbon.Business.ModelMigrations;
using Carbon.Business.Sync;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.Framework.Specifications;

namespace Carbon.Business.Services
{
    public class CloudProjectModelRepository : Repository<ProjectModel>
    {
        public const int SnapshotFlushInterval = 500;
        public const int MaxUpdateConflicts = 100;
        public static readonly TimeSpan PrimitiveKeyTolerance = TimeSpan.FromSeconds(3);

#if USE_INT_VERSIONS
        private static int _versionCounter = 0;
#endif

        private readonly ILogService _logService;
        private readonly IRepository<ProjectSnapshot> _snapshotRepository;
        private readonly IRepository<ProjectLog> _primitivesRepository;
        private readonly IRepository<ProjectState> _realtimeInfoRepository;

        public CloudProjectModelRepository(
            ILogService logService,
            IRepository<ProjectSnapshot> snapshotRepository,
            IRepository<ProjectLog> primitivesRepository,
            IRepository<ProjectState> realtimeInfoRepository)
        {
            _logService = logService;
            _snapshotRepository = snapshotRepository;
            _primitivesRepository = primitivesRepository;
            _realtimeInfoRepository = realtimeInfoRepository;
        }

        public override IQueryable<ProjectModel> FindAll(bool cache)
        {
            throw new NotImplementedException();
        }

        public override IQueryable<ProjectModel> FindAllBy(ISpecification<ProjectModel> specification)
        {
            throw new NotImplementedException();
        }

        public override ProjectModel FindSingleBy(ISpecification<ProjectModel> specification)
        {
            throw new NotImplementedException();
        }

        public override ProjectModel FindById(dynamic key, bool lockForUpdate = false)
        {
            var versionSpec = new FindByRowKey<ProjectState>(key.CompanyId, key.ProjectId);
            var realtimeInfo = _realtimeInfoRepository.FindSingleBy(versionSpec);
            return FindById(key, realtimeInfo);
        }
        public override async Task<ProjectModel> FindByIdAsync(dynamic key, bool lockForUpdate = false)
        {
            var versionSpec = new FindByRowKey<ProjectState>(key.CompanyId.ToString(), key.ProjectId.ToString());
            var realtimeInfo = await _realtimeInfoRepository.FindSingleByAsync(versionSpec);
            return FindById(key, realtimeInfo);
        }

        private ProjectModel FindById(dynamic key, ProjectState state)
        {
            if (state == null)
            {
                return null;
            }
            string projectId = key.ProjectId;
            string companyId = key.CompanyId;
            var model = ProjectModel.CreateLazy(companyId, projectId, LoadModel);
            model.State = state;
            model.EditVersion = state.EditVersion;
            return model;
        }


        public override ProjectModel FindFirstOnly()
        {
            throw new NotImplementedException();
        }

        public override bool ExistsBy(ISpecification<ProjectModel> specification)
        {
            throw new NotImplementedException();
        }

        public override void Insert(ProjectModel model)
        {
            throw new NotSupportedException("Use async version");
        }

        public override async Task InsertAsync(ProjectModel model)
        {
            if (model.GetProp("modelVersion") == null)
            {
                model.SetProp("modelVersion", ModelMigrator.MaxVersion);
            }

            var version = string.IsNullOrEmpty(model.EditVersion) ? Guid.NewGuid().ToString() : model.EditVersion;
            var dateTime = DateTimeOffset.UtcNow;

            using (var stream = model.ToStream())
            {
                var snapshot = new ProjectSnapshot(ProjectSnapshot.LatestId(model.CompanyId, model.Id), dateTime);
                snapshot.ContentStream = stream;
                snapshot.EditVersion = version;
                await _snapshotRepository.InsertAsync(snapshot);
            }

            var state = new ProjectState(model.CompanyId, model.Id);
            state.InitialVersion = version;
            state.EditVersion = version;
            await _realtimeInfoRepository.InsertAsync(state);

            model.State = state;
            model.EditVersion = version;
        }

        public override void Update(ProjectModel entity)
        {
            throw new NotSupportedException("Use async version");
        }

        public override async Task UpdateAsync(ProjectModel model)
        {
            var saved = false;
            var change = model.Change;
            var realtimeInfo = model.State;
            var originalModel = model;
            var fromVersion = realtimeInfo.EditVersion;
#if USE_INT_VERSIONS
            var toVersion = Interlocked.Increment(ref _versionCounter).ToString();
#else
            var toVersion = Guid.NewGuid().ToString();
#endif
            var attempt = 0;
            var tooManyConflicts = false;
            do
            {
                try
                {
                    fromVersion = realtimeInfo.EditVersion;
                    realtimeInfo.EditVersion = toVersion;
                    realtimeInfo.TimesSaved += 1;
                    await _realtimeInfoRepository.UpdateAsync(realtimeInfo);
                    saved = true;
                }
                catch (UpdateConflictException)
                {
                    model = FindById(new { CompanyId = model.CompanyId, ProjectId = model.Id });
                    realtimeInfo = model.State;
                    tooManyConflicts = ++attempt == MaxUpdateConflicts;
                }
            } while (!saved && !tooManyConflicts);

            if (tooManyConflicts)
            {
                throw new UpdateConflictException("Too many update conflicts");
            }

            originalModel.PreviousEditVersion = fromVersion;
            originalModel.EditVersion = realtimeInfo.EditVersion;
            originalModel.State = realtimeInfo;

            await _primitivesRepository.InsertAsync(new ProjectLog(model.CompanyId, model.Id)
            {
                Primitives = change.PrimitiveStrings,
                FromVersion = fromVersion,
                ToVersion = toVersion,
                UserId = change.UserId
            });

            if (realtimeInfo.TimesSaved % SnapshotFlushInterval == 0)
            {
                await LoadModel(model);
            }
        }

        public override void Delete(ProjectModel model)
        {
        }

        public override async Task DeleteAsync(ProjectModel model)
        {
            await _snapshotRepository.DeleteAsync(ProjectSnapshot.LatestId(model.CompanyId, model.Id));

            dynamic realtimeInfoKey = new ExpandoObject();
            realtimeInfoKey.PartitionKey = model.CompanyId;
            realtimeInfoKey.RowKey = model.Id;
            await _realtimeInfoRepository.DeleteAsync(realtimeInfoKey);
        }

        public override void InsertOrUpdate(ProjectModel entity)
        {
            throw new NotSupportedException("Use async version");
        }

        public override void Delete(dynamic id)
        {
        }

        public override void Lock(dynamic id)
        {
        }

        public async Task LoadModel(ProjectModel model)
        {
            await LoadModel(model, null);
        }

        public async Task<IList<ProjectLog>> LoadModel(ProjectModel model, IList<ProjectLog> batchPrimitives)
        {
            var projectId = model.Id;
            var latestSnapshot = await _snapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(model.CompanyId, projectId));
            model.Read(latestSnapshot.ContentStream);
            model.EditVersion = latestSnapshot.EditVersion;

            if (batchPrimitives == null)
            {
                var partitionKey = ProjectLog.GeneratePartitionKey(model.CompanyId, model.Id);
                var rowKey = ProjectLog.GenerateKey(latestSnapshot.DateTime.Add(-PrimitiveKeyTolerance));
                var forwardSpec = new FindByRowKeyRange<ProjectLog>(partitionKey, fromKey: rowKey);
                batchPrimitives = (await _primitivesRepository.FindAllByAsync(forwardSpec)).ToList();
            }

            var tail = BuildTail(projectId, latestSnapshot.EditVersion, batchPrimitives);
            var updateSnapshot = false;

            if (tail.Count > 0)
            {
                var primitives = new List<DataNodeBasePrimitive>();

                foreach (var batchEntity in tail)
                {
                    var batch = PrimitiveFactory.CreateMany<DataNodeBasePrimitive>(batchEntity.Primitives);
                    primitives.AddRange(batch);
                }

                if (RemoveDuplicatePrimitives(primitives))
                {
                    _logService.GetLogger().Warning("Primitives with duplicate ids");
                }

                ApplyPrimitives(model, primitives);
                model.EditVersion = tail.Last().ToVersion;

                updateSnapshot = true;
            }

            updateSnapshot = ModelMigrator.Run(model, _logService) || updateSnapshot;

            if (updateSnapshot)
            {
                using (var stream = model.ToStream())
                {
                    latestSnapshot.ContentStream = stream;
                    if (tail.Count > 0)
                    {
                        latestSnapshot.EditVersion = model.EditVersion;
                        latestSnapshot.DateTime = tail.Last().GetDateTime();
                    }
                    await _snapshotRepository.UpdateAsync(latestSnapshot);
                }
            }

            return tail;
        }

        private bool RemoveDuplicatePrimitives(List<DataNodeBasePrimitive> primitives)
        {
            var idSet = new HashSet<string>();
            List<DataNodeBasePrimitive> toRemove = null;
            foreach (var primitive in primitives)
            {
                if (!idSet.Contains(primitive.Id))
                {
                    idSet.Add(primitive.Id);
                }
                else
                {
                    if (toRemove == null)
                    {
                        toRemove = new List<DataNodeBasePrimitive>();
                    }
                    toRemove.Add(primitive);
                }
            }
            if (toRemove != null)
            {
                foreach (var json in toRemove)
                {
                    primitives.Remove(json);
                }
            }

            return toRemove != null;
        }

        private void ApplyPrimitives(DataNode model, List<DataNodeBasePrimitive> primitives)
        {
            var primitiveVisitor = new PrimitiveVisitor(primitives);
            model.Visit(primitiveVisitor);
        }

        private IList<ProjectLog> BuildTail(string projectId, string fromVersion, IList<ProjectLog> primitives)
        {
            var result = new List<ProjectLog>();
            var index = 0;
            var currentVersion = fromVersion;

            //build forward
            while (primitives.Count > 0 && index < primitives.Count)
            {
                var currentBatch = primitives[index];
                if (currentBatch.FromVersion == currentVersion)
                {
                    result.Add(currentBatch);
                    primitives.RemoveAt(index);
                    currentVersion = currentBatch.ToVersion;
                    index = 0;
                }
                else
                {
                    ++index;
                }
            }

            //build backward
            if (primitives.Count > 0)
            {
                index = 0;
                currentVersion = fromVersion;
                while (primitives.Count > 0 && index < primitives.Count)
                {
                    var currentBatch = primitives[index];
                    if (currentBatch.ToVersion == currentVersion)
                    {
                        primitives.RemoveAt(index);
                        currentVersion = currentBatch.FromVersion;
                        index = 0;
                    }
                    else
                    {
                        ++index;
                    }
                }
            }

            if (primitives.Count > 0)
            {
                _logService.GetLogger().Warning($"Could not build tail for project ${projectId}: {fromVersion} -> ${primitives.Last().ToVersion}");

                result.AddRange(BuildTail(projectId, primitives[0].FromVersion, primitives));
            }

            return result;
        }
    }
}
