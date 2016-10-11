//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Carbon.Business;
//using Carbon.Business.Domain;
//using Carbon.Business.Exceptions;
//using Carbon.Framework;
//using Carbon.Framework.Logging;
//using Carbon.Framework.Security;
//using Carbon.Framework.UnitOfWork;
//using Carbon.Framework.Util;

//namespace Carbon.Services.IdentityServer
//{
//    public class ClaimsProvider : IClaimsProvider
//    {
//        private readonly IDependencyContainer _container;
        
//        public ClaimsProvider(IDependencyContainer container)
//        {
//            _container = container;
//        }

//        public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, NameValueCollection request)
//        {
//            throw new NotSupportedException();
//        }

//        public Task<IEnumerable<Claim>> GetIdentityTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, bool includeAllIdentityClaims, ValidatedRequest request)
//        {
//            throw new NotSupportedException();
//        }

//        public async Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, NameValueCollection request)
//        {
//            var userId = Guid.Parse(subject.GetSubjectId());
//            using (var scope = _container.BeginScope())
//            {                
//                var uow = scope.Resolve<IUnitOfWork>();
//                var outputClaims = new List<Claim>(GetStandardSubjectClaims(subject));

//                var user = uow.FindSingleBy(User.FindBySID(userId));                
//                string role = null;
//                if (user == null)
//                {
//                    throw new DeletedUserException(userId.ToString());
//                }
//                if (user.IsGuest)
//                {
//                    role = Business.Defs.Roles.Guest;
//                }
//                else
//                {
//                    try
//                    {                       
//                        var userManager = scope.Resolve<ApplicationUserManager>();
//                        var applicationUser = await userManager.FindByEmailAsync(user.Email);                        
//                        if (applicationUser != null && await userManager.IsInRoleAsync(applicationUser.Id, Business.Defs.Roles.Administrators))
//                        {
//                            role = Business.Defs.Roles.Administrators;
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        scope.Resolve<ILogService>().GetLogger(this).Error("Failed to check if user {0} is admin. {1}", user.Id, ex);
//                    }
//                }

//                if (!string.IsNullOrEmpty(role))
//                {
//                    outputClaims.Add(new Claim(ClaimTypes.Role, role));
//                }

//                return outputClaims;   
//            }            
//        }
        
//        protected virtual IEnumerable<Claim> GetStandardSubjectClaims(ClaimsPrincipal subject)
//        {
//            var claims = new List<Claim>
//            {
//                subject.FindFirst(Constants.ClaimTypes.Subject),
//                subject.FindFirst(Constants.ClaimTypes.AuthenticationMethod),
//                subject.FindFirst(Constants.ClaimTypes.AuthenticationTime),
//                subject.FindFirst(Constants.ClaimTypes.IdentityProvider),
//                subject.FindFirst(Constants.ClaimTypes.Name)
//            };

//            return claims;
//        }        

//        public Task<IEnumerable<Claim>> GetAccessTokenClaimsAsync(ClaimsPrincipal subject, Client client, IEnumerable<Scope> scopes, ValidatedRequest request)
//        {
//            return GetAccessTokenClaimsAsync(subject, client, scopes, (NameValueCollection)null);
//        }        
//    }
//}