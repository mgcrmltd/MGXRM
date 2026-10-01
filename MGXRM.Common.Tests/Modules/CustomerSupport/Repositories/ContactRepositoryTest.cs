using System;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Modules.CustomerSupport.Repositories;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Modules.CustomerSupport.Repositories
{
    public class ContactRepositoryTest : RepositoryTestBase<ContactRepository>
    {
        private const string Email = "bob@example.com";

        public ContactRepositoryTest() : base(repository => new ContactRepository(repository))
        {
        }

        [Fact]
        public void GetByEmail_Queries_Contacts_By_Email_Address()
        {
            Repository.GetByEmail(Email);

            AssertSingleQueryByAttribute(Contact.EntityLogicalName, Contact.Fields.EmailAddress1, Email);
        }

        [Fact]
        public void GetByEmail_Writes_Nothing()
        {
            Repository.GetByEmail(Email);

            AssertNothingWritten();
        }

        [Fact]
        public void GetByEmail_Returns_Each_Retrieved_Record_As_A_Contact()
        {
            var first = new Entity(Contact.EntityLogicalName, Guid.NewGuid());
            var second = new Entity(Contact.EntityLogicalName, Guid.NewGuid());
            ServiceRetrieves(first, second);

            var contacts = Repository.GetByEmail(Email);

            Assert.Equal(new[] { first.Id, second.Id }, contacts.Select(c => c.Id));
        }

        [Fact]
        public void GetByEmail_Returns_An_Empty_List_When_No_Contact_Has_The_Email()
        {
            Assert.Empty(Repository.GetByEmail(Email));
        }
    }
}
