using System;

namespace Carbon.Business.Exceptions
{
    public class ProjectStorageChangedException : Exception
    {
        public ProjectStorageChangedException(string currentStorage, string newStorage)
            : base(string.Format("currentStorage: {0}, newStorage: {1}", currentStorage, newStorage))
        {
            NewStorage = newStorage;
        }

        public string NewStorage { get; set; }
    }
}
