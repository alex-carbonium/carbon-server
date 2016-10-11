using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
namespace Carbon.Framework.Cloud
{
    public interface ITableRepository<TEntity>
        where TEntity : ITableEntity, new()
    {
        void Delete(TEntity entity);
        IEnumerable<TEntity> FindAllBy(ITableRepositorySpecification<TEntity> specification);
        void Insert(TEntity entity);
        void CreateIfNotExists();
        void DeleteAll();
        void Update(TEntity entity);
    }
}
