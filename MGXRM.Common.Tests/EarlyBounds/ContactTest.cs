using System;
using MGXRM.Common.EarlyBounds;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.EarlyBounds
{
    public class ContactTest
    {
        [Fact]
        public void Default_Constructor_Sets_Logical_Name()
        {
            Assert.Equal("contact", new Contact().LogicalName);
            Assert.Equal("contact", Contact.EntityLogicalName);
        }

        [Fact]
        public void Guid_Constructor_Sets_Logical_Name_And_Id()
        {
            var id = Guid.NewGuid();
            var contact = new Contact(id);
            Assert.Equal("contact", contact.LogicalName);
            Assert.Equal(id, contact.Id);
        }

        [Fact]
        public void Setters_Write_Expected_Attribute_Keys()
        {
            var parent = new EntityReference("account", Guid.NewGuid());
            var owner = new EntityReference("systemuser", Guid.NewGuid());
            var contact = new Contact
            {
                FirstName = "Grace",
                MiddleName = "Brewster",
                LastName = "Hopper",
                EmailAddress1 = "grace@example.com",
                Telephone1 = "0123",
                MobilePhone = "0456",
                ParentCustomerId = parent,
                OwnerId = owner,
                StateCode = new OptionSetValue(0),
                StatusCode = new OptionSetValue(1)
            };

            Assert.Equal("Grace", contact[Contact.Fields.FirstName]);
            Assert.Equal("Brewster", contact[Contact.Fields.MiddleName]);
            Assert.Equal("Hopper", contact[Contact.Fields.LastName]);
            Assert.Equal("grace@example.com", contact[Contact.Fields.EmailAddress1]);
            Assert.Equal("0123", contact[Contact.Fields.Telephone1]);
            Assert.Equal("0456", contact[Contact.Fields.MobilePhone]);
            Assert.Same(parent, contact[Contact.Fields.ParentCustomerId]);
            Assert.Same(owner, contact[Contact.Fields.OwnerId]);
            Assert.Equal(0, ((OptionSetValue)contact[Contact.Fields.StateCode]).Value);
            Assert.Equal(1, ((OptionSetValue)contact[Contact.Fields.StatusCode]).Value);
        }

        [Fact]
        public void Id_And_ContactId_Stay_In_Step()
        {
            var id = Guid.NewGuid();

            var setViaId = new Contact { Id = id };
            Assert.Equal(id, setViaId.ContactId);
            Assert.Equal(id, setViaId[Contact.Fields.ContactId]);

            var setViaContactId = new Contact { ContactId = id };
            Assert.Equal(id, setViaContactId.Id);
        }

        [Fact]
        public void ContactId_Set_To_Null_Empties_Id()
        {
            var contact = new Contact(Guid.NewGuid()) { ContactId = null };
            Assert.Equal(Guid.Empty, contact.Id);
        }

        [Fact]
        public void Unset_Attributes_Return_Null_Rather_Than_Throwing()
        {
            var contact = new Contact();
            Assert.Null(contact.FirstName);
            Assert.Null(contact.EmailAddress1);
            Assert.Null(contact.FullName);
            Assert.Null(contact.ParentCustomerId);
            Assert.Null(contact.StateCode);
            Assert.Null(contact.CreatedOn);
            Assert.Null(contact.ModifiedOn);
        }

        [Fact]
        public void ToEntity_From_Late_Bound_Reads_Through_Properties()
        {
            var parent = new EntityReference("account", Guid.NewGuid());
            var created = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            var late = new Entity("contact", Guid.NewGuid());
            late[Contact.Fields.FirstName] = "Grace";
            late[Contact.Fields.LastName] = "Hopper";
            late[Contact.Fields.FullName] = "Grace Hopper";
            late[Contact.Fields.EmailAddress1] = "grace@example.com";
            late[Contact.Fields.ParentCustomerId] = parent;
            late[Contact.Fields.StateCode] = new OptionSetValue(0);
            late[Contact.Fields.CreatedOn] = created;

            var contact = late.ToEntity<Contact>();

            Assert.Equal(late.Id, contact.Id);
            Assert.Equal("Grace", contact.FirstName);
            Assert.Equal("Hopper", contact.LastName);
            Assert.Equal("Grace Hopper", contact.FullName);
            Assert.Equal("grace@example.com", contact.EmailAddress1);
            Assert.Equal(parent, contact.ParentCustomerId);
            Assert.Equal(0, contact.StateCode.Value);
            Assert.Equal(created, contact.CreatedOn);
            Assert.Equal(late.Attributes.Count, contact.Attributes.Count);
        }

        [Theory]
        [InlineData(Contact.Fields.ContactId, "contactid")]
        [InlineData(Contact.Fields.FirstName, "firstname")]
        [InlineData(Contact.Fields.MiddleName, "middlename")]
        [InlineData(Contact.Fields.LastName, "lastname")]
        [InlineData(Contact.Fields.FullName, "fullname")]
        [InlineData(Contact.Fields.EmailAddress1, "emailaddress1")]
        [InlineData(Contact.Fields.Telephone1, "telephone1")]
        [InlineData(Contact.Fields.MobilePhone, "mobilephone")]
        [InlineData(Contact.Fields.ParentCustomerId, "parentcustomerid")]
        [InlineData(Contact.Fields.OwnerId, "ownerid")]
        [InlineData(Contact.Fields.StateCode, "statecode")]
        [InlineData(Contact.Fields.StatusCode, "statuscode")]
        [InlineData(Contact.Fields.CreatedOn, "createdon")]
        [InlineData(Contact.Fields.ModifiedOn, "modifiedon")]
        public void Fields_Hold_Expected_Logical_Names(string field, string expected)
        {
            Assert.Equal(expected, field);
        }
    }
}
