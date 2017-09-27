using Carbon.Framework.Models;
using System.Collections.Generic;

namespace Carbon.Business.Domain
{
    public class Project : IDomainObject<string>
    {
        private readonly HashSet<MirroringCode> _mirroringCodes = new HashSet<MirroringCode>(MirroringCode.UniqueComparer);
        private readonly HashSet<ProjectShareCode> _shareCodes = new HashSet<ProjectShareCode>(ProjectShareCode.UniqueComparer);

        public string Id { get; set; }

        public string Name { get; set; }
        public string Avatar { get; set; }
        //public virtual string MirroringCode { get; set; }

        public ICollection<MirroringCode> MirroringCodes => _mirroringCodes;
        public ICollection<ProjectShareCode> ShareCodes => _shareCodes;

        public void AddOrReplaceShareCode(ProjectShareCode code)
        {
            _shareCodes.Remove(code);
            _shareCodes.Add(code);
        }
    }

    public class ProjectSettings
    {
        public string Name { get; set; }
        public string Avatar { get; set; }
    }

    public class ProjectShareCode : IDomainObject<string>
    {
        public string Id { get; set; }
        public int Permission { get; set; }

        public static readonly IEqualityComparer<ProjectShareCode> UniqueComparer = new UniquePropertyComparer<ProjectShareCode>(x => x.Id);
    }
}
