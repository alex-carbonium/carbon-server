using System.Dynamic;
using System.Linq;
using System.Threading;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Exceptions;
using Carbon.Data.Azure.Table;
using Carbon.Framework.Util;
using Carbon.Services;
using Carbon.Test.Common;
using Carbon.Test.Common.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Test.Integration.CloudPersistence
{
    [TestClass]
    public class TableRepositoryTests : IntegrationTestBase
    {
        private IDependencyContainer _container;
        private TableRepository<ProjectLog> _repository;
        private CloudTableClient _client;

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();

            var account = CloudStorageAccount.DevelopmentStorageAccount;
            _client = account.CreateCloudTableClient();
            var listOfTables = _client.ListTables();
            foreach (var table in listOfTables)
            {
                table.Delete();
            }

            _container = TestDependencyContainer.Configure();

            _repository = _container.Resolve<TableRepository<ProjectLog>>();            
        }

        [TestMethod]
        public void SimpleInsert()
        {   
            //arrange
            var primitives = new ProjectLog("c1", "10");
            primitives.UserId = "user";
            primitives.FromVersion = "aa";
            primitives.ToVersion = "bb";            

            //act
            _repository.Insert(primitives);            

            //assert
            dynamic key = new ExpandoObject();
            key.PartitionKey = primitives.PartitionKey;
            key.RowKey = primitives.RowKey;
            var persisted = _repository.FindById(key);

            Assert.AreEqual(primitives.PartitionKey, persisted.PartitionKey, "Wrong partition key");
            Assert.AreEqual(primitives.RowKey, persisted.RowKey, "Wrong row key key");
            Assert.AreEqual(primitives.UserId, persisted.UserId, "Wrong user id");
            Assert.AreEqual(primitives.FromVersion, persisted.FromVersion, "Wrong from version");
            Assert.AreEqual(primitives.ToVersion, persisted.ToVersion, "Wrong to version");            
        }

        [TestMethod]
        public void FindAllBy()
        {
            //arrange
            _repository.Insert(new ProjectLog("c1", "10"));            
            _repository.Insert(new ProjectLog("c2", "10"));

            //act   
            var spec = new FindByPartition<ProjectLog>("c1-10");
            var persisted = _repository.FindAllBy(spec);

            //assert            
            Assert.AreEqual(1, persisted.Count());
        }

        [TestMethod]
        public void UpdateShouldUseOptimisticLock()
        {
            //arrange
            var primitives = new ProjectLog("c1", "10");
            _repository.Insert(primitives);

            dynamic key = new ExpandoObject();
            key.PartitionKey = primitives.PartitionKey;
            key.RowKey = primitives.RowKey;

            var t1Found = new ManualResetEvent(false);                       
            var t1Update = new ManualResetEvent(false);            
            
            //act            
            var t1 = new Thread(() =>
            {                
                var persisted = _repository.FindById(key);
                t1Found.Set();
                persisted.UserId = "1";
                t1Update.WaitOne();
                try
                {
                    _repository.Update(persisted);
                    Assert.Fail("First thread must fail");
                }
                catch (UpdateConflictException)
                {                    
                }
            });

            var t2 = new Thread(() =>
            {             
                var persisted = _repository.FindById(key);
                persisted.UserId = "2";
                _repository.Update(persisted);
            });

            t1.Start();                        
            t1Found.WaitOne();

            t2.Start();
            t2.Join();
            
            t1Update.Set();

            //assert
            t1.Join();                
            var final = _repository.FindById(key);
            Assert.AreEqual("2", final.UserId, "Incorrect update");
        }
    }
}
