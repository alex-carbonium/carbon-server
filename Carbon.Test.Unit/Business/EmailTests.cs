using Carbon.Business.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class EmailTests : BaseTest
    {
        [TestMethod]
        public void SerializeAndDeserialize()
        {
            //arrange
            var email = new Email();
            email.To = "test@carbonium.io";
            email.TemplateName = "template";            
            email.Model = new
                              {
                                  FirstLevelProperty = 1,
                                  ComplexProperty = new
                                          {
                                              SecondLevelProperty = 2
                                          }
                              };

            //act
            var s = email.ToString();
            var newEmail = Email.FromString<Email>(s);

            //assert
            Assert.AreEqual(email.To, newEmail.To, "Wrong to");            
            Assert.AreEqual(email.TemplateName, newEmail.TemplateName, "Wrong template");
            Assert.AreEqual((int)email.Model.FirstLevelProperty, (int)newEmail.Model.FirstLevelProperty, "Wrong first level property");
            Assert.AreEqual((int)email.Model.ComplexProperty.SecondLevelProperty, (int)newEmail.Model.ComplexProperty.SecondLevelProperty, 
                "Wrong second level property");
        }

        [TestMethod]
        public void SerializeBulkEmail()
        {
            //arrange
            var email = new BulkEmail();
            email.To = "test@carbonium.io";            
            email.MessageCount = 100;            

            //act
            var s = email.ToString();
            var newEmail = Email.FromString<BulkEmail>(s);

            //assert
            Assert.AreEqual(email.To, newEmail.To, "Wrong to");
            Assert.AreEqual(email.MessageCount, newEmail.MessageCount, "Wrong derived property");            
        }
    }
}
