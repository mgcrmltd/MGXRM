namespace MGXRM.Plugins.Tests.Framework
{
    /// <summary>
    /// Base class for the tests covering a class in MGXRM.Plugins.PluginExecution. Derive from it with the
    /// plugin as the type argument and assert against <see cref="PluginUnderTest"/>:
    /// <code>
    /// public class PreContactUpdateTests : PluginTestBase&lt;PreContactUpdate&gt;
    /// {
    ///     [Fact]
    ///     public void Is_Registered_Correctly()
    ///     {
    ///         PluginUnderTest
    ///             .IsRegisteredFor(MessageNameEnum.Update, Contact.EntityLogicalName)
    ///             .IsPreOperation()
    ///             .IsSandbox()
    ///             .HasOrder(10)
    ///             .HasSingleFilteringAttribute(Contact.Fields.LastName)
    ///             .IsConsistent();
    ///     }
    /// }
    /// </code>
    /// </summary>
    public abstract class PluginTestBase<TPlugin> where TPlugin : Plugin
    {
        /// <summary>
        /// A fresh assertion chain over the plugin. Each call starts a new chain, so tests do not leak
        /// the step selected by ForStep/ForMessage into one another.
        /// </summary>
        protected static PluginAssertions<TPlugin> PluginUnderTest => new PluginAssertions<TPlugin>();

        /// <summary>
        /// A chain scoped to one named step, for plugins that declare more than one registration.
        /// </summary>
        protected static PluginAssertions<TPlugin> Step(string stepName)
        {
            return new PluginAssertions<TPlugin>().ForStep(stepName);
        }
    }
}
