//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Sketch.Business.Domain;
//using Sketch.Business.Services;
//using Sketch.Framework.UnitOfWork;
//using Sketch.Framework.Validation;
//using Sketch.Services.IdentityServer;
//using Thinktecture.IdentityServer.Core.Models;

//namespace Sketch.Test.Unit.Business
//{
//    [TestClass]
//    public class AuthenticationTests : BaseTest
//    {
//        private IdentityUserService _identityService;
//        private UserService _userService;
//        private IValidator _validator;

//        [TestInitialize]
//        public override void Setup()
//        {
// 	        base.Setup();
//            _userService = Scope.Resolve<UserService>();
//            _identityService = new IdentityUserService(Scope, LogService.Object);
//            _validator = new DictionaryValidator();            
//        }

//        [TestMethod]
//        public void AuthLocal()
//        {
//            //arrange
//            var user = new User("user", "user@email.com") { RegistrationType = RegistrationType.Local };
//            _userService.RegisterNewUser(user);

//            //act
//            var result = _identityService.AuthenticateLocalAsync(Scope, _userService, user.Email, _validator).Result;

//            //assert
//            Assert.AreEqual(string.Empty, _validator.GetAllErrors(), "No errors should be present");
//            Assert.IsFalse(result.IsError, "Wrong result status");
//            Assert.IsFalse(result.IsPartialSignIn, "Wrong login type");
//        }

//        //[TestMethod]
//        //public void AuthExternalIfSameEmailAlreadyRegistered()
//        //{
//        //    //arrange            
//        //    var user = new User("user", "user@email.com") { RegistrationType = RegistrationType.Google, ExternalId = "GoogleId" };
//        //    _userService.RegisterNewUser(user);

//        //    //act
//        //    var externalIdentity = new ExternalIdentity
//        //    {
//        //        Provider = new IdentityProvider { Name = "Google" },
//        //        ProviderId = "GoogleId",
//        //        Claims = new List<Claim> { new Claim("email", "user@email.com"), new Claim("name", "user") }
//        //    };
//        //    var result = _identityService.AuthenticateExternalAsync(user.Email, externalIdentity).Result;

//        //    //assert            
//        //    Assert.AreEqual(string.Empty, _validator.GetAllErrors(), "No errors should be present");
//        //    Assert.IsFalse(result.IsError, "Wrong result status");
//        //    Assert.IsFalse(result.IsPartialSignIn, "Wrong login type");
//        //}

//        //[TestMethod]
//        //public void AuthExternalIfNoUserRegistered()
//        //{
//        //    //arrange            
//        //    var user = new User("user", "user@email.com") { RegistrationType = RegistrationType.Google, ExternalId = "GoogleId" };            

//        //    //act
//        //    var externalIdentity = new ExternalIdentity
//        //    {
//        //        Provider = new IdentityProvider { Name = "Google" },
//        //        ProviderId = "GoogleId",
//        //        Claims = new List<Claim> { new Claim("email", "user@email.com"), new Claim("name", "user") }
//        //    };
//        //    var result = _identityService.AuthenticateExternalAsync(user.Email, externalIdentity).Result;

//        //    //assert            
//        //    Assert.AreEqual(string.Empty, _validator.GetAllErrors(), "No errors should be present");
//        //    Assert.IsFalse(result.IsError, "Wrong result status");
//        //    Assert.IsTrue(result.IsPartialSignIn, "Wrong login type");
//        //    Assert.IsTrue(UnitOfWorkStub.ExistsBy(User.FindByEmail(user.Email, RegistrationType.Google)), "User must be registered");
//        //}

//        //[TestMethod]
//        //public void DontAuthExternalIfSameEmailIsRegisteredWithAnotherProvider()
//        //{
//        //    //arrange            
//        //    var user = new User("user", "user@email.com") { RegistrationType = RegistrationType.Facebook, ExternalId = "FacebookId"};
//        //    _userService.RegisterNewUser(user);

//        //    //act
//        //    var externalIdentity = new ExternalIdentity
//        //    {
//        //        Provider = new IdentityProvider { Name = "Google" },
//        //        ProviderId = "GoogleId",
//        //        Claims = new List<Claim> { new Claim("email", "user@email.com"), new Claim("name", "user") }
//        //    };            
//        //    var result = _identityService.AuthenticateExternalAsync(user.Email, externalIdentity).Result;

//        //    //assert            
//        //    Assert.IsFalse(result.IsError, "Wrong result status");
//        //    Assert.IsTrue(result.IsPartialSignIn, "Wrong login type");
//        //    Assert.AreSame(user, _userService.FindUserByEmail(user.Email), "There should not be users with duplicate emails");
//        //    Assert.IsNotNull(result.RedirectClaims.Single(x => x.Type == ClaimTypes.Email && x.Value == "user@email.com"), "Email claim should be issued");
//        //}

//        //[TestMethod]
//        //public void AuthExternalShouldConsiderLegacyIdAndUpgradeId()
//        //{
//        //    //arrange            
//        //    var user = new User("user", "user@email.com") { RegistrationType = RegistrationType.Google, ExternalId = "legacy@email.com" };
//        //    _userService.RegisterNewUser(user);

//        //    //act
//        //    var externalIdentity = new ExternalIdentity
//        //    {
//        //        Provider = new IdentityProvider { Name = "Google" },
//        //        ProviderId = "NewId",
//        //        Claims = new List<Claim> { new Claim("email", "legacy@email.com"), new Claim("name", "user") }
//        //    };
//        //    var result = _identityService.AuthenticateExternalAsync(user.Email, externalIdentity).Result;

//        //    //assert            
//        //    Assert.AreEqual(string.Empty, _validator.GetAllErrors(), "No errors should be present");
//        //    Assert.IsFalse(result.IsError, "Wrong result status");
//        //    Assert.IsFalse(result.IsPartialSignIn, "Wrong login type");
//        //    Assert.AreEqual("NewId", user.ExternalId, "Legacy id should be updated");
//        //}
//    }
//}
