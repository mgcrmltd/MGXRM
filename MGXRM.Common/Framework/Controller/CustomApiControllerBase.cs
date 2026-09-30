using System;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.CustomApi;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class CustomApiControllerBase<T> : ControllerBase<T>, ICustomApiOperation where T : Entity
    {
        protected ICustomApiRequest Request { get; }
        protected ICustomApiResponse Response { get; }

        protected CustomApiControllerBase(IServiceProvider provider)
            : this(new PluginContextManager<T>(provider))
        {
        }

        protected CustomApiControllerBase(IContextManager<T> contextManager) : base(contextManager)
        {
            Request = new CustomApiRequest(contextManager.InputParams);
            Response = new CustomApiResponse(contextManager.OutputParams);
        }

        public abstract void Execute();
    }
}
