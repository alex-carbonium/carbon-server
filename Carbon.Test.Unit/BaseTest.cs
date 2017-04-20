using System;
using System.Collections.Generic;
using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Framework.JobScheduling;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;

namespace Carbon.Test.Unit
{
    [TestClass]
    public abstract class BaseTest
    {
        public IDependencyContainer Container { get; private set; }
        public IDependencyContainer Scope { get; private set; }
        public Mock<IUnitOfWork> UnitOfWork { get; set; }
        public UnitOfWorkStub UnitOfWorkStub {
            get { return (UnitOfWorkStub) Scope.Resolve<IUnitOfWork>(); }
        }
        public AppSettings AppSettings
        {
            get { return Scope.Resolve<AppSettings>(); }
        }
        public Mock<IProjectRendersService> ProjectRendersService { get; set; }
        public Mock<IJobScheduler> JobSchedulerMock { get; set; }
        public IJobScheduler JobScheduler
        {
            get { return JobSchedulerMock.Object; }
        }
        public Mock<ILogService> LogService { get; set; }
        public Mock<ILogger> Logger { get; set; }
        public ActorFabricStub ActorFabricStub { get; private set; }

        private Dictionary<string, object> _repositories;

        [TestInitialize]
        public virtual void Setup()
        {
            UnitOfWork = new Mock<IUnitOfWork>();
            UnitOfWork.DefaultValue = DefaultValue.Mock;

            //AppSettingsMock.Setup(x => x.SiteHost).Returns("http://localhost:8010");
            //AppSettingsMock.Setup(x => x.Smtp.LogReaders).Returns("devs@carbonium.io");

            ProjectRendersService = new Mock<IProjectRendersService>();

            JobSchedulerMock = new Mock<IJobScheduler>();

            Logger = new Mock<ILogger>();
            LogService = new Mock<ILogService>();
            LogService.Setup(x => x.GetLogger()).Returns(Logger.Object);
            LogService.Setup(x => x.GetLogger()).Returns(Logger.Object);

            UnitOfWork.DefaultValue = DefaultValue.Mock;

            _repositories = new Dictionary<string, object>();

            ActorFabricStub = new ActorFabricStub();

            Container = new NinjectDependencyContainer(new StandardKernel());
            Container.RegisterInstance<Configuration>(new ConfigurationStub());
            Container.RegisterInstance<DataProvider>(new DataProviderStub());
            Container.RegisterInstance<IActorFabric>(ActorFabricStub);
            DataLayerConfig.RegisterImplementation(Container);

            Container.RegisterTypePerWebRequest<IUnitOfWork>(() =>
            {
                var uow = Scope.Resolve<UnitOfWorkStub>();
                uow.AllRepositories = _repositories;
                return uow;
            });
            Container.RegisterInstance(JobScheduler);
            Container.RegisterInstance(LogService.Object);

            Scope = Container.BeginScope();
        }

        public InMemoryRepository<T> SetupRepository<T>()
        {
            var repo = new InMemoryRepository<T>();

            UnitOfWork.Setup(x => x.Repository<T>()).Returns(repo);

            return repo;
        }

        public DateTime Date(int year, int month, int day)
        {
            return new DateTime(year, month, day);
        }
    }

    public static class MoqExtensions
    {
        public static void VerifyWarningWithContext(this Mock<ILogger> logger, string message)
        {
            logger.Verify(x => x.Warning(message, It.IsAny<IDependencyContainer>(), It.IsAny<string>()));
        }
        public static void VerifyNoWarning(this Mock<ILogger> logger)
        {
            logger.Verify(x => x.Warning(It.IsAny<string>(), It.IsAny<IDependencyContainer>(), It.IsAny<string>()),
                Times.Never());
        }
        public static void VerifyErrorWithContext(this Mock<ILogger> logger, string message)
        {
            logger.Verify(x => x.Error(message, It.IsAny<IDependencyContainer>(), It.IsAny<string>()));
        }
    }
}