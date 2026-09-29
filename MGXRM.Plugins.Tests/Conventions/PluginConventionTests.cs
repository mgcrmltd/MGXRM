using MGXRM.Plugins.Tests.Framework;
using Xunit;

namespace MGXRM.Plugins.Tests.Conventions
{
    /// <summary>
    /// Assembly wide checks. These cover every plugin as it is added, so a new class picks them up
    /// without anyone having to remember to write them.
    /// </summary>
    public class PluginConventionTests
    {
        [Fact]
        public void Every_Plugin_Is_Registered()
        {
            PluginConventions.AssertEveryPluginIsRegistered();
        }

        [Fact]
        public void Every_Step_Has_A_Unique_Id()
        {
            PluginConventions.AssertRegistrationIdsAreUnique();
        }

        [Fact]
        public void Every_Registration_Matches_Its_Registered_Events()
        {
            PluginConventions.AssertRegistrationsMatchRegisteredEvents();
        }

        [Fact]
        public void Every_Image_Uses_The_Context_Manager_Names()
        {
            PluginConventions.AssertImagesUseContextManagerNames();
        }
    }
}
