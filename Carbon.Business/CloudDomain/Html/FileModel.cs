namespace Carbon.Business.CloudDomain.Html
{
    public class FileModel
    {
        public string Path { get; set; }
        public string Content { get; set; }
        public bool CacheForever { get; set; }
        public bool IsEntryFile { get; set; }
    }
}
