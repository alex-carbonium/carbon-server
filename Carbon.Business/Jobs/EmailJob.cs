using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.JobScheduling;

namespace Carbon.Business.Jobs
{
    public class EmailJob : Job
    {        
        private readonly IMailService _mailService;
        private Email _email;

        public EmailJob(IMailService mailService)
        {
            _mailService = mailService;            
        }

        public override void Initialize(string parameters)
        {
            _email = Email.FromString<Email>(parameters);
        }

        public override void Execute(JobContext context)
        {
            _mailService.Send(_email);
        }
    }
}