using System.IO;
using Carbon.Business;

namespace Carbon.Test.Common
{    
    public class TestAppSettings : AppSettings
    {
        //only supports the case when tests are deployed into TestResults\RunName\Out
        private static string GetRootDirectory()
        {
            var parent = Directory.GetParent(Directory.GetCurrentDirectory());
            parent = parent.Parent;
            parent = parent.Parent;
            return parent.FullName;            
        }

        public override string GetPhysicalPath(string virtualOrPhysicalPath)
        {
            if (virtualOrPhysicalPath.StartsWith("/"))
            {
                virtualOrPhysicalPath = virtualOrPhysicalPath.Substring(1);
            }
            return Path.Combine(GetRootDirectory(), virtualOrPhysicalPath.Replace("/", "\\"));
        }

        private static void CleanupDir(string path)
        {
            var dir = new DirectoryInfo(path);

            if (dir.Exists)
            {
                foreach (var file in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
                }
                foreach (var file in dir.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    file.Delete();
                }
                foreach (var subDir in dir.GetDirectories())
                {
                    subDir.Delete(true);
                }                
            }
        }

        public TestAppSettings(Configuration configuration, DataProvider dataProvider) : base(configuration, dataProvider)
        {
        }
    }
}
