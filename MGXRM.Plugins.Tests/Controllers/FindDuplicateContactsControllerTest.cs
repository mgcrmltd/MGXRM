using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;
using MGXRM.Plugins.Controllers;
using Xunit;

namespace MGXRM.Plugins.Tests.Controllers
{
    public class FindDuplicateContactsControllerTest
    {
        [Fact]
        public void The_Operation_Is_Handed_To_The_Model()
        {
            var model = A.Fake<IContactModel>();

            new FindDuplicateContactsController(A.Fake<IContextManager<Contact>>(), model).Execute();

            A.CallTo(() => model.FindDuplicatesByEmail()).MustHaveHappenedOnceExactly();
        }
    }
}
