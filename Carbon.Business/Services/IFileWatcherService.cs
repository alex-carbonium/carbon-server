using System;
using System.Collections.Generic;
namespace Carbon.Business.Services
{
    public interface IFileWatcherService
    {
        void Reset(string rootPath);
        List<string> GetChnages(string rootPath);
    }
}
