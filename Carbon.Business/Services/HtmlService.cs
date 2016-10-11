using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Carbon.Business.CloudDomain.Html;
using Carbon.Framework.Cloud;
using Carbon.Framework.Cloud.Blob;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Business.Services
{
    public class HtmlFileCache
    {
        public ConcurrentDictionary<string, byte[]> CommonFiles { get; private set; }
        public ConcurrentDictionary<string, byte[]> ProjectTemplateFiles { get; private set; }

        public HtmlFileCache()
        {
            CommonFiles = new ConcurrentDictionary<string, byte[]>();
            ProjectTemplateFiles = new ConcurrentDictionary<string, byte[]>();
        }
    }

    public interface IHtmlService
    {
        string Export(string shareCode, List<FileModel> fileModels);
    }

    public class HtmlService : IHtmlService
    {
        private readonly ICloudUnitOfWorkFactory _uowFactory;
        private readonly HtmlFileCache _cache;

        public HtmlService(ICloudUnitOfWorkFactory uowFactory, HtmlFileCache cache)
        {
            _uowFactory = uowFactory;
            _cache = cache;
        }

        private void SaveProjectTemplate(IUnitOfWork uow, string shareCode)
        {            
            foreach (var file in _cache.ProjectTemplateFiles)
            {
                var entity = new ProjectFile(shareCode, file.Key);
                if (!uow.Exists<ProjectFile>(entity.Id))
                {
                    entity.SetContent(file.Value);
                    entity.AutoDetectContentType();                    
                    uow.Insert(entity);
                }
            }
        }

        public string Export(string shareCode, List<FileModel> fileModels)
        {
            using (var uow = _uowFactory.NewUnitOfWork())
            {
                uow.DeleteBy(new PrefixSpecification<ProjectFile>(shareCode));
                SaveProjectTemplate(uow, shareCode);

                var entryUri = string.Empty;
                foreach (var model in fileModels)
                {
                    var file = new ProjectFile(shareCode, model.Path);
                    file.SetContent(model.Content);
                    file.AutoDetectContentType();
                    uow.Insert(file);
                    if (model.IsEntryFile)
                    {
                        entryUri = file.Uri.AbsoluteUri;
                    }
                }

                uow.Commit();
                
                return entryUri;
            }
        }
    }
}
