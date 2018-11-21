using MGXRM.Common.Framework.Controller;
using Microsoft.Xrm.Sdk;
using Xunit;
using FakeItEasy;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Tests.Framework.Controller
{
    public class WorkflowControllerBaseTest
    {
        [Fact]
        public void Context_Set_In_Constructor()
        {
            var context = A.Fake<IWorkflowContext>();
            var controller = new TestWorkflowControllerBaseClass(context);
            Assert.Same(context, controller.BaseExecutionContext);
        }
    }

    public class TestWorkflowControllerBaseClass : WorkflowControllerBase<Entity>
    {
        public IWorkflowContext BaseExecutionContext => base.Context;
        public TestWorkflowControllerBaseClass(IWorkflowContext context) : base(context)
        {
        }
    }
}