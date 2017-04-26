using System;
using System.Collections.Generic;
using System.Linq;
using Carbon.Business.Domain;
using Carbon.Business.Domain.Marketing;
using Carbon.Framework.Logging;
using Carbon.Framework.UnitOfWork;

namespace Carbon.Business.Services
{
    public class CampaignService
    {
        private readonly IMailService _mailService;
        private readonly ICampaignRepository _campaignRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public CampaignService(IMailService mailService, ICampaignRepository campaignRepository, IUnitOfWork unitOfWork, ILogService logService)
        {
            _mailService = mailService;
            _campaignRepository = campaignRepository;
            _unitOfWork = unitOfWork;
            _logService = logService;
        }

        public void Send(BulkEmail email)
        {
            const int desiredBatchSize = 1000;

            var servers = _unitOfWork.FindAll<EmailServer>().ToList();
            foreach (var server in servers)
            {
                //server.Usage = 0;
            }

            servers = servers.OrderBy(x => x.Usage).ToList();
            var serverNo = -1;

            var campaign = _unitOfWork.FindById<Campaign>(email.CampaignId);
            var allUsers = email.TestMode ?
                new List<User>
                {
                    _unitOfWork.FindAll<User>().Single(x => x.Email == "dennis@carbonium.io"),
                    _unitOfWork.FindAll<User>().Single(x => x.Email == "alex@carbonium.io"),
                }
                : _campaignRepository.SelectUsers(campaign, email.MessageCount);
            if (allUsers.Count == 0)
            {
                _logService.GetLogger().Error("No more users.");
                return;
            }

            var allRecipients = allUsers.Select(x => x.Email).ToList();
            var allSubstitutions = BuildSubstitutions(allUsers);

            for (var i = 0; i < allRecipients.Count;)
            {
                ++serverNo;
                if (serverNo == servers.Count)
                {
                    serverNo = 0;
                }
                var server = servers[serverNo];
                var possibleBatchSize = Math.Min(desiredBatchSize, allRecipients.Count - i);
                var actualBatchSize = Math.Min(possibleBatchSize, server.Limit - server.Usage);

                if (actualBatchSize == 0)
                {
                    _logService.GetLogger().Error("Not enough capacity.");
                    break;
                }

                var batchEmail = Email.FromString<BulkEmail>(email.ToString());

                var batchRecipients = allRecipients.GetRange(i, actualBatchSize);
                var batchSubstitutions = allSubstitutions.ToDictionary(
                    s => s.Key,
                    s => s.Value.GetRange(i, actualBatchSize));

                _mailService.SendBulkEmail(server, batchEmail, batchRecipients, batchSubstitutions);
                server.Usage += actualBatchSize;
                i += actualBatchSize;
            }
        }

        private static Dictionary<string, List<string>> BuildSubstitutions(IEnumerable<User> users)
        {
            var substitutions = new Dictionary<string, List<string>>();
            var names = new List<string>();
            var sids = new List<string>();

            foreach (var user in users)
            {
                var name = "there";
                if (!string.IsNullOrEmpty(user.FriendlyName))
                {
                    var space = user.FriendlyName.IndexOf(" ", StringComparison.Ordinal);
                    name = space >= 0 ? user.FriendlyName.Substring(0, space) : user.FriendlyName;
                }
                names.Add(name);
                sids.Add(user.Id);
            }

            substitutions.Add(":name", names);
            substitutions.Add(":sid", sids);

            return substitutions;
        }
    }
}
