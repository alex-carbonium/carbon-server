using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Data.Azure.Table;
using Carbon.Framework.Util;
using Carbon.Services;
using Carbon.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Test.Integration.CloudPersistence
{
    [TestClass]
    public class FatEntitityRepositoryTests : IntegrationTestBase
    {
        private IDependencyContainer _container;
        private FatEntityRepository<ProjectLog> _repository;
        private CloudTableClient _client;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            //testing against an online account since limitations are different in the emulator
            var account = CreateTestStorageAccount(canUseLocal: false);
            _client = account.CreateCloudTableClient();
            var listOfTables = _client.ListTables();
            foreach (var table in listOfTables)
            {
                table.Delete();
            }

            _container = TestDependencyContainer.Configure();
            _container.RegisterInstance(account);
            _container.RegisterInstance(_client);

            _repository = _container.Resolve<FatEntityRepository<ProjectLog>>();
            _repository.CustomTableName = "T" + Guid.NewGuid().ToString().Replace("-", string.Empty);
        }

        [TestMethod]
        public void SimpleInsert()
        {   
            //arrange
            var primitives = new ProjectLog("c1", "10");
            primitives.UserId = "user";
            primitives.FromVersion = "aa";
            primitives.ToVersion = "bb";
            primitives.Primitives = new List<string>{"test"};

            //act
            _repository.Insert(primitives);            

            //assert
            var persisted = _repository.FindAll().Single();            

            Assert.AreEqual(primitives.PartitionKey, persisted.PartitionKey, "Wrong partition key");
            Assert.AreEqual(primitives.RowKey, persisted.RowKey, "Wrong row key key");
            Assert.AreEqual(primitives.UserId, persisted.UserId, "Wrong user id");
            Assert.AreEqual(primitives.FromVersion, persisted.FromVersion, "Wrong from version");
            Assert.AreEqual(primitives.ToVersion, persisted.ToVersion, "Wrong to version");
            Assert.AreEqual(
                string.Join(",", primitives.Primitives), 
                string.Join(", ", persisted.Primitives), 
                "Wrong data");
        }
        
        [TestMethod]
        public void InsertExceedingPropertyLimit()
        {
            //arrange
            var primitives = new ProjectLog("c1", "10");
            primitives.UserId = "userId";
            primitives.FromVersion = "from";
            primitives.ToVersion = "to";
            primitives.Primitives = new List<string> { RandomString(64*1024+1) };

            //act
            _repository.Insert(primitives);

            //assert
            var persisted = _repository.FindAll().Single();            
            Assert.AreEqual(string.Join(",", primitives.Primitives), 
                string.Join(",", persisted.Primitives));
        }

        [TestMethod]
        public void InsertExceedingEntitySizeLimit()
        {
            //arrange
            var primitives = new ProjectLog("c1", "10");
            primitives.UserId = "userId";
            primitives.FromVersion = "from";
            primitives.ToVersion = "to";
            primitives.Primitives = new List<string> { Encoding.UTF8.GetString(FindEntityPayload(FatEntity.MaxRowSize + 1000))};

            //act
            _repository.Insert(primitives);

            //assert
            Assert.AreEqual(2, _client.GetTableReference(_repository.GetTableName()).ExecuteQuery(new TableQuery()).Count(), 
                "Entity should be split");

            var persisted = _repository.FindAll().Single();
            Assert.AreEqual(
                string.Join(",", primitives.Primitives),
                string.Join(", ", persisted.Primitives),
                "Wrong data");
        }

        [TestMethod]
        public async Task InsertExceedingBatchPayloadLimit()
        {
            //arrange            
            var recordPayloadSize = FatEntityRepository<ProjectLog>.MaxBatchPayload / 4;
            var payload = FindEntityPayload(recordPayloadSize);
            var entities = new List<FatEntity>();
            var position = 0;
            for (var i = 0; i < 4; ++i)
            {
                var entity = new FatEntity("P", i.ToString());
                entity.Fill(payload, ref position);
                entities.Add(entity);
            }

            var extra = new FatEntity("P", "extra");
            extra.Fill(new byte[1], ref position);
            entities.Add(extra);

            //act
            await _repository.ExecuteBatch(entities, (o, e) => o.Insert(e.WrappedEntity), async: true);

            //assert
            Assert.AreEqual(5, _client.GetTableReference(_repository.GetTableName()).ExecuteQuery(new TableQuery()).Count(),
                "Entities should be written");                        
        }

        [TestMethod]
        public async Task InsertExceedingBatchCountLimit()
        {
            //arrange                                    
            var entities = new List<FatEntity>();
            var count = FatEntityRepository<ProjectLog>.MaxBatchOperations + 1;
            var position = 0;
            for (var i = 0; i < count; ++i)
            {
                var entity = new FatEntity("P", i.ToString());
                entity.Fill(new byte[1], ref position);
                entities.Add(entity);
            }

            //act
            await _repository.ExecuteBatch(entities, (o, e) => o.Insert(e.WrappedEntity), async: true);

            //assert
            Assert.AreEqual(count, _client.GetTableReference(_repository.GetTableName()).ExecuteQuery(new TableQuery()).Count(),
                "Entities should be written");
        }

        [TestMethod]
        public async Task SelectExceedingReadCountLimit()
        {
            //arrange                                    
            var entities = new List<FatEntity>();
            var count = 1001;
            var position = 0;
            for (var i = 0; i < count; ++i)
            {
                var entity = new FatEntity("P", i.ToString());
                entity.Fill(new byte[1], ref position);
                entities.Add(entity);
            }

            //act
            await _repository.ExecuteBatch(entities, (o, e) => o.Insert(e.WrappedEntity), async: true);

            //assert
            Assert.AreEqual(count, _client.GetTableReference(_repository.GetTableName()).ExecuteQuery(new TableQuery()).Count(),
                "Entities should be written");
        }

        [TestMethod]
        public void PredicateSpecificationsMustBeSupported()
        {
            //arrange                                    
            var entity1 = new ProjectLog("c1", "10");
            var entity2 = new ProjectLog("c1", "20");
            entity1.UserId = "userId";
            entity1.FromVersion = "from";
            entity1.ToVersion = "to";

            entity2.UserId = "userId";
            entity2.FromVersion = "from";
            entity2.ToVersion = "to";

            _repository.Insert(entity1);
            _repository.Insert(entity2);

            //act
            var results = _repository.FindAllBy(new FindByPartition<ProjectLog>("c1-10"));

            //assert
            Assert.AreEqual("c1-10", results.Single().PartitionKey);
        }

        private byte[] FindEntityPayload(int limit)
        {
            var buffer = new byte[limit];            
            var entity = new FatEntity("PK", "RK");
            var position = 0;
            entity.Fill(buffer, ref position);
            return new byte[limit*2 - entity.GetPayloadSize()];
        }

        private string RandomString(int size)
        {
            var random = new Random();
            var builder = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(random.Next(0x4e00, 0x4f80));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
