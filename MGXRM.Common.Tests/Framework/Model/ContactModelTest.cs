using System;
using System.Collections.Generic;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Framework.Repositories;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Model
{
    public class ContactModelTest
    {
        private static ContactModel ModelFor(ModelSetup<Contact> setup)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository);
        }

        private static ContactModel ModelFor(ModelSetup<Contact> setup,
            IEnvironmentVariableRepository environmentVariables)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository,
                new ContactRepository(setup.Repository), environmentVariables);
        }

        private static ContactModel ModelFor(ModelSetup<Contact> setup, IContactRepository contacts,
            IEnvironmentVariableRepository environmentVariables)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository, contacts, environmentVariables);
        }

        #region MakeSurnameUppercase

        [Fact]
        public void MakeSurnameUppercase_Uppercases_A_Surname_Being_Set_On_Create()
        {
            var setup = ModelSetup.For<Contact>()
                .Create().PreOperation().Synchronous()
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        [Fact]
        public void MakeSurnameUppercase_Uppercases_A_Surname_Being_Changed_On_Update()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous().Depth(1)
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        /// <summary>
        /// The surname is not in the target, so the step fired for some other attribute and the model must
        /// leave the record alone rather than writing the pre image value back.
        /// </summary>
        [Fact]
        public void MakeSurnameUppercase_Does_Nothing_When_The_Surname_Is_Not_Being_Set()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.FirstName = "bob");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.False(setup.Images.TargetImage.Contains(Contact.Fields.LastName));
        }

        [Fact]
        public void MakeSurnameUppercase_Does_Nothing_When_The_Surname_Is_Being_Cleared()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = null);

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Null(setup.Images.TargetImage.LastName);
        }

        [Fact]
        public void MakeSurnameUppercase_Leaves_An_Already_Uppercase_Surname_Alone()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation().Synchronous()
                .WithTarget(c => c.LastName = "JONES");

            ModelFor(setup).MakeSurnameUppercase();

            Assert.Equal("JONES", setup.Images.TargetImage.LastName);
        }

        #endregion

        #region EnforceSurnameRequired

        [Fact]
        public void EnforceSurnameRequired_Throws_When_The_Surname_Is_Being_Cleared()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = null);

            Assert.Throws<InvalidPluginExecutionException>(() => ModelFor(setup).EnforceSurnameRequired());
        }

        [Fact]
        public void EnforceSurnameRequired_Allows_A_Surname_Being_Changed()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup).EnforceSurnameRequired();
        }

        [Fact]
        public void EnforceSurnameRequired_Allows_An_Update_That_Does_Not_Touch_The_Surname()
        {
            var setup = ModelSetup.For<Contact>()
                .Update().PostOperation().Synchronous()
                .WithPreImage(c => c.LastName = "smith")
                .WithTarget(c => c.FirstName = "bob");

            ModelFor(setup).EnforceSurnameRequired();
        }

        #endregion


        #region EnforceEmailIsUnique - arranged contact repository

        [Fact]
        public void EnforceEmailIsUnique_Throws_When_Another_Contact_Uses_The_Email()
        {
            var contacts = A.Fake<IContactRepository>();
            A.CallTo(() => contacts.GetByEmail("bob@example.com"))
                .Returns(new List<Contact> { new Contact { Id = Guid.NewGuid() } });

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = "bob@example.com");

            var ex = Assert.Throws<InvalidPluginExecutionException>(
                () => ModelFor(setup, contacts, UniqueEmailEnforcement(true)).EnforceEmailIsUnique());
            Assert.Contains("bob@example.com", ex.Message);
        }

        [Fact]
        public void EnforceEmailIsUnique_Allows_An_Email_No_One_Else_Uses()
        {
            var contacts = A.Fake<IContactRepository>();
            A.CallTo(() => contacts.GetByEmail(A<string>._)).Returns(new List<Contact>());

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = "bob@example.com");

            ModelFor(setup, contacts, UniqueEmailEnforcement(true)).EnforceEmailIsUnique();
        }

        [Fact]
        public void EnforceEmailIsUnique_Does_Not_Query_When_The_Email_Is_Not_Being_Set()
        {
            var contacts = A.Fake<IContactRepository>();

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithPreImage(c => c.EmailAddress1 = "bob@example.com")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup, contacts, UniqueEmailEnforcement(true)).EnforceEmailIsUnique();

            A.CallTo(() => contacts.GetByEmail(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void EnforceEmailIsUnique_Does_Not_Query_When_The_Email_Is_Being_Cleared()
        {
            var contacts = A.Fake<IContactRepository>();

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithPreImage(c => c.EmailAddress1 = "bob@example.com")
                .WithTarget(c => c.EmailAddress1 = null);

            ModelFor(setup, contacts, UniqueEmailEnforcement(true)).EnforceEmailIsUnique();

            A.CallTo(() => contacts.GetByEmail(A<string>._)).MustNotHaveHappened();
        }

        #endregion

        #region EnforceEmailIsUnique - faked environment variable, real contacts in the org service

        [Fact]
        public void A_Contact_Created_Through_The_Org_Service_Is_Found_By_The_Real_Query()
        {
            var setup = SetupForEmail("bob@example.com");
            setup.Repository.Create(new Contact { EmailAddress1 = "bob@example.com" });

            var ex = Assert.Throws<InvalidPluginExecutionException>(
                () => ModelFor(setup, UniqueEmailEnforcement(true)).EnforceEmailIsUnique());
            Assert.Contains("bob@example.com", ex.Message);
        }

        [Fact]
        public void A_Contact_Created_With_A_Different_Email_Does_Not_Clash()
        {
            var setup = SetupForEmail("bob@example.com");
            setup.Repository.Create(new Contact { EmailAddress1 = "someone@example.com" });

            ModelFor(setup, UniqueEmailEnforcement(true)).EnforceEmailIsUnique();
        }

        [Fact]
        public void The_Check_Is_Skipped_When_The_Environment_Variable_Turns_It_Off()
        {
            var setup = SetupForEmail("bob@example.com");
            setup.Repository.Create(new Contact { EmailAddress1 = "bob@example.com" });

            ModelFor(setup, UniqueEmailEnforcement(false)).EnforceEmailIsUnique();
        }

        [Fact]
        public void The_Check_Runs_When_The_Environment_Variable_Is_Unset()
        {
            var setup = SetupForEmail("bob@example.com");
            setup.Repository.Create(new Contact { EmailAddress1 = "bob@example.com" });

            Assert.Throws<InvalidPluginExecutionException>(
                () => ModelFor(setup, UniqueEmailEnforcement(null)).EnforceEmailIsUnique());
        }

        [Fact]
        public void The_Contact_Being_Updated_Is_Not_Its_Own_Duplicate()
        {
            var id = Guid.NewGuid();
            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithId(id)
                .WithTarget(c => c.EmailAddress1 = "bob@example.com")
                .WithFakeCrm();

            setup.Repository.Create(new Contact(id) { EmailAddress1 = "bob@example.com" });

            ModelFor(setup, UniqueEmailEnforcement(true)).EnforceEmailIsUnique();
        }

        [Fact]
        public void Contacts_Really_Are_In_The_Org_Service()
        {
            var setup = SetupForEmail("bob@example.com");
            var id = setup.Repository.Create(new Contact { EmailAddress1 = "bob@example.com" });

            var stored = setup.Service.Retrieve(Contact.EntityLogicalName, id, new ColumnSet(true));

            Assert.Equal("bob@example.com", stored.ToEntity<Contact>().EmailAddress1);
        }

        [Fact]
        public void The_Environment_Variable_Is_Only_Read_Once_Per_Check()
        {
            var variables = UniqueEmailEnforcement(true);
            var setup = SetupForEmail("bob@example.com");

            ModelFor(setup, variables).EnforceEmailIsUnique();

            A.CallTo(() => variables.GetBoolean(ContactModel.EnforceUniqueEmailVariable))
                .MustHaveHappenedOnceExactly();
        }

        private static ModelSetup<Contact> SetupForEmail(string email)
        {
            return ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = email)
                .WithFakeCrm();
        }

        private static IEnvironmentVariableRepository UniqueEmailEnforcement(bool? enabled)
        {
            var variables = A.Fake<IEnvironmentVariableRepository>();
            A.CallTo(() => variables.GetBoolean(ContactModel.EnforceUniqueEmailVariable)).Returns(enabled);
            return variables;
        }

        #endregion

        #region FindDuplicatesByEmail - plain arguments, no api plumbing

        [Fact]
        public void FindDuplicatesByEmail_Returns_Contacts_Sharing_The_Email()
        {
            var existing = new Contact { Id = Guid.NewGuid(), EmailAddress1 = "bob@example.com" };
            var setup = ModelSetup.For<Contact>().WithFakeCrm(existing);

            var duplicates = ModelFor(setup).FindDuplicatesByEmail("bob@example.com", null);

            Assert.Single(duplicates);
            Assert.Equal(existing.Id, duplicates[0].Id);
        }

        [Fact]
        public void FindDuplicatesByEmail_Excludes_The_Given_Contact()
        {
            var id = Guid.NewGuid();
            var setup = ModelSetup.For<Contact>()
                .WithFakeCrm(new Contact { Id = id, EmailAddress1 = "bob@example.com" });

            Assert.Empty(ModelFor(setup).FindDuplicatesByEmail("bob@example.com", id));
        }

        [Fact]
        public void FindDuplicatesByEmail_Returns_Nothing_For_A_Blank_Email()
        {
            var setup = ModelSetup.For<Contact>()
                .WithFakeCrm(new Contact { Id = Guid.NewGuid(), EmailAddress1 = "bob@example.com" });

            Assert.Empty(ModelFor(setup).FindDuplicatesByEmail("   ", null));
        }

        [Fact]
        public void FindDuplicatesByEmail_Does_Not_Query_For_A_Blank_Email()
        {
            var contacts = A.Fake<IContactRepository>();
            var setup = ModelSetup.For<Contact>();

            ModelFor(setup, contacts, UniqueEmailEnforcement(true)).FindDuplicatesByEmail(null, null);

            A.CallTo(() => contacts.GetByEmail(A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void EnforceEmailIsUnique_Uses_The_Same_Domain_Query()
        {
            var contacts = A.Fake<IContactRepository>();
            A.CallTo(() => contacts.GetByEmail("bob@example.com"))
                .Returns(new List<Contact> { new Contact { Id = Guid.NewGuid() } });

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = "bob@example.com");

            var model = ModelFor(setup, contacts, UniqueEmailEnforcement(true));

            Assert.Throws<InvalidPluginExecutionException>(() => model.EnforceEmailIsUnique());
            Assert.Single(model.FindDuplicatesByEmail("bob@example.com", null));
        }

        #endregion
    }
}
