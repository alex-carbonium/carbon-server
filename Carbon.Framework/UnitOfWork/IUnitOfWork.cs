using System;
using System.Collections.Generic;
using Carbon.Framework.Repositories;

namespace Carbon.Framework.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity> Repository<TEntity>();

        void Flush();
        void Commit();
        void Rollback();        
    }
}