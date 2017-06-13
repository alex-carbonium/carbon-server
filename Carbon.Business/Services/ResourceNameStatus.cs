using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.CloudSpecifications;
using Carbon.Business.Domain;
using Carbon.Business.Exceptions;
using Carbon.Framework.Repositories;

namespace Carbon.Business.Services
{

    public enum ResourceNameStatus
    {
        Available,
        CanOverride,
        Taken
    }
}