using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Carbon.Business.CloudDomain;
using Carbon.Framework.Repositories;

namespace Carbon.Business.Services
{
    public class ActivityService
    {
        private readonly IRepository<FeatureSubscription> _featureSubscriptionRepository;
        private readonly IRepository<BetaSubscription> _betaSubscriptionRepository;
        private readonly IActorFabric _actorFabric;
        private readonly IMailService _mailService;

        public ActivityService(
            IRepository<FeatureSubscription> featureSubscriptionRepository,
            IRepository<BetaSubscription> betaSubscriptionRepository,
            IMailService mailService,
            IActorFabric actorFabric)
        {
            _featureSubscriptionRepository = featureSubscriptionRepository;
            _betaSubscriptionRepository = betaSubscriptionRepository;
            _actorFabric = actorFabric;
            _mailService = mailService;
        }

        public void SubscribeForFeature(string companyId, string projectId, string feature)
        {
            this._featureSubscriptionRepository.InsertOrUpdate(new FeatureSubscription(companyId, projectId, feature));
        }

        public async Task SubscribeForBeta(string email)
        {
            if(string.IsNullOrEmpty(email))
            {
                return;
            }
            this._betaSubscriptionRepository.InsertOrUpdate(new BetaSubscription(email));

            await _mailService.Send("team@carbonium.io;alex@carbonium.io;denis@carbonium.io", "betaSignaup", new
            {
                Email = email
            });
        }
    }
}
