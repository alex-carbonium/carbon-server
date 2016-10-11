namespace Carbon.Framework.Repositories
{
    public interface IRepositoryFactory<T>
    {
        IRepository<T> CreateRepository();
    }
}
