using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class WorkflowControllerBase<T> where T : Entity
    {
        protected IWorkflowContext Context { get; }

        protected WorkflowControllerBase(IWorkflowContext context)
        {
            Context = context;
        }
    }
}