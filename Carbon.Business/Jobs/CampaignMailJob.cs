using Carbon.Business.Domain;
using Carbon.Business.Services;
using Carbon.Framework.JobScheduling;

namespace Carbon.Business.Jobs
{
    public class CampaignMailJob : Job
    {
        private readonly CampaignService _campaignService;        
        private BulkEmail _email;

        public CampaignMailJob(CampaignService campaignService)
        {
            _campaignService = campaignService;           
        }

        public override void Initialize(string parameters)
        {
            _email = Email.FromString<BulkEmail>(parameters);
        }

        public override void Execute(JobContext context)
        {
            _campaignService.Send(_email);
        }
    }
}