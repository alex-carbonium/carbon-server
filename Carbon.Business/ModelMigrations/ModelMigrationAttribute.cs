using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carbon.Business.ModelMigrations
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    internal sealed class  ModelMigrationAttribute : Attribute
    {
        public ModelMigrationAttribute(int version)
        {
            this.Version = version;
        }

        public int Version { get; private set; }
    }
}
