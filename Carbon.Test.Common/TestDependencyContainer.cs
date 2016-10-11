using Carbon.Business;
using Carbon.Framework.Util;
using Carbon.Services;

namespace Carbon.Test.Common
{
    public class TestDependencyContainer
    {
        public static IDependencyContainer Configure()
        {
            return ServiceDependencyConfiguration.Configure(x =>
            {
                x.RegisterInstance<Configuration>(new ConfigurationStub());
                x.RegisterInstance<DataProvider>(new DataProviderStub());
            });
        }
    }
}
