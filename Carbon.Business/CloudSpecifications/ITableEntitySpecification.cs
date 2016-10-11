using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

namespace Carbon.Business.CloudSpecifications
{
    public interface ITableEntitySpecification
    {
        IQueryable<ITableEntity> Apply(IQueryable<ITableEntity> query);
    }
}