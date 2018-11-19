using System;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Xunit;

namespace MGXRM.Common.Tests.Framework.ContextManagement
{
    public class WorkflowContextManagerTest
    {
        private IWorkflowContext _fakeWorkflowContext;
        private IOrganizationService _fakeOrganizationService;
        private WorkflowContextManager _manager;

        public WorkflowContextManagerTest()
        {
            _fakeWorkflowContext = A.Fake<IWorkflowContext>();
            _fakeOrganizationService = A.Fake<IOrganizationService>();

            A.CallTo(() => _fakeWorkflowContext.BusinessUnitId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeWorkflowContext.Depth).Returns(3);
            A.CallTo(() => _fakeWorkflowContext.MessageName).Returns("Associate");

            A.CallTo(() => _fakeWorkflowContext.UserId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeWorkflowContext.PrimaryEntityName).Returns("mgxrm_customentity");
            A.CallTo(() => _fakeWorkflowContext.PrimaryEntityId).Returns(Guid.NewGuid());

            A.CallTo(() => _fakeWorkflowContext.CorrelationId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeWorkflowContext.OrganizationId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeWorkflowContext.OrganizationName).Returns("Org1");

            A.CallTo(() => _fakeWorkflowContext.Mode).Returns((int)SdkMessageProcessingStep_Mode.Asynchronous);
            A.CallTo(() => _fakeWorkflowContext.OrganizationName).Returns("Org1");

            A.CallTo(() => _fakeWorkflowContext.InputParameters).Returns(new ParameterCollection());
            A.CallTo(() => _fakeWorkflowContext.OutputParameters).Returns(new ParameterCollection());

            var fakeParentContext = A.Fake<IWorkflowContext>();
            A.CallTo(() => fakeParentContext.PrimaryEntityName).Returns("mgxrm_parent");
            var fakeGrandParentContext = A.Fake<IWorkflowContext>();
            A.CallTo(() => fakeGrandParentContext.PrimaryEntityName).Returns("mgxrm_grandparent");
            A.CallTo(() => _fakeWorkflowContext.ParentContext).Returns(fakeParentContext);
            A.CallTo(() => fakeParentContext.ParentContext).Returns(fakeGrandParentContext);
            A.CallTo(() => fakeGrandParentContext.ParentContext).Returns(null);

            _manager = new WorkflowContextManager(_fakeWorkflowContext, _fakeOrganizationService);
        }

        [Fact]
        public void Context_And_Service_Set_In_Constructor()
        {
            Assert.Same(_fakeWorkflowContext, _manager.Context);
            Assert.Same(_fakeOrganizationService, _manager.Service);
        }

        [Fact]
        public void Expected_Values_Returned()
        {
            Assert.Equal(_fakeWorkflowContext.MessageName, _manager.Message);
            Assert.Equal(_fakeWorkflowContext.Depth, _manager.Depth);
            Assert.Equal(_fakeWorkflowContext.UserId, _manager.UserId);
            Assert.Equal(_fakeWorkflowContext.PrimaryEntityName, _manager.PrimaryEntityName);
            Assert.Equal(_fakeWorkflowContext.PrimaryEntityId, _manager.PrimaryEntityId);
            Assert.Equal(_fakeWorkflowContext.CorrelationId, _manager.CorrelationId);
            Assert.Equal(_fakeWorkflowContext.OrganizationId, _manager.OrganizationId);
            Assert.Equal(_fakeWorkflowContext.OrganizationName, _manager.OrganizationName);
            Assert.Equal((SdkMessageProcessingStep_Mode)_fakeWorkflowContext.Mode, _manager.Mode);
            Assert.Same(_fakeWorkflowContext.InputParameters, _manager.InputParams);
            Assert.Same(_fakeWorkflowContext.OutputParameters, _manager.OutputParams);
        }

        [Fact]
        public void Stage_Returns_Post()
        {
            Assert.Equal(SdkMessageProcessingStep_Stage.Postoperation, _manager.Stage);
        }

        [Theory]
        [InlineData("mgxrm_parent", true)]
        [InlineData("mgxrm_grandparent", true)]
        [InlineData("mgxrm_notPresent", false)]
        public void CalledFromParent_Returns_True_If_Hierarchical_Context_Exists_With_Entity(string entityName, bool expectedReturnValue)
        {
            Assert.Equal(_manager.CalledFromParent(entityName), expectedReturnValue);
        }
    }
}