using MGXRM.Common.Framework.Controller;
using Microsoft.Xrm.Sdk;
using Xunit;
using FakeItEasy;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Tests.Framework.Controller
{
    public class WorkflowControllerBaseTest
    {
        private IOrganizationService _fakeService;

        public WorkflowControllerBaseTest()
        {
            _fakeService = A.Fake<IOrganizationService>();
        }

        [Fact]
        public void Context_Set_In_Constructor()
        {
            var context = A.Fake<IWorkflowContext>();
            var controller = new TestWorkflowControllerBaseClass(context, _fakeService);

            Assert.Same(context, ((WorkflowContextManager<Entity>)controller.BaseContext).Context);
        }
    }

    public class TestWorkflowControllerBaseClass : WorkflowControllerBase<Entity>
    {
        public IContextManager<Entity> BaseContext => base.ContextManager;
        public TestWorkflowControllerBaseClass(IWorkflowContext context, IOrganizationService service) : base(context, service)
        {
        }
    }
}