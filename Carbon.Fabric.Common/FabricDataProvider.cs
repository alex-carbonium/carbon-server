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

        public override string ResolvePath(string file)
        {            
            return Path.Combine(GetPackage().Path, file);
        }

        public override string GetPackageVersion()
        {
            return GetPackage().Description.Version;
        }

        private DataPackage GetPackage()
        {
            return _context.CodePackageActivationContext.GetDataPackageObject("Data");
        }        
    }
}