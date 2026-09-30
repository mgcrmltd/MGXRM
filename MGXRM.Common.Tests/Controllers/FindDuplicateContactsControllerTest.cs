using FakeItEasy;
using MGXRM.Common.Controllers;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Tests.TestCore;
using Xunit;

namespace MGXRM.Common.Tests.Controllers
{
    public class FindDuplicateContactsControllerTest
    {
        private const string UniqueName = "mgxrm_FindDuplicateContacts";

        [Fact]
        public void The_Operation_Is_Handed_To_The_Model()
        {
            var model = A.Fake<IContactModel>();
            var setup = CustomApiSetup.For<Contact>(UniqueName);

            new FindDuplicateContactsController(setup.Context, model).Execute();

            A.CallTo(() => model.FindDuplicatesByEmail()).MustHaveHappenedOnceExactly();
        }
    }
}
