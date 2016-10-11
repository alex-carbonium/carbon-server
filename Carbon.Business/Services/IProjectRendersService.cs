using System.Collections.Generic;
using System.Drawing;

namespace Carbon.Business.Services
{
    public interface IProjectRendersService
    {
        void RenderProjectTile(string path, List<Image> images);
        void RenderProjectTile(string path, long projectId);
        void SaveProjectRenders(string pageImages, long projectId);
        void CopyRenders(long fromProjectId, long toProjectId);
        Image ImageFromDataURL(string dataURL);
        byte[] DataFromDataURL(string dataURL);
    }
}