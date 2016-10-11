using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbon.Framework.Cloud
{
    public interface ITableRepositorySpecification<TEntity>
        where TEntity : ITableEntity, new()
    {
        void Apply(TableQuery<TEntity> query);
    }
}
