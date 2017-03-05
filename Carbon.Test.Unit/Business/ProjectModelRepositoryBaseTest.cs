using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.Repositories;
using Carbon.Test.Common;

namespace Carbon.Test.Unit.Business
{
    public abstract class ProjectModelRepositoryBaseTest : BaseTest
    {
        protected InMemoryRepository<ProjectState> RealtimeRepository;
        protected InMemoryRepository<ProjectSnapshot> SnapshotRepository;
        protected InMemoryRepository<ProjectLog> PrimitivesRepository;

        public override void Setup()
        {
            base.Setup();

            RealtimeRepository = SetupInMemoryRepository<ProjectState>();
            SnapshotRepository = new ProjectSnaphotRepositoryStub();
            PrimitivesRepository = SetupInMemoryRepository<ProjectLog>();

            Container.RegisterInstance<IRepository<ProjectSnapshot>>(SnapshotRepository);
            Container.RegisterInstance<FontManager>(new TestFontManager());

            SetupInMemoryRepository<ActiveProject>();
        }

        protected InMemoryRepository<T> SetupInMemoryRepository<T>()
        {
            var repo = new InMemoryRepository<T>();
            Container.RegisterInstance<IRepository<T>>(repo);
            return repo;
        }
    }
}