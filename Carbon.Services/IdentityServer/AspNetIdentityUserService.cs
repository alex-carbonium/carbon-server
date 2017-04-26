/*
 * Copyright 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core;
using IdentityModel;
using IdentityServer3.Core.Services.Default;

namespace Carbon.Services.IdentityServer
{
    public class AspNetIdentityUserService<TUser, TKey> : UserServiceBase
        where TUser : class, Microsoft.AspNet.Identity.IUser<TKey>, new()
        where TKey : IEquatable<TKey>
    {
        public string DisplayNameClaimType { get; set; }
        public bool EnableSecurityStamp { get; set; }

        protected readonly Microsoft.AspNet.Identity.UserManager<TUser, TKey> userManager;

        protected readonly Func<string, TKey> ConvertSubjectToKey;

        public AspNetIdentityUserService(Microsoft.AspNet.Identity.UserManager<TUser, TKey> userManager, Func<string, TKey> parseSubject = null)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");

            this.userManager = userManager;

            if (parseSubject != null)
            {
                ConvertSubjectToKey = parseSubject;
            }
            else
            {
                var keyType = typeof (TKey);
                if (keyType == typeof (string)) ConvertSubjectToKey = subject => (TKey) ParseString(subject);
                else if (keyType == typeof (int)) ConvertSubjectToKey = subject => (TKey) ParseInt(subject);
                else if (keyType == typeof (uint)) ConvertSubjectToKey = subject => (TKey) ParseUInt32(subject);
                else if (keyType == typeof (long)) ConvertSubjectToKey = subject => (TKey) ParseLong(subject);
                else if (keyType == typeof (Guid)) ConvertSubjectToKey = subject => (TKey) ParseGuid(subject);
                else
                {
                    throw new InvalidOperationException("Key type not supported");
                }
            }

            EnableSecurityStamp = true;
        }

        object ParseString(string sub)
        {
            return sub;
        }
        object ParseInt(string sub)
        {
            int key;
            if (!Int32.TryParse(sub, out key)) return 0;
            return key;
        }
        object ParseUInt32(string sub)
        {
            uint key;
            if (!UInt32.TryParse(sub, out key)) return 0;
            return key;
        }
        object ParseLong(string sub)
        {
            long key;
            if (!Int64.TryParse(sub, out key)) return 0;
            return key;
        }
        object ParseGuid(string sub)
        {
            Guid key;
            if (!Guid.TryParse(sub, out key)) return Guid.Empty;
            return key;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext ctx)
        {
            var subject = ctx.Subject;
            var requestedClaimTypes = ctx.RequestedClaimTypes;

            if (subject == null) throw new ArgumentNullException("subject");

            TKey key = ConvertSubjectToKey(subject.GetSubjectId());
            var acct = await userManager.FindByIdAsync(key);
            if (acct == null)
            {
                throw new ArgumentException("Invalid subject identifier");
            }

            var claims = await GetClaimsFromAccount(acct);
            if (requestedClaimTypes != null && requestedClaimTypes.Any())
            {
                claims = claims.Where(x => requestedClaimTypes.Contains(x.Type));
            }

            ctx.IssuedClaims = claims;
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsFromAccount(TUser user)
        {
            var claims = new List<Claim>{
                new Claim(Constants.ClaimTypes.Subject, user.Id.ToString()),
                new Claim(Constants.ClaimTypes.PreferredUserName, user.UserName),
            };

            if (userManager.SupportsUserEmail)
            {
                var email = await userManager.GetEmailAsync(user.Id);
                if (!String.IsNullOrWhiteSpace(email))
                {
                    claims.Add(new Claim(Constants.ClaimTypes.Email, email));
                    var verified = await userManager.IsEmailConfirmedAsync(user.Id);
                    claims.Add(new Claim(Constants.ClaimTypes.EmailVerified, verified ? "true" : "false"));
                }
            }

            if (userManager.SupportsUserPhoneNumber)
            {
                var phone = await userManager.GetPhoneNumberAsync(user.Id);
                if (!String.IsNullOrWhiteSpace(phone))
                {
                    claims.Add(new Claim(Constants.ClaimTypes.PhoneNumber, phone));
                    var verified = await userManager.IsPhoneNumberConfirmedAsync(user.Id);
                    claims.Add(new Claim(Constants.ClaimTypes.PhoneNumberVerified, verified ? "true" : "false"));
                }
            }

            if (userManager.SupportsUserClaim)
            {
                claims.AddRange(await userManager.GetClaimsAsync(user.Id));
            }

            if (userManager.SupportsUserRole)
            {
                var roleClaims =
                    from role in await userManager.GetRolesAsync(user.Id)
                    select new Claim(Constants.ClaimTypes.Role, role);
                claims.AddRange(roleClaims);
            }

            return claims;
        }

        protected virtual Task<string> GetDisplayNameForAccountAsync(TUser user)
        {
            return Task.FromResult(user.UserName);
        }

        protected async virtual Task<TUser> FindUserAsync(string username)
        {
            return await userManager.FindByNameAsync(username);
        }

        protected virtual Task<AuthenticateResult> PostAuthenticateLocalAsync(TUser user, SignInMessage message)
        {
            return Task.FromResult<AuthenticateResult>(null);
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext ctx)
        {
            var username = ctx.UserName;
            var password = ctx.Password;
            var message = ctx.SignInMessage;

            ctx.AuthenticateResult = null;

            if (userManager.SupportsUserPassword)
            {
                var user = await FindUserAsync(username);
                if (user != null)
                {
                    if (userManager.SupportsUserLockout &&
                        await userManager.IsLockedOutAsync(user.Id))
                    {
                        ctx.AuthenticateResult = new AuthenticateResult("@lockedOut");
                        return;
                    }

                    if (await userManager.CheckPasswordAsync(user, password))
                    {
                        if (userManager.SupportsUserLockout)
                        {
                            await userManager.ResetAccessFailedCountAsync(user.Id);
                        }

                        var result = await PostAuthenticateLocalAsync(user, message);
                        if (result == null)
                        {
                            var claims = await GetClaimsForAuthenticateResult(user);
                            result = new AuthenticateResult(user.Id.ToString(), await GetDisplayNameForAccountAsync(user), claims);
                        }

                        ctx.AuthenticateResult = result;
                    }
                    else if (userManager.SupportsUserLockout)
                    {
                        ctx.AuthenticateResult = new AuthenticateResult("@wrongPassword");
                        await userManager.AccessFailedAsync(user.Id);
                    }
                }
                else
                {
                    ctx.AuthenticateResult = new AuthenticateResult("@wrongEmail");
                }
            }
        }

        protected virtual async Task<IEnumerable<Claim>> GetClaimsForAuthenticateResult(TUser user)
        {
            List<Claim> claims = new List<Claim>();
            if (EnableSecurityStamp && userManager.SupportsUserSecurityStamp)
            {
                var stamp = await userManager.GetSecurityStampAsync(user.Id);
                if (!String.IsNullOrWhiteSpace(stamp))
                {
                    claims.Add(new Claim("security_stamp", stamp));
                }
            }
            return claims;
        }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext ctx)
        {
            var externalUser = ctx.ExternalIdentity;
            var message = ctx.SignInMessage;

            if (externalUser == null)
            {
                throw new ArgumentNullException("externalUser");
            }

            var user = await userManager.FindAsync(new Microsoft.AspNet.Identity.UserLoginInfo(externalUser.Provider, externalUser.ProviderId));
            if (user == null)
            {
                ctx.AuthenticateResult = await ProcessNewExternalAccountAsync(externalUser.Provider, externalUser.ProviderId, externalUser.Claims, ctx.SignInMessage.Tenant);
            }
            else
            {
                ctx.AuthenticateResult = await ProcessExistingExternalAccountAsync(user, externalUser.Provider, externalUser.ProviderId, externalUser.Claims);
            }
        }

        protected virtual async Task<AuthenticateResult> ProcessNewExternalAccountAsync(string provider, string providerId, IEnumerable<Claim> claims, string tenant)
        {
            var newUser = false;
            var user = await TryGetExistingUserFromExternalProviderClaimsAsync(provider, claims, tenant);
            if (user == null)
            {
                user = await InstantiateNewUserFromExternalProviderAsync(provider, providerId, claims);
                if (user == null)
                    throw new InvalidOperationException("CreateNewAccountFromExternalProvider returned null");

                var createResult = await userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new AuthenticateResult(createResult.Errors.First());
                }
                newUser = true;
            }

            var externalLogin = new Microsoft.AspNet.Identity.UserLoginInfo(provider, providerId);
            var addExternalResult = await userManager.AddLoginAsync(user.Id, externalLogin);
            if (!addExternalResult.Succeeded)
            {
                return new AuthenticateResult(addExternalResult.Errors.First());
            }

            var result = await AccountCreatedFromExternalProviderAsync(user, provider, providerId, claims, newUser);
            if (result != null) return result;

            return await SignInFromExternalProviderAsync(user, provider);
        }

        protected virtual Task<TUser> InstantiateNewUserFromExternalProviderAsync(string provider, string providerId, IEnumerable<Claim> claims)
        {
            var user = new TUser() { UserName = Guid.NewGuid().ToString("N") };
            return Task.FromResult(user);
        }

        protected virtual Task<TUser> TryGetExistingUserFromExternalProviderClaimsAsync(string provider, IEnumerable<Claim> claims, string tenant)
        {
            return Task.FromResult<TUser>(null);
        }

        protected virtual async Task<AuthenticateResult> AccountCreatedFromExternalProviderAsync(TUser user, string provider, string providerId, IEnumerable<Claim> claims, bool isNewUser)
        {
            claims = await SetAccountEmailAsync(user, claims);
            claims = await SetAccountPhoneAsync(user, claims);

            return await UpdateAccountFromExternalClaimsAsync(user, provider, providerId, claims);
        }

        protected virtual async Task<AuthenticateResult> SignInFromExternalProviderAsync(TUser user, string provider)
        {
            var claims = await GetClaimsForAuthenticateResult(user);

            return new AuthenticateResult(
                user.Id.ToString(),
                await GetDisplayNameForAccountAsync(user),
                claims,
                authenticationMethod: Constants.AuthenticationMethods.External,
                identityProvider: provider);
        }

        protected virtual async Task<AuthenticateResult> UpdateAccountFromExternalClaimsAsync(TUser user, string provider, string providerId, IEnumerable<Claim> claims)
        {
            var existingClaims = await userManager.GetClaimsAsync(user.Id);
            var intersection = existingClaims.Intersect(claims, new ClaimComparer());
            var newClaims = claims.Except(intersection, new ClaimComparer());

            foreach (var claim in newClaims)
            {
                var result = await userManager.AddClaimAsync(user.Id, claim);
                if (!result.Succeeded)
                {
                    return new AuthenticateResult(result.Errors.First());
                }
            }

            return null;
        }

        protected virtual async Task<AuthenticateResult> ProcessExistingExternalAccountAsync(TUser user, string provider, string providerId, IEnumerable<Claim> claims)
        {
            return await SignInFromExternalProviderAsync(user, provider);
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountEmailAsync(TUser user, IEnumerable<Claim> claims)
        {
            var email = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.Email);
            if (email != null)
            {
                var userEmail = await userManager.GetEmailAsync(user.Id);
                if (userEmail == null)
                {
                    // if this fails, then presumably the email is already associated with another account
                    // so ignore the error and let the claim pass thru
                    var result = await userManager.SetEmailAsync(user.Id, email.Value);
                    if (result.Succeeded)
                    {
                        var email_verified = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.EmailVerified);
                        if (email_verified != null && email_verified.Value == "true")
                        {
                            var token = await userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            await userManager.ConfirmEmailAsync(user.Id, token);
                        }

                        var emailClaims = new string[] { Constants.ClaimTypes.Email, Constants.ClaimTypes.EmailVerified };
                        return claims.Where(x => !emailClaims.Contains(x.Type));
                    }
                }
            }

            return claims;
        }

        protected virtual async Task<IEnumerable<Claim>> SetAccountPhoneAsync(TUser user, IEnumerable<Claim> claims)
        {
            var phone = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.PhoneNumber);
            if (phone != null)
            {
                var userPhone = await userManager.GetPhoneNumberAsync(user.Id);
                if (userPhone == null)
                {
                    // if this fails, then presumably the phone is already associated with another account
                    // so ignore the error and let the claim pass thru
                    var result = await userManager.SetPhoneNumberAsync(user.Id, phone.Value);
                    if (result.Succeeded)
                    {
                        var phone_verified = claims.FirstOrDefault(x => x.Type == Constants.ClaimTypes.PhoneNumberVerified);
                        if (phone_verified != null && phone_verified.Value == "true")
                        {
                            var token = await userManager.GenerateChangePhoneNumberTokenAsync(user.Id, phone.Value);
                            await userManager.ChangePhoneNumberAsync(user.Id, phone.Value, token);
                        }

                        var phoneClaims = new string[] { Constants.ClaimTypes.PhoneNumber, Constants.ClaimTypes.PhoneNumberVerified };
                        return claims.Where(x => !phoneClaims.Contains(x.Type));
                    }
                }
            }

            return claims;
        }

        public override async Task IsActiveAsync(IsActiveContext ctx)
        {
            var subject = ctx.Subject;

            if (subject == null) throw new ArgumentNullException("subject");

            var id = subject.GetSubjectId();
            TKey key = ConvertSubjectToKey(id);
            var acct = await userManager.FindByIdAsync(key);

            await IsActiveAsync(ctx, acct);
        }

        protected virtual async Task IsActiveAsync(IsActiveContext ctx, TUser user)
        {
            ctx.IsActive = false;
            if (user != null)
            {
                if (EnableSecurityStamp && userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = ctx.Subject.Claims.Where(x => x.Type == "security_stamp").Select(x => x.Value).SingleOrDefault();
                    if (security_stamp != null)
                    {
                        var db_security_stamp = await userManager.GetSecurityStampAsync(user.Id);
                        if (db_security_stamp != security_stamp)
                        {
                            return;
                        }
                    }
                }

                ctx.IsActive = true;
            }
        }
    }
}
