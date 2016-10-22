using System.Fabric;
using System.IO;
using Carbon.Business;

namespace Carbon.Fabric.Common
{
    public class FabricDataProvider : DataProvider
    {
        private readonly ServiceContext _context;

        public FabricDataProvider(ServiceContext context)
        {
            _context = context;
        }

        public override string ResolvePath(string packageName, string file)
        {            
            return Path.Combine(GetPackage(packageName).Path, file);
        }

        public override string GetPackageVersion(string name)
        {
            return GetPackage(name).Description.Version;
        }

        private DataPackage GetPackage(string name)
        {
            return _context.CodePackageActivationContext.GetDataPackageObject(name);
        }        
    }
}