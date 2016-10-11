using System.Collections.Generic;
using Carbon.Business.Domain;
using Carbon.Business.Domain.Marketing;

namespace Carbon.Business.Services
{
    public interface ICampaignRepository
    {
        IList<User> SelectUsers(Campaign campaign, int count);
    }
}