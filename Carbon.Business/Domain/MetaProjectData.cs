namespace Sketch.Business.Domain
{
    public class MetaProjectData
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public string Theme { get; set; }
        public string Data { get; set; }
        public string DataFormat { get; set; }
        public string PageImages { get; set; }
        public string ProjectType { get; set; }
        public bool IsCopyOfDemo { get; set; }
        public bool IsOwnNewProject { get; set; }

        public MetaProjectData()
        {
            DataFormat = ProjectData.DataFormat.JSON;
            Version = 1;
        }

        public static MetaProjectData CreateFromProjectData(ProjectData data)
        {
            return new MetaProjectData
            {
                Id = data.Id,
                Name = data.Project.Name,
                Theme = data.Project.Theme,
                ProjectType = data.Project.ProjectType,
                Data = data.GetDataAsString(),
                DataFormat = data.Format
            };
        }
    }
}
