//using System.Collections.Generic;
//using System.Linq;
//using Carbon.Business.Domain;
//using Carbon.Business.Domain.Marketing;
//using Carbon.Business.Services;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using Sketch.Business.Domain;
//using Sketch.Business.Domain.Marketing;
//using Sketch.Business.Services;
//using Sketch.Framework.UnitOfWork;

//namespace Sketch.Test.Unit.Business
//{
//    [TestClass]
//    public class CampaignServiceTests : BaseTest
//    {
//        private Mock<IMailService> _mailService;
//        private Mock<ICampaignRepository> _campaignRepository;
//        private CampaignService _campaignService;

//        [TestInitialize]
//        public override void Setup()
//        {
//            base.Setup();

//            _mailService = new Mock<IMailService>();
//            _campaignRepository = new Mock<ICampaignRepository>();
//            _campaignService = new CampaignService(_mailService.Object, _campaignRepository.Object, UnitOfWorkStub, LogService.Object);
//        }

//        [TestMethod]
//        public void MultipleBatchesOnOneServer()
//        {
//            //arrange
//            var servers = new List<EmailServer>
//            {
//                new EmailServer {Limit = 30}
//            };
//            UnitOfWorkStub.InsertAll(servers);            

//            var email = CreateCampaign(30);

//            //act
//            _campaignService.Send(email);

//            //assert
//            Assert.IsTrue(servers.All(x => x.Usage == 30), "Server should be fully used");
//        }

//        [TestMethod]
//        public void MultipleBatchesEvenlyOnMultipleServers()
//        {
//            //arrange
//            var servers = new List<EmailServer>
//            {
//                new EmailServer {Limit = 10},
//                new EmailServer {Limit = 10},
//                new EmailServer {Limit = 10}
//            };
//            UnitOfWorkStub.InsertAll(servers);

//            var email = CreateCampaign(30);

//            //act
//            _campaignService.Send(email);

//            //assert
//            Assert.IsTrue(servers.All(x => x.Usage == 10), "All servers should be used");
//        }

//        [TestMethod]
//        public void MultipleServersAlreadyHaveUsage()
//        {
//            //arrange
//            var servers = new List<EmailServer>
//            {
//                new EmailServer {Limit = 10, Usage = 2},
//                new EmailServer {Limit = 10, Usage = 5},
//                new EmailServer {Limit = 10, Usage = 8}
//            };
//            UnitOfWorkStub.InsertAll(servers);

//            var email = CreateCampaign(15);

//            //act
//            _campaignService.Send(email);

//            //assert
//            Assert.IsTrue(servers.All(x => x.Usage == 10), "All servers should be used");
//        }

//        [TestMethod]
//        public void ServersMightHaveNotEnoughCapacity()
//        {
//            //arrange
//            var servers = new List<EmailServer>
//            {
//                new EmailServer {Limit = 10, Usage = 2},
//                new EmailServer {Limit = 10, Usage = 5},
//                new EmailServer {Limit = 10, Usage = 8}
//            };
//            UnitOfWorkStub.InsertAll(servers);

//            var email = CreateCampaign(16);

//            //act
//            _campaignService.Send(email);

//            //assert
//            Assert.IsTrue(servers.All(x => x.Usage == 10), "All servers should be used");
//            Logger.VerifyErrorWithContext("Not enough capacity.");
//        }

//        [TestMethod]
//        public void LargeCampaignScenarioWithLeftoverCapacity()
//        {
//            //arrange
//            var servers = new List<EmailServer>
//            {
//                new EmailServer {Limit = 8000, Usage = 2},
//                new EmailServer {Limit = 8000, Usage = 5},
//                new EmailServer {Limit = 8000, Usage = 8}
//            };
//            UnitOfWorkStub.InsertAll(servers);

//            var email = CreateCampaign(10000);

//            //act
//            _campaignService.Send(email);

//            //assert
//            Assert.AreEqual(10015, servers.Sum(x => x.Usage), "Wrong usage");            
//        }

//        private BulkEmail CreateCampaign(int n)
//        {
//            var users = new List<User>(n);
//            for (var i = 0; i < n; i++)
//            {                
//                users.Add(new User());
//            }

//            var campaign = new Campaign();
//            UnitOfWorkStub.Insert(campaign);

//            _campaignRepository
//                .Setup(x => x.SelectUsers(campaign, n))
//                .Returns(users);

//            var email = new BulkEmail
//            {
//                CampaignId = campaign.Id,
//                MessageCount = n
//            };            
//            return email;
//        }
//    }
//}
