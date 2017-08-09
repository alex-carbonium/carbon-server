using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using Carbon.Business.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Business
{
    [TestClass]
    public class CompanyStateTests
    {
        [TestMethod]
        public void AclsShouldBeUniquePerResource()
        {
            var company = new Company();
            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p0", (int) Permission.Read));

            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p1", (int) Permission.Write));
            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p1", (int) Permission.WriteComments));

            Assert.AreEqual((int)Permission.Read, company.Acls.Single(x => x.Entry.ResourceId == "p0").Permission);
            Assert.AreEqual((int)Permission.WriteComments, company.Acls.Single(x => x.Entry.ResourceId == "p1").Permission);
        }

        [TestMethod]
        public void AclsShouldBeUniquePerResource_Deserialized()
        {
            var company = new Company();
            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p0", (int) Permission.Read));
            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p1", (int) Permission.Write));

            company = SaveRestore(company);
            company.AddOrReplaceAcl(Acl.CreateForProject("guest", "p1", (int) Permission.WriteComments));

            Assert.AreEqual((int)Permission.Read, company.Acls.Single(x => x.Entry.ResourceId == "p0").Permission);
            Assert.AreEqual((int)Permission.WriteComments, company.Acls.Single(x => x.Entry.ResourceId == "p1").Permission);
        }

        [TestMethod]
        public void SaveRestoreFullState()
        {
            var company = new Company();
            company.RootFolder = new ProjectFolder { Id = "1", Name = "2"};
            var acl = Acl.CreateForProject("guest", "p0", (int) Permission.Read);
            company.AddOrReplaceAcl(acl);
            company.AddOrReplaceExternalAcl(ExternalAcl.Create(acl.Entry, company.Name, "1", "ava.png"));
            company.AddOrReplaceUser(new User {Email = "d@c"});

            company.RootFolder.Projects.Add(new Project {Id = "p1", Name = "Project1"});

            company = SaveRestore(company);

            Assert.AreEqual(1, company.Acls.Count);
            Assert.AreEqual(1, company.ExternalAcls.Count);
            Assert.AreEqual(1, company.Users.Count);
            Assert.AreEqual(1, company.RootFolder.Projects.Count);

            company.RootFolder.Projects.Add(new Project());
        }

        private static Company SaveRestore(Company company)
        {
            var serializer = new DataContractSerializer(company.GetType());
            var stringBuilder = new StringBuilder();
            using (var writer = XmlWriter.Create(stringBuilder))
            {
                serializer.WriteObject(writer, company);
            }
            using (var stringReader = new StringReader(stringBuilder.ToString()))
            using (var reader = XmlReader.Create(stringReader))
            {
                return (Company)serializer.ReadObject(reader);
            }
        }
    }
}
