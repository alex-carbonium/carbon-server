using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Carbon.Business.Domain;
using Carbon.Framework.Logging;

namespace Carbon.Business.ModelMigrations
{
    public class ModelMigrator
    {
        private static List<Tuple<int,IModelMigration>> _migrations;
        public static int MaxVersion { get; private set; }

        static ModelMigrator()
        {
            _migrations = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttributes<ModelMigrationAttribute>().Any())
                .Select(t => new Tuple<int, IModelMigration>(t.GetCustomAttribute<ModelMigrationAttribute>().Version, (IModelMigration)Activator.CreateInstance(t)))
                .OrderBy(t=>t.Item1)
                .ToList();

            MaxVersion = _migrations.Count > 0 ? _migrations.Max(t => t.Item1) : 0;
        }

        public static bool Run(ProjectModel model, ILogService logService)
        {
            var currentVersion = (int?)model.GetProp("modelVersion");
            
            if (!currentVersion.HasValue)
            {
                currentVersion = 0;
            }

            if (currentVersion >= MaxVersion)
            {
                return false;
            }

            var newVersion = currentVersion.Value;

            for (var i = 0; i < _migrations.Count; ++i)
            {
                var migration = _migrations[i];
                if (newVersion < migration.Item1)
                {
                    migration.Item2.Up(model, logService);
                    newVersion = migration.Item1;
                }
            }

            model.SetProp("modelVersion", newVersion);

            return currentVersion != newVersion;
        }
    }
}
