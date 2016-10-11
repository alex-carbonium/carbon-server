namespace Carbon.Business
{
    public abstract class DataProvider
    {
        public abstract string ResolvePath(string file);
        public abstract string GetPackageVersion();
    }
}