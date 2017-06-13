using System;
using Carbon.Business.CloudDomain;
using Carbon.Data.Azure.Table;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Data.RepositoryImpl
{
    public class PrivateSharedPageRepository : TableRepository<SharedPage>, IPrivateSharedPageRepository
    {
        public PrivateSharedPageRepository(CloudTableClient client) : base(client)
        {
        }

        protected override string GetTableName(Type t)
        {
            return "CompanySharedPage";
        }
    }

    public class PublicSharedPageRepository : TableRepository<SharedPage>, IPublicSharedPageRepository
    {
        public PublicSharedPageRepository(CloudTableClient client) : base(client)
        {
        }

        protected override string GetTableName(Type t)
        {
            return "SharedPage";
        }
    }
}
