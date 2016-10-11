using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Carbon.Framework.Logging;

namespace Carbon.Business.ModelMigrations
{
    public interface IModelMigration
    {
        void Up(ProjectModel model, ILogService logService);
    }
}
