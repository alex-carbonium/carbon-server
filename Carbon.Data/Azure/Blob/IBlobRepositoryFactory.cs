using Carbon.Framework.Repositories;

namespace Carbon.Data.Azure.Blob
{
    public interface IBlobRepositoryFactory
    {
        IRepository<T> CreateBlobRepository<T>();
    }
}
