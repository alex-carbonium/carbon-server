using Carbon.Business.Services;
using Carbon.Owin.Common.Security;
using Carbon.Owin.Common.WebApi;

namespace Carbon.StorageService.Controllers
{    
    public class ProjectDataController : AuthorizedApiController
    {
        private readonly ProjectModelService _projectModelService;        

        public ProjectDataController(ProjectModelService projectModelService)             
        {
            _projectModelService = projectModelService;
        }
        

        //[HttpGet]
        //public async Task<HttpResponseMessage> ExportPackage(long id)
        //{
        //    var user = GetLoggedInUser();
        //    var project = _designerService.FindProject(user, id, Permission.Export);
        //    var exported = await _designerService.ExportProjectPackage(user, id);
        //    return ZippedContent(exported, project.Name + ".zip");
        //}

        //[HttpPost]
        //public async Task<HttpResponseMessage> ImportPackage()
        //{
        //    try
        //    {
        //        var provider = new MultipartMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        if (provider.Contents.Count == 0)
        //        {
        //            return Html("<script>window.parent.postMessage(JSON.stringify({success: false, error: 'File is not received.'}), '*')</script>");
        //        }

        //        var user = GetLoggedInUser();

        //        foreach (var file in provider.Contents)
        //        {
        //            var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');                    
                    
        //            if (Path.GetExtension(fileName) == ".zip")
        //            {
        //                await _designerService.ImportProjectPackage(user, await file.ReadAsStreamAsync());
        //            }
        //            else
        //            {
        //                using (var reader = new StreamReader(await file.ReadAsStreamAsync(), _designerService.Encoding))
        //                {
        //                    await _designerService.ImportProject(user, reader.ReadToEnd());
        //                }
        //            }
        //        }                                                                
        //    }
        //    catch (Exception e)
        //    {
        //        LogService.GetLogger(this).ErrorWithContext(e, Scope);
        //        return Html("<script>window.parent.postMessage(JSON.stringify({success: false, error: 'You have selected an invalid file type.'}), '*')</script>");
        //    }

        //    return Html("<script>window.parent.postMessage(JSON.stringify({success: true}), '*')</script>");
        //}        

        //[HttpPost]
        //public virtual async Task<IHttpActionResult> Duplicate(long id)
        //{
        //    await _designerService.DuplicateProject(GetLoggedInUser(), id);
        //    return Ok();
        //}
    }
}