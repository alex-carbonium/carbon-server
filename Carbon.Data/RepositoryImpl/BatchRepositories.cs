using System.Collections.Concurrent;
using Carbon.Framework.Repositories;

namespace Carbon.Data.RepositoryImpl
{
    public class BatchRepositories
    {
        public static readonly ConcurrentBag<IRepository> All = new ConcurrentBag<IRepository>(); 
    }
}