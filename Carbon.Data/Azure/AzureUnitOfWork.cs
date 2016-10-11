using System;
using System.Collections.Generic;
using Carbon.Data.Azure.Blob;
using Carbon.Framework.Cloud;
using Carbon.Framework.Repositories;

namespace Carbon.Data.Azure
{   
    public class AzureUnitOfWork : ICloudUnitOfWork
    {
        private readonly IBlobRepositoryFactory _repositoryFactory;

        public AzureUnitOfWork(IBlobRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public void Dispose()
        {            
        }

        public IRepository<TEntity> Repository<TEntity>()
        {
            return _repositoryFactory.CreateBlobRepository<TEntity>();          
        }

        public void Flush()
        {            
        }

        public void Commit()
        {            
        }

        public void Rollback()
        {         
        }

        public bool IsDisposed { get; set; }

        public long ExecuteDirect(string code, IDictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }        
    }
}
