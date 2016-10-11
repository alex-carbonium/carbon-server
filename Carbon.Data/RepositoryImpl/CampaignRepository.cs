//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Carbon.Business.Domain;
//using Carbon.Business.Domain.Marketing;
//using Carbon.Business.Services;
//using Carbon.Framework.UnitOfWork;
//using NHibernate.Linq;

//namespace Carbon.Data.RepositoryImpl
//{
//    public class CampaignRepository : NativeDbRepository, ICampaignRepository
//    {
//        public CampaignRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
//        {            
//        }

//        public IList<User> SelectUsers(Campaign campaign, int count)
//        {
//            var sessionId = Guid.NewGuid();
//            var subscribtionFilter = campaign.IncludeUnsubsribedUsers ? "0, 1" : "1";

//            var insertSql = string.Format(@"
//insert into SessionBatch (id, SessionId)
//select top {0} Id, '{1}' from [User] u 
//where not exists (select * from CampaignNotification cn where cn.user_id = u.id and cn.campaign_id = :campaignId)
//	and SubscribeForUpdates in ({2})
//    and Email is not null 
//    and Email != ''
//order by Id desc;

//insert into CampaignNotification(campaign_id, user_id)
//select :campaignId, Id
//from SessionBatch b
//where SessionId = :sessionId;", count, sessionId, subscribtionFilter);

//            Session.CreateSQLQuery(insertSql)                
//                .SetParameter("sessionId", sessionId)
//                .SetParameter("campaignId", campaign.Id)
//                .ExecuteUpdate();

//            var users = (
//                from u in Session.Query<User>()                
//                from b in Session.Query<SessionBatch>()
//                where u.Id == b.Id && b.SessionId == sessionId 
//                select u)
//                .ToList();

//            Session.CreateSQLQuery("delete from SessionBatch where SessionId = :sessionId")
//                .SetParameter("sessionId", sessionId)
//                .ExecuteUpdate();

//            return users;
//        }        
//    }
//}