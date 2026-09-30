using System;
using System.Collections.Generic;
using FakeItEasy;
using MGXRM.Common.Controllers;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Tests.TestCore;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Controllers
{
    public class FindDuplicateContactsControllerTest
    {
        private const string UniqueName = "mgxrm_FindDuplicateContacts";

        [Fact]
        public void The_Email_Parameter_Is_Passed_To_The_Model()
        {
            var model = A.Fake<IContactModel>();
            var setup = SetupWith("bob@example.com");

            ControllerFor(setup, model).Execute();

            A.CallTo(() => model.FindDuplicatesByEmail("bob@example.com", null)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void The_Optional_Exclude_Parameter_Is_Passed_To_The_Model()
        {
            var excluded = Guid.NewGuid();
            var model = A.Fake<IContactModel>();
            var setup = SetupWith("bob@example.com")
                .WithParameter(FindDuplicateContactsController.ExcludeContactIdParameter, excluded);

            ControllerFor(setup, model).Execute();

            A.CallTo(() => model.FindDuplicatesByEmail("bob@example.com", excluded)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void The_Count_Is_Returned_As_A_Response_Property()
        {
            var model = ModelFinding(new Contact(Guid.NewGuid()), new Contact(Guid.NewGuid()));
            var setup = SetupWith("bob@example.com");

            ControllerFor(setup, model).Execute();

            Assert.Equal(2, setup.OutputParameters[FindDuplicateContactsController.DuplicateCountProperty]);
        }

        [Fact]
        public void The_First_Match_Is_Returned_As_A_Response_Property()
        {
            var first = new Contact(Guid.NewGuid());
            var model = ModelFinding(first, new Contact(Guid.NewGuid()));
            var setup = SetupWith("bob@example.com");

            ControllerFor(setup, model).Execute();

            var match = (EntityReference)setup.OutputParameters[FindDuplicateContactsController.FirstMatchProperty];
            Assert.Equal(first.Id, match.Id);
            Assert.Equal(Contact.EntityLogicalName, match.LogicalName);
        }

        [Fact]
        public void No_First_Match_Is_Returned_When_Nothing_Is_Found()
        {
            var setup = SetupWith("bob@example.com");

            ControllerFor(setup, ModelFinding()).Execute();

            Assert.Equal(0, setup.OutputParameters[FindDuplicateContactsController.DuplicateCountProperty]);
            Assert.False(setup.OutputParameters.Contains(FindDuplicateContactsController.FirstMatchProperty));
        }

        [Fact]
        public void A_Missing_Email_Parameter_Is_Rejected_By_Name()
        {
            var setup = CustomApiSetup.For<Contact>(UniqueName);

            var ex = Assert.Throws<InvalidPluginExecutionException>(
                () => ControllerFor(setup, A.Fake<IContactModel>()).Execute());

            Assert.Contains(FindDuplicateContactsController.EmailAddressParameter, ex.Message);
        }

        [Fact]
        public void A_Parameter_Of_The_Wrong_Type_Is_Rejected_By_Name()
        {
            var setup = CustomApiSetup.For<Contact>(UniqueName)
                .WithParameter(FindDuplicateContactsController.EmailAddressParameter, 42);

            var ex = Assert.Throws<InvalidPluginExecutionException>(
                () => ControllerFor(setup, A.Fake<IContactModel>()).Execute());

            Assert.Contains(FindDuplicateContactsController.EmailAddressParameter, ex.Message);
        }

        private static CustomApiSetup<Contact> SetupWith(string email)
        {
            return CustomApiSetup.For<Contact>(UniqueName)
                .WithParameter(FindDuplicateContactsController.EmailAddressParameter, email);
        }

        private static FindDuplicateContactsController ControllerFor(CustomApiSetup<Contact> setup, IContactModel model)
        {
            return new FindDuplicateContactsController(setup.Context, model);
        }

        private static IContactModel ModelFinding(params Contact[] duplicates)
        {
            var model = A.Fake<IContactModel>();
            A.CallTo(() => model.FindDuplicatesByEmail(A<string>._, A<Guid?>._))
                .Returns(new List<Contact>(duplicates));
            return model;
        }
    }
}
