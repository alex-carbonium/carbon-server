using System.Collections.Generic;
using System.Runtime.Serialization;
using Carbon.Framework.Models;

namespace Carbon.Business.Domain
{
    [KnownType(typeof(ExternalProject))]
    public class ProjectFolder : IDomainObject<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Project> Projects { get; set; }
        //public IList<ProjectFolder> SubFolders { get; set; }

        public ProjectFolder()
        {                     
            Projects = new List<Project>();
        }        
        
        //public virtual ProjectFolder AddSubFolder(string name)
        //{
        //    var folder = new ProjectFolder();                        
        //    folder.Name = name;
        //    SubFolders.Add(folder);            
        //    return folder;
        //}

        //public virtual Project AddProject(string id, string name)
        //{
        //    var project = new Project
        //    {
        //        Id = id,
        //        Name = name
        //    };
        //    Projects.Add(project);
        //    return project;
        //}
    }
}
