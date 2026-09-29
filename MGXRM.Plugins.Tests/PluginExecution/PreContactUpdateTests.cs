using MGXRM.Common.EarlyBounds;
using MGXRM.Plugins.Controllers;
using MGXRM.Plugins.PluginExecution;
using MGXRM.Plugins.Tests.Framework;
using Xunit;

namespace MGXRM.Plugins.Tests.PluginExecution
{
    public class PreContactUpdateTests : PluginTestBase<PreContactUpdate>
    {
        [Fact]
        public void Is_Registered_As_Expected()
        {
            PluginUnderTest
                .HasRegistrationCount(1)
                .IsRegisteredFor(MessageNameEnum.Update, Contact.EntityLogicalName)
                .HasStepName("Pre Contact Update")
                .IsPreOperation()
                .IsSynchronous()
                .IsSandbox()
                .HasOrder(10)
                .HasSingleFilteringAttribute(Contact.Fields.LastName)
                .HasNoImages()
                .IsConsistent();
        }

        [Fact]
        public void Delegates_To_The_Contact_Controller()
        {
            PluginUnderTest
                .InvokesControllerOperation<ContactController>(nameof(ContactController.PreUpdate))
                .DoesNotInvokeControllerOperation<ContactController>(nameof(ContactController.PreCreate));
        }
    }
}
