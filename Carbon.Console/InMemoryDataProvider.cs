using Carbon.Business;
using System.Collections.Generic;
using System.IO;

namespace Carbon.Console
{
    public class InMemoryDataProvider : DataProvider
    {
        private readonly Dictionary<string, string> _folders;

        public InMemoryDataProvider()
        {
            _folders = new Dictionary<string, string>();
            _folders.Add(Defs.Packages.Data, "Data");
            _folders.Add(Defs.Packages.Client, @"..\..\..\..\..\carbon-ui");
        }

        public override string ResolvePath(string packageName, string file)
        {
            string folder;
            if (_folders.TryGetValue(packageName, out folder))
            {
                return Path.Combine(folder, file);
            }
            return file;
        }

        public override string GetPackageVersion(string name)
        {
            return "1.0.0";
        }
    }
}