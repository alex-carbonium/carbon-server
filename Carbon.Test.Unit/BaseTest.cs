using System;
using System.Collections.Generic;
using Carbon.Business;
using Carbon.Business.Services;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.Framework.Util;
using Carbon.Owin.Common.Data;
using Carbon.Test.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Carbon.Console;

namespace Carbon.Test.Unit
{
    [TestClass]
    public abstract class BaseTest
    {
        public IDependencyContainer Container { get; private set; }
        public IDependencyContainer Scope { get; private set; }
        public AppSettings AppSettings
        {
            get { return Scope.Resolve<AppSettings>(); }
        }
        public Mock<IProjectRendersService> ProjectRendersService { get; set; }
        public Mock<ILogService> LogService { get; set; }
        public Mock<ILogger> Logger { get; set; }
        public InMemoryActorFabric ActorFabricStub { get; private set; }

        private Dictionary<string, object> _repositories;

        [TestInitialize]
        public virtual void Setup()
        {
            //AppSettingsMock.Setup(x => x.SiteHost).Returns("http://localhost:8010");
            //AppSettingsMock.Setup(x => x.Smtp.LogReaders).Returns("devs@carbonium.io");

            ProjectRendersService = new Mock<IProjectRendersService>();

            Logger = new Mock<ILogger>();
            LogService = new Mock<ILogService>();
            LogService.Setup(x => x.GetLogger()).Returns(Logger.Object);
            LogService.Setup(x => x.GetLogger()).Returns(Logger.Object);

            _repositories = new Dictionary<string, object>();

            ActorFabricStub = new InMemoryActorFabric();

            Container = new NinjectDependencyContainer(new StandardKernel());
            Container.RegisterInstance<Configuration>(new InMemoryConfiguration());
            Container.RegisterInstance<DataProvider>(new InMemoryDataProvider());
            Container.RegisterInstance<IActorFabric>(ActorFabricStub);
            DataLayerConfig.RegisterImplementation(Container);

            Container.RegisterInstance(LogService.Object);

            Scope = Container.BeginScope();
        }

        public InMemoryRepository<T> SetupRepository<T>()
        {
            var repo = new InMemoryRepository<T>();

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
            logger.Verify(x => x.Warning(It.Is<string>(s => s.Contains(message)), It.IsAny<IDependencyContainer>(), It.IsAny<string>()));
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