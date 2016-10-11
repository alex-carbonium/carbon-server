using Carbon.Framework.Cloud;
using Carbon.Framework.Repositories;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Data.UnitOfWorkImpl
{
    public class CompositeUnitOfWork : IUnitOfWork
    {                
        private readonly ICloudUnitOfWork _cloudUnitOfWork;

        public CompositeUnitOfWork(ICloudUnitOfWork cloudUnitOfWork)
        {            
            _cloudUnitOfWork = cloudUnitOfWork;
        }

        public void Dispose()
        {            
            _cloudUnitOfWork.Dispose();
        }

        public IRepository<TEntity> Repository<TEntity>()
        {
            if (DomainConfiguration.IsStoredInCloud(typeof(TEntity)))
            {
                return _cloudUnitOfWork.Repository<TEntity>();
            }
            return null;
        }

        public void Flush()
        {            
            _cloudUnitOfWork.Flush();
        }
        public void Commit()
        {         
            _cloudUnitOfWork.Commit();
        }

        public void Rollback()
        {         
            _cloudUnitOfWork.Commit();
        }                                        
    }
}
