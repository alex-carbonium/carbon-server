using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Business.ModelMigrations;
using Carbon.Business.Services;
using Carbon.Business.Sync;
using Carbon.Framework;
using Carbon.Test.Unit.Sync;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class CloudProjectModelRepositoryTests : ProjectModelRepositoryBaseTest
    {
        private const string OwnerId = "owner";
        private const string GuestId = "guest";

        private ProjectModelService _designerService;        

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();            
    
            _designerService = Scope.Resolve<ProjectModelService>();            
        }

        [TestMethod]
        public async Task SimpleSaveAndRetrieve()
        {
            //arrange
            var model = new ProjectModel();            
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));

            //act
            model = await _designerService.ChangeProjectModel(Scope, model.Change);
            var persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));

            //assert
            Assert.IsFalse(persisted.Loaded, "Model must be lazy loaded");
            await persisted.EnsureLoaded();            
            Assert.AreEqual(1, persisted.Children.Count, "Model loaded incorrectly");            
            Assert.IsTrue(persisted.Loaded, "Model must be loaded");
            
            var latestSnapshot = SnapshotRepository.FindById(ProjectSnapshot.LatestId(model.CompanyId, model.Id));
            var realtimeInfo = RealtimeRepository.Store[0];

            Assert.IsNotNull(latestSnapshot, "Latest snapshot must be created");            
            Assert.AreEqual(realtimeInfo.EditVersion, latestSnapshot.EditVersion, "Wrong edit version");
            Assert.AreEqual(persisted.EditVersion, latestSnapshot.EditVersion, "Wrong model edit version");
            Assert.AreEqual(latestSnapshot.EditVersion, latestSnapshot.EditVersion, "Wrong snapshot version");
            Assert.AreEqual(latestSnapshot.DateTime, latestSnapshot.DateTime, "Wrong date time");
        }

        //this is a test for import, actually
        //[TestMethod]
        //public async Task SavingShouldPreserveWithSpecificModelVersionIfSet()
        //{
        //    //arrange            
        //    var model = new ProjectModel();
        //    AddChanges(model, PrimitiveStubs.AppPropertyChanged("modelVersion", "1"));

        //    //act
        //    var persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));

        //    //assert
        //    var snapshot = await SnapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(persisted.Id));
        //    model = new ProjectModel();
        //    model.Read(snapshot.ContentStream);
        //    Assert.AreEqual("1", model.GetProp("modelVersion"));
        //}

        [TestMethod]
        public async Task SaveThenUpdateWithPrimitivesThenRetrieve()
        {
            //arrange            
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("PageId1"));

            model = await _designerService.ChangeProjectModel(Scope, model.Change);            

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));            

            AddChanges(persisted, PrimitiveStubs.PageAdd("PageId2"));
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId3") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            //assert
            persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));
            await persisted.EnsureLoaded();
            Assert.AreEqual(3, persisted.Children.Count);
        }

        [TestMethod]
        public async Task UnrelatedPrimitivesShouldBeIgnored()
        {
            //arrange            
            var model = new ProjectModel();           

            model = await _designerService.ChangeProjectModel(Scope, AddChanges(model));

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));

            AddChanges(persisted, new RawPrimitive { Type = PrimitiveType.Error });
            await _designerService.UpdateProjectModel(persisted, Permission.Owner);

            //assert, should not fail
            persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));
            await persisted.EnsureLoaded();
        }

        [TestMethod]
        [ExpectedException(typeof(InsufficientPermissionsException))]
        public async Task AnotherUserShouldNotReadModel()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act && assert
            var change = CreateChange(model, GuestId);
            await _designerService.ChangeProjectModel(Scope, change);                                    
        }

        [TestMethod]
        public async Task AnotherUserCanUpdateTheModel()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            await ActorFabricStub.GetProxy<ICompanyActor>(OwnerId).ShareProject(GuestId, model.Id, (int)Permission.Write);

            //act                        
            var change = CreateChange(model, GuestId);
            var persisted = await _designerService.ChangeProjectModel(Scope, change);
            change = CreateChange(persisted, GuestId, OwnerId, PrimitiveStubs.PageAdd("PageId2"));
            await _designerService.ChangeProjectModel(Scope, change);

            //assert
            persisted = await _designerService.ChangeProjectModel(Scope, AddChanges(model));
            await persisted.EnsureLoaded();
            Assert.AreEqual(2, persisted.Children.Count);
        }

        [TestMethod]
        public async Task PrimitivesCanBeInTheWrongOrder()
        {
            //arrange            
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId2", "name", "new name") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            var firstBatch = PrimitivesRepository.Store[0];
            PrimitivesRepository.Store.Remove(firstBatch);
            PrimitivesRepository.Store.Add(firstBatch);

            //assert
            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await persisted.EnsureLoaded();
            Assert.AreEqual("new name", persisted.GetPageProperty<string>("PageId2", "name"));
        }

        [TestMethod]
        public async Task PrimitivesCanBeInsertedEarlierWithCertainTolerance()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId2", "name", "new name") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            var secondBatch = PrimitivesRepository.Store[1];
            secondBatch.RowKey = ProjectLog.GenerateKey(secondBatch.GetDateTime().Add(-CloudProjectModelRepository.PrimitiveKeyTolerance));

            //assert
            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await persisted.EnsureLoaded();
            Assert.AreEqual("new name", persisted.GetPageProperty<string>("PageId2", "name"));
        }

        [TestMethod]
        public async Task AlreadyAppliedPrimitivesAlsoCanBeInWrongOrder()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId2", "name", "new name") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);
            await persisted.EnsureLoaded();

            var secondBatch = PrimitivesRepository.Store[1];
            secondBatch.RowKey = ProjectLog.GenerateKey(secondBatch.GetDateTime().Add(-CloudProjectModelRepository.PrimitiveKeyTolerance));

            var firstBatch = PrimitivesRepository.Store[0];
            firstBatch.RowKey = ProjectLog.GenerateKey(secondBatch.GetDateTime().AddMilliseconds(100));

            PrimitivesRepository.Store.Remove(firstBatch);
            PrimitivesRepository.Store.Remove(secondBatch);
            PrimitivesRepository.Store.Add(secondBatch);
            PrimitivesRepository.Store.Add(firstBatch);

            //assert
            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await persisted.EnsureLoaded();
            Assert.AreEqual("new name", persisted.GetPageProperty<string>("PageId2", "name"));
        }

        [TestMethod]
        public async Task IfPrimitivesCannotBeAppliedWarningShouldBeRecordedAndChangesMustBeLinked()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId2", "name", "new name") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);
            PrimitivesRepository.Store[1].FromVersion = "wrongVersion";

            AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId2", "name", "new name 2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);
            PrimitivesRepository.Store[2].FromVersion = "wrongVersionAgain";

            persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await persisted.EnsureLoaded();

            //assert            
            Assert.AreEqual("new name 2", persisted.FindPageById("PageId2").GetProp("name"), "Primitives should be 'glued' together even if version does not match");
            Logger.VerifyWarningWithContext("Could not build tail");
        }

        [TestMethod]
        public async Task NewSnapshopMustBePeriodicallyCreated()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("PageId1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act                        
            for (var i = 0; i < CloudProjectModelRepository.SnapshotFlushInterval; ++i)
            {
                var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

                AddChanges(persisted, new[] { PrimitiveStubs.PagePropertyChange("PageId1", "name", $"new name {i}") });
                await _designerService.UpdateProjectModel(persisted, Permission.Write);
            }

            model = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await model.EnsureLoaded();

            //assert            
            var lastSnapshot = SnapshotRepository.FindAll().Last();
            var lastBatch = PrimitivesRepository.FindAll().Last();
            Assert.AreEqual(lastBatch.ToVersion, lastSnapshot.EditVersion, "Wrong version of latest snapshot");

            var final = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await final.EnsureLoaded();
            Assert.AreEqual("new name " + (CloudProjectModelRepository.SnapshotFlushInterval - 1), final.GetPageProperty<string>("PageId1", "name"),
                "All primitives must be applied");
        }

        [TestMethod]
        public async Task WhenModelIsLoadedNewSnapshotMustBeCreated()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model, PrimitiveStubs.PageAdd("Page1"));
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId2") });
            await _designerService.UpdateProjectModel(persisted, Permission.Write);

            var latestBatch = PrimitivesRepository.FindAll().Last();
            var snapshot = await SnapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(model.CompanyId, model.Id));
            var snapshotString1 = await snapshot.GetContentAsString();
            var found1 = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            var found2 = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            //assert                        
            await found1.EnsureLoaded();
            await found2.EnsureLoaded();

            Assert.AreEqual(2, found1.Children.Count, "Primitives not applied at all");
            Assert.AreEqual(2, found2.Children.Count, "Seems like primitives are applied twice");

            var newSnapshot = await SnapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(model.CompanyId, model.Id));

            Assert.AreNotEqual(snapshotString1, await newSnapshot.GetContentAsString(), "Snapshot not updated");
            Assert.AreEqual(found2.EditVersion, newSnapshot.EditVersion, "Snapshot version incorrect");
            Assert.AreEqual(latestBatch.GetDateTime(), newSnapshot.DateTime, "Snapshot time is not updated");
        }

        [TestMethod]
        public async Task SavingRealtimeInfoMayFailWithUpdateConflict()
        {
            //arrange
            var model = new ProjectModel();            
            model = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId1") });

            string conflictedEditVersion = null;

            RealtimeRepository.EnableChangeTracking();
            Action<object> updated = null;
            updated = o =>
            {
                RealtimeRepository.Updated -= updated;
                RealtimeRepository.Rollback();

                var parallel = _designerService.ChangeProjectModel(Scope, CreateChange(model)).Result;
                AddChanges(parallel, new[] { PrimitiveStubs.PageAdd("PageId2") });
                _designerService.UpdateProjectModel(parallel, Permission.Write).Wait();
                conflictedEditVersion = parallel.EditVersion;

                throw new UpdateConflictException(new Exception());
            };
            RealtimeRepository.Updated += updated;

            var fromVersion = persisted.EditVersion;
            await _designerService.UpdateProjectModel(persisted, Permission.Write);
            var toVersion = persisted.EditVersion;

            //assert                                    
            Assert.AreNotEqual(fromVersion, toVersion, "Model must be updated with a new version");
            Assert.AreEqual(conflictedEditVersion, persisted.PreviousEditVersion, "Previous edit version must be correctly in case of conflict");

            var final = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await final.EnsureLoaded();
            Assert.AreEqual(2, final.Children.Count, "Wrong model");
        }

        [TestMethod]
        [ExpectedException(typeof(UpdateConflictException))]
        public async Task SavingRealtimeInfoShouldFailIfUpdateConflictNotResolved()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model);
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            var persisted = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            AddChanges(persisted, new[] { PrimitiveStubs.PageAdd("PageId1") });

            RealtimeRepository.EnableChangeTracking();
            Action<object> updated = null;
            updated = o =>
            {
                RealtimeRepository.Updated -= updated;
                RealtimeRepository.Rollback();

                var parallel = _designerService.ChangeProjectModel(Scope, CreateChange(model)).Result;
                AddChanges(parallel, new[] { PrimitiveStubs.PageAdd("PageId2") });
                _designerService.UpdateProjectModel(parallel, Permission.Write).Wait();

                RealtimeRepository.Updated += updated;

                throw new UpdateConflictException(new Exception());
            };
            RealtimeRepository.Updated += updated;

            await _designerService.UpdateProjectModel(persisted, Permission.Write);
        }

        [TestMethod]
        public async Task PrimitivesWithIdenticalUidsShouldBeIgnored()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model);
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            model = await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            var pageAdd = PrimitiveStubs.PageAdd("Page1");
            var elementNew = PrimitiveStubs.ElementNew(pageAdd.Node.Id);

            AddChanges(model, new[] { pageAdd, elementNew });
            await _designerService.UpdateProjectModel(model, Permission.Write);

            AddChanges(model, new[] { elementNew });
            await _designerService.UpdateProjectModel(model, Permission.Write);

            //assert
            model = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await model.EnsureLoaded();
            Assert.AreEqual(1, model.FindPageById("Page1").Children.Count, "Page must contain one instance of a new element");
        }

        [TestMethod]
        public async Task IfModelMigrationExistsSnapshotMustBeUpdated()
        {
            //arrange
            var model = new ProjectModel();
            AddChanges(model);
            model = await _designerService.ChangeProjectModel(Scope, model.Change);

            //act            
            await _designerService.ChangeProjectModel(Scope, CreateChange(model));

            var snapshot = await SnapshotRepository.FindByIdAsync(ProjectSnapshot.LatestId(model.CompanyId, model.Id));
            var content = JObject.Parse(await snapshot.GetContentAsString());
            content["props"]["modelVersion"] = 0;
            snapshot.ContentStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(snapshot.ContentStream, Defs.Encoding, 1024, true))
            {
                streamWriter.Write(content.ToString());
            }
            snapshot.ContentStream.Position = 0;
            await SnapshotRepository.UpdateAsync(snapshot);

            //assert
            model = await _designerService.ChangeProjectModel(Scope, CreateChange(model));
            await model.EnsureLoaded();
            Assert.AreEqual(ModelMigrator.MaxVersion, model.GetProp("modelVersion"));
        }

        [TestMethod]
        public async Task TwoCompaniesCanHaveProjectsWithSameId()
        {
            //act
            var model1 = new ProjectModel();
            model1.Change = CreateChange(model1, OwnerId, OwnerId, PrimitiveStubs.PageAdd("1"));
            model1 = await _designerService.ChangeProjectModel(Scope, model1.Change);
                     
            var model2 = new ProjectModel();
            model2.Change = CreateChange(model2, GuestId, GuestId, PrimitiveStubs.AppPropertyChanged("name", "newName"));
            model2 = await _designerService.ChangeProjectModel(Scope, model2.Change);

            //assert
            model1.Change = CreateChange(model1, OwnerId, OwnerId);
            model1 = await _designerService.ChangeProjectModel(Scope, model1.Change);
            await model1.EnsureLoaded();

            model2.Change = CreateChange(model2, GuestId, GuestId);
            model2 = await _designerService.ChangeProjectModel(Scope, model2.Change);
            await model2.EnsureLoaded();

            Assert.AreEqual(model1.Id, model2.Id);            
            Assert.AreEqual(1, model1.Children?.Count);
            Assert.AreEqual(null, model2.Children);

            Assert.AreEqual(null, model1.GetProp("name"));
            Assert.AreEqual("newName", model2.GetProp("name"));
        }

        private ProjectModelChange AddChanges(ProjectModel model, params RawPrimitive[] primitives)
        {
            model.Change = CreateChange(model, OwnerId, OwnerId, primitives);
            return model.Change;
        }

        private static ProjectModelChange CreateChange(ProjectModel model, string userId = OwnerId, string companyId = OwnerId, params RawPrimitive[] primitives)
        {
            return new ProjectModelChange
            {
                UserId = userId,
                CompanyId = companyId,
                ModelId = model.Id,
                PrimitiveStrings = primitives.Select(x => x.Write()).ToList()
            };
        }
    }
}
