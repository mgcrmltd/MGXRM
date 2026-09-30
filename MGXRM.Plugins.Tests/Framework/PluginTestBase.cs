namespace MGXRM.Plugins.Tests.Framework
{
    public abstract class PluginTestBase<TPlugin> where TPlugin : Plugin
    {
        protected static PluginAssertions<TPlugin> PluginUnderTest => new PluginAssertions<TPlugin>();

        protected static PluginAssertions<TPlugin> Step(string stepName)
        {
            return new PluginAssertions<TPlugin>().ForStep(stepName);
        }
    }
}
