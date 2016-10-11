using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Framework.Cloud.Blob;

namespace Carbon.Business.CloudDomain
{
    [Container(Name = "files", Type = ContainerType.Public)]
    public class File : BlobDomainObject
    {
        public File()
        {
        }

        public File(string prefix, string fileName)
        {
            Id = prefix + "/" + fileName;
            AutoDetectContentType();
        }
    }
}
