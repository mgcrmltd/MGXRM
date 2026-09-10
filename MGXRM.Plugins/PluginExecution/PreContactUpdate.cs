using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Plugins.Controllers;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Plugins.PluginExecution
{
    public class PreContactUpdate : Plugin
    {
        public PreContactUpdate()
            : base(typeof(PreContactUpdate))
        {
            base.RegisteredEvents.Add(new Tuple<int, string, string, Action<LocalPluginContext>>(20, "Update",
                "incident", ExecutePreContactUpdate));
        }
        
        protected void ExecutePreContactUpdate(LocalPluginContext localContext)
        {
            #region Context, Service and Images
            if (localContext == null)
            {
                throw new ArgumentNullException("localContext");
            }
            var context = localContext.PluginExecutionContext;
            #endregion

            if (context.Depth > 1)
                return;

            new ContactController(localContext.ServiceProvider).PreUpdate();
        }
    }
}