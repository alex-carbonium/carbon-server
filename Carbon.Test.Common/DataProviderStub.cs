using Carbon.Business;

namespace Carbon.Test.Common
{
    public class DataProviderStub : DataProvider
    {
        public override string ResolvePath(string packageName, string file)
        {
            return file;
        }

        public override string GetPackageVersion(string name)
        {
            return "1.0.0";
        }
    }
}