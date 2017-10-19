using Carbon.Business;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Business.Sync;
using Carbon.Console;
using Carbon.Framework.Extensions;
using Carbon.Framework.Logging;
using Carbon.Framework.Repositories;
using Carbon.StorageService.Dependencies;
using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Carbon.Tools.LogAnalysis
{
    public class LogPlayer
    {
        [Verb("testLog")]
        public class Options
        {
            [Option('c', "companyId", Required = true)]
            public string CompanyId { get; set; }

            [Option('m', "modelId", Required = true)]
            public string ModelId { get; set; }

            [Option('f', "filter", Default = null)]
            public string Filter { get; set; }

            [Option('t', "targetFolder", Default = ".")]
            public string TargetFolder { get; set; }
        }

        public async Task ReplayProjectLog(Options options)
        {
            var dir = LogDownloader.GetModelDir(options.TargetFolder, options.CompanyId, options.ModelId);
            if (!Directory.Exists(dir))
            {
                System.Console.WriteLine("Could not find model directory " + dir);
                return;
            }

            var logs = Directory.GetFiles(dir, "P_*.json")
                .Select(x =>
                {
                    var json = JObject.Parse(System.IO.File.ReadAllText(x));
                    var log = new ProjectLog();
                    log.PartitionKey = json.Value<string>("PartitionKey");
                    log.RowKey = json.Value<string>("RowKey");
                    log.FromVersion = json.Value<string>("FromVersion");
                    log.ToVersion = json.Value<string>("ToVersion");
                    log.UserId = json.Value<string>("UserId");
                    log.Primitives = json.Value<JArray>("Primitives").Select(p => p.ToString()).ToList();
                    return log;
                })
                .ToList();

            var state = JObject.Parse(System.IO.File.ReadAllText(Path.Combine(dir, "State.json")));

            var container = StorageDependencyConfig.Configure(c =>
            {
                c.RegisterInstance<Configuration>(new InMemoryConfiguration());
                c.RegisterTypeSingleton<DataProvider, InMemoryDataProvider>();
                c.RegisterTypeSingleton<ILogService, ConsoleLogService>();
                c.RegisterInstance<IActorFabric>(new InMemoryActorFabric());
            });
            var snapshotRepo = new InMemoryRepository<ProjectSnapshot>();
            container.RegisterInstance<IRepository<ProjectSnapshot>>(snapshotRepo);
            container.RegisterInstance<IRepository<ProjectState>>(new InMemoryRepository<ProjectState>());

            var model = new ProjectModel(options.ModelId);
            var snapshot = new ProjectSnapshot(ProjectSnapshot.LatestId(model.CompanyId, model.Id), DateTimeOffset.MinValue);
            snapshot.ContentStream = model.ToStream();
            snapshot.EditVersion = state.Value<string>("InitialVersion");
            await snapshotRepo.InsertAsync(snapshot);

            var repo = container.Resolve<CloudProjectModelRepository>();
            var tail = await repo.LoadModel(model, logs);

            System.IO.File.WriteAllText(Path.Combine(dir, "Model.json"), await model.ToStringPretty());

            PrintPrimitives(tail, options.Filter);
        }

        private void PrintPrimitives(IList<ProjectLog> logs, string filter)
        {
            foreach (var log in logs)
            {
                var primitives = PrimitiveFactory.CreateMany<DataNodeBasePrimitive>(log.Primitives);
                foreach (var p in primitives)
                {
                    if (!string.IsNullOrEmpty(filter) && !p.SourceString.Contains(filter))
                    {
                        continue;
                    }

                    PrintPrimitiveType(p);
                    System.Console.Write(" " + p.Path.Shortened());
                    PrintPrimitiveBody(p);
                }
            }
        }

        private void PrintPrimitiveBody(DataNodeBasePrimitive p)
        {
            switch (p.Type)
            {
                case PrimitiveType.DataNodeAdd:
                    var add = (DataNodeAddPrimitive)p;

                    string id = add.Node.Props["id"];
                    System.Console.Write(" " + id.Shorten());
                    System.Console.WriteLine(" Type=" + add.Node.Type);
                    PrintPrimitiveProps(add.Node.Props);
                    break;
                case PrimitiveType.DataNodeSetProps:
                    var setProps = (DataNodeSetPropsPrimitive)p;

                    System.Console.WriteLine();
                    PrintPrimitiveProps(setProps.Props);
                    break;
                case PrimitiveType.DataNodePatchProps:
                    var patchProps = (DataNodePatchPropsPrimitive)p;
                    System.Console.Write(" " + patchProps.PatchType);
                    System.Console.WriteLine(" " + patchProps.PropName);
                    System.Console.WriteLine("\t" + patchProps.Item.ToString(Formatting.None));
                    break;
                case PrimitiveType.DataNodeRemove:
                    var remove = (DataNodeRemovePrimitive)p;
                    System.Console.WriteLine(" " + remove.ChildId.Shorten());
                    break;
                default:
                    System.Console.WriteLine();
                    break;
            }
        }

        private static void PrintPrimitiveType(DataNodeBasePrimitive p)
        {
            var color = GetConsoleColor(p.Type);
            if (color.HasValue)
            {
                System.Console.ForegroundColor = color.Value;
            }
            System.Console.Write(p.Type.ToString());
            if (color.HasValue)
            {
                System.Console.ResetColor();
            }
        }

        private static void PrintPrimitiveProps(Dictionary<string, dynamic> props)
        {
            foreach (var prop in props)
            {
                System.Console.Write("\t" + prop.Key + " = ");
                var obj = prop.Value as JObject;
                if (obj != null)
                {
                    System.Console.WriteLine(obj.ToString(Formatting.None));
                }
                else
                {
                    System.Console.WriteLine(prop.Value == null ? "null" : prop.Value.ToString());
                }
            }
        }

        private static ConsoleColor? GetConsoleColor(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.DataNodeAdd:
                    return ConsoleColor.Green;
                case PrimitiveType.DataNodeRemove:
                    return ConsoleColor.Red;
                case PrimitiveType.DataNodeSetProps:
                    return ConsoleColor.Cyan;
                case PrimitiveType.DataNodePatchProps:
                    return ConsoleColor.Yellow;
            }
            return null;
        }
    }
}
