using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Plugins.Controllers;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Plugins.PluginExecution
{
    [CrmPluginRegistration(MessageNameEnum.Update, Contact.EntityLogicalName, StageEnum.PreOperation,
        ExecutionModeEnum.Synchronous, Contact.Fields.LastName, "Pre Contact Update", 10,
        IsolationModeEnum.Sandbox, Id = "4dc8ac06-9bf3-4861-8321-41f9432aa804")]
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