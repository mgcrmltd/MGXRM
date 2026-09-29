using System;
using System.Collections.Generic;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Framework.Repositories;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Model
{
    public class ContactModelTest
    {
        /// <summary>
        /// The contact repository decorates whatever repository the setup produced, so the same helper
        /// serves both a faked repository and a real one over a seeded CRM.
        /// </summary>
        private static ContactModel ModelFor(ModelSetup<Contact> setup)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository,
                new ContactRepository(setup.Repository));
        }

        private static ContactModel ModelFor(ModelSetup<Contact> setup, IContactRepository contacts)
        {
            return new ContactModel(setup.Images, setup.Context, setup.Repository, contacts);
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

        #region EnforceEmailIsUnique - arranging the repository

        /// <summary>
        /// Faking the repository and passing it in. The test states what the query returns, so it says
        /// nothing about how the query is built. Use this when the rule is the point.
        /// </summary>
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
                () => ModelFor(setup, contacts).EnforceEmailIsUnique());
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

            ModelFor(setup, contacts).EnforceEmailIsUnique();
        }

        /// <summary>
        /// The step can fire for any of its filtering attributes, so a model that queries on every call
        /// would cost a query per update for no reason.
        /// </summary>
        [Fact]
        public void EnforceEmailIsUnique_Does_Not_Query_When_The_Email_Is_Not_Being_Set()
        {
            var contacts = A.Fake<IContactRepository>();

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithPreImage(c => c.EmailAddress1 = "bob@example.com")
                .WithTarget(c => c.LastName = "jones");

            ModelFor(setup, contacts).EnforceEmailIsUnique();

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

            ModelFor(setup, contacts).EnforceEmailIsUnique();

            A.CallTo(() => contacts.GetByEmail(A<string>._)).MustNotHaveHappened();
        }

        #endregion

        #region EnforceEmailIsUnique - seeding the CRM instead

        /// <summary>
        /// Seeding the data and letting ContactRepository's real query run against it. Use this when the
        /// query itself is the point - this test would catch a wrong attribute name, which the arranged
        /// version above cannot.
        /// </summary>
        [Fact]
        public void EnforceEmailIsUnique_Finds_A_Real_Duplicate_In_The_Seeded_Crm()
        {
            var existing = new Contact { Id = Guid.NewGuid(), EmailAddress1 = "bob@example.com" };

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = "bob@example.com")
                .WithFakeCrm(existing);

            Assert.Throws<InvalidPluginExecutionException>(() => ModelFor(setup).EnforceEmailIsUnique());
        }

        [Fact]
        public void EnforceEmailIsUnique_Allows_An_Email_No_Seeded_Contact_Uses()
        {
            var existing = new Contact { Id = Guid.NewGuid(), EmailAddress1 = "someone@example.com" };

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithTarget(c => c.EmailAddress1 = "bob@example.com")
                .WithFakeCrm(existing);

            ModelFor(setup).EnforceEmailIsUnique();
        }

        /// <summary>
        /// An update that re-sends the same email must not reject itself.
        /// </summary>
        [Fact]
        public void EnforceEmailIsUnique_Ignores_The_Contact_Being_Updated()
        {
            var id = Guid.NewGuid();

            var setup = ModelSetup.For<Contact>()
                .Update().PreOperation()
                .WithId(id)
                .WithTarget(c => c.EmailAddress1 = "bob@example.com")
                .WithFakeCrm(new Contact { Id = id, EmailAddress1 = "bob@example.com" });

            ModelFor(setup).EnforceEmailIsUnique();
        }

        #endregion
    }
}
