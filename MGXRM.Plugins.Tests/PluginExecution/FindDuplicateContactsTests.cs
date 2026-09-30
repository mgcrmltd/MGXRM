using MGXRM.Plugins.Controllers;
using MGXRM.Plugins.PluginExecution;
using MGXRM.Plugins.Tests.Framework;
using Xunit;

namespace MGXRM.Plugins.Tests.PluginExecution
{
    public class FindDuplicateContactsTests : PluginTestBase<FindDuplicateContacts>
    {
        [Fact]
        public void Is_Registered_As_An_Unbound_Custom_Api()
        {
            PluginUnderTest
                .HasRegistrationCount(1)
                .IsCustomApi(FindDuplicateContacts.UniqueName)
                .IsUnbound()
                .IsConsistent();
        }

        [Fact]
        public void Delegates_To_The_Custom_Api_Controller()
        {
            PluginUnderTest
                .InvokesControllerOperation<FindDuplicateContactsController>(
                    nameof(FindDuplicateContactsController.Execute));
        }
    }
}
