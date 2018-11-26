using MGXRM.Common.Framework.ContextManagement;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class WorkflowControllerBase<T> : ControllerBase<T> where T : Entity
    {
        protected WorkflowControllerBase(IWorkflowContext context, IOrganizationService service) 
            : base(new WorkflowContextManager<T>(context, service))
        {
        }
    }
}