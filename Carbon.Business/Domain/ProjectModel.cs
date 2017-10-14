using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Business.Domain.DataTree;
using Carbon.Framework.Models;
using Newtonsoft.Json;

namespace Carbon.Business.Domain
{
    public class ProjectModel : DataNode, IDomainObject<string>
    {
        private Func<ProjectModel, Task> _lazyLoader;
        private List<Action<ProjectModel>> _upgradeRules;

        public ProjectModel(string id = "") : base(id, NodeType.App)
        {
            Loaded = true;
        }

        public override void Read(string json)
        {
            base.Read(json);
            Loaded = true;
            _lazyLoader = null;
        }

        public DataNode FindPageById(string id)
        {
            return Children.SingleOrDefault(page => page.Id == id);
        }

        public DataNode AddPage(string id)
        {
            return AddChild(id, NodeType.Page);
        }
        public void RemovePage(string id)
        {
            RemoveChild(id);
        }

        public IEnumerable<string> GetPageIds()
        {
            return Children.Select(x => x.Id);
        }

        public T GetPageProperty<T>(string id, string name)
        {
            return FindPageById(id).GetProp(name);
        }
        public void SetPageProperty(string id, string name, dynamic value)
        {
            FindPageById(id).SetProp(name, value);
        }

        public void Upgrade(Action<ProjectModel> action)
        {
            if (_upgradeRules == null)
            {
                _upgradeRules = new List<Action<ProjectModel>>();
            }
            _upgradeRules.Add(action);
        }

        public async Task<string> ToStringPretty()
        {
            await EnsureLoaded();
            return Write(Formatting.Indented);
        }
        public async Task<string> ToStringCompact()
        {
            await EnsureLoaded();
            return Write();
        }

        public string EditVersion
        {
            get { return GetProp("editVersion"); }
            set { SetProp("editVersion", value); }
        }
        public string PreviousEditVersion { get; set; }
        public int PageCount => Children.Count;

        public string CompanyId { get; set; }

        public ProjectModelChange Change { get; set; }
        public ProjectState State { get; set; }
        public bool Loaded { get; private set; }

        public static ProjectModel CreateNew(string companyId, string id)
        {
            return new ProjectModel(id) {CompanyId = companyId};
        }
        public static ProjectModel CreateLazy(string companyId, string id, Func<ProjectModel, Task> loader)
        {
            return new ProjectModel
            {
                Id = id,
                CompanyId = companyId,
                Loaded = false,
                _lazyLoader = loader
            };
        }

        public async Task EnsureLoaded()
        {
            if (!Loaded)
            {
                await _lazyLoader(this);
                Loaded = true;
                _lazyLoader = null;
            }
            if (_upgradeRules != null)
            {
                var rules = _upgradeRules;
                _upgradeRules = null;
                foreach (var rule in rules)
                {
                    rule(this);
                }
            }
        }

        public async Task<string> WriteAsync()
        {
            await EnsureLoaded();
            return Write();
        }
    }
}
