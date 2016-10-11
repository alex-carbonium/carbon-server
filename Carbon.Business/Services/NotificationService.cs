using System;
using Sketch.Framework.UnitOfWork;
using Sketch.Business.Domain;
using Newtonsoft.Json;
using System.IO;

namespace Sketch.Business.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JsonSerializer _serializer = new JsonSerializer();
        private readonly ISecurity _security;

        public NotificationService(IUnitOfWork unitOfWork, ISecurity security)
        {
            _unitOfWork = unitOfWork;
            _security = security;
            //Needed for proper date conversion
            _serializer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            _serializer.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
        }
        public void NotifyProjectUsers(User user, Project project, string message, object data)
        {
            _security.AssertProjectPermission(project.Id, user, Permission.Read);

            Notification notification = new Notification
            {
                User = user,
                Project = project,
                Message = message,
                Data = JsonConvert.SerializeObject(data, Formatting.None, Defs.Config.JsonSerializerSettings),
                DateTime = DateTime.Now
            };
            var repository = _unitOfWork.Repository<Notification>();            
            repository.Insert(notification);
        }

        public void NotifyProjectUsers(User user, long projectId, string message, object data)
        {            
            var project = _unitOfWork.FindById<Project>(projectId);            
            NotifyProjectUsers(user, project, message, data);
        }

        public object DeserializeData(string text)
        {
            return _serializer.Deserialize(new JsonTextReader(new StringReader(text)));
        }
    }
}
