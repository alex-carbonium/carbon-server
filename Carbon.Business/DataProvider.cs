namespace Carbon.Business
{
    public abstract class DataProvider
    {
        public abstract string ResolvePath(string packageName, string file);
        public abstract string GetPackageVersion(string name);
    }
}