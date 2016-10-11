using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Carbon.Data.Azure.Blob;
using Carbon.Framework.Cloud.Blob;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Services;
using Carbon.Test.Common;
using Carbon.Test.Common.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;

namespace Carbon.Test.Integration.CloudPersistence
{
    [TestClass]
    public class BlobRepositoryTests : IntegrationTestBase
    {
        private IDependencyContainer _container;
        private BlobRepository<TestEntity> _repository;

        [Container(Name = "testContainer", Type = ContainerType.Public)]
        public class TestEntity : BlobDomainObject
        {
            public TestEntity()
            {                
            }
            public TestEntity(string id)
            {
                Id = id;
                SetContent("content");
            }
        }

        [TestInitialize]
        public override void Setup()
        {
            base.Setup();
                        
            _container = TestDependencyContainer.Configure();
            _repository = _container.Resolve<BlobRepository<TestEntity>>();

            var client = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient();            
            foreach (var container in client.ListContainers())
            {
                container.Delete();
            }
        }

        [TestMethod]
        public void ShouldBePossibleToSaveAndRetrieveBlob()
        {
            //arrange
            const string id = "some-id";

            var entity = new TestEntity(id);
            entity.ContentType = "text/html";
            entity.SetContent("content");
            entity.Metadata["a"] = "b";

            //act
            _repository.Insert(entity);

            //assert            

            var persisted = _repository.FindById(id);

            Assert.IsNotNull(persisted, "Object must be persisted");
            Assert.AreEqual(entity.ContentType, persisted.ContentType, "Content type must be persisted");
            Assert.AreEqual(entity.Content.Length, persisted.Content.Length, "Content length must be persisted");
            Assert.AreEqual(entity.Metadata["a"], persisted.Metadata["a"], "Metadata must be persisted");
        }

        [TestMethod]
        public void ShouldBePossibleToSaveBlobUsingStream()
        {
            //arrange
            const string id = "some-id";
            const string content = "content";

            var entity = new TestEntity(id);

            //act            
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                entity.ContentStream = stream;
                _repository.Insert(entity);
            }

            //assert
            var persisted = _repository.FindById(id);

            Assert.IsNotNull(persisted, "Object must be persisted");
            Assert.AreEqual("content", persisted.GetContentAsString().Result, "Content must be persisted");
        }

        [TestMethod]
        public void ShouldBePossibleToDeleteBySpecification()
        {
            //arrange                                                
            _repository.Insert(new TestEntity("Prefix1/10"));
            _repository.Insert(new TestEntity("Prefix1/11"));
            _repository.Insert(new TestEntity("Prefix2/20"));

            //act            
            _repository.DeleteBy(new PrefixSpecification<TestEntity>("Prefix1"));

            //assert            
            var all = _repository.FindAll().ToList();

            Assert.AreEqual(1, all.Count, "One entity should be left");
            Assert.AreEqual("Prefix2/20", all[0].Id, "Wrong entity left after deletion");
        }

        [TestMethod]
        public void UpdateShouldUseLastWinStrategy()
        {
            //arrange
            var entity = new TestEntity("10");
            _repository.Insert(entity);

            var t1Found = new ManualResetEvent(false);
            var t1Update = new ManualResetEvent(false);

            //act            
            var t1 = new Thread(() =>
            {
                var persisted = _repository.FindById(entity.Id);
                t1Found.Set();
                persisted.SetContent("1");
                t1Update.WaitOne();
                _repository.Update(persisted);
            });

            var t2 = new Thread(() =>
            {
                var persisted = _repository.FindById(entity.Id);
                persisted.SetContent("2");
                _repository.Update(persisted);
            });

            t1.Start();
            t1Found.WaitOne();

            t2.Start();
            t2.Join();

            t1Update.Set();

            //assert
            t1.Join();
            var final = _repository.FindById(entity.Id);
            Assert.AreEqual("1", final.GetContentAsString().Result, "Incorrect update");
        }
    }
}
