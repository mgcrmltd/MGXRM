using System;
using FakeItEasy;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ContextManagement;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace MGXRM.Common.Tests.Framework.ContextManagement
{
    public class PluginContextManagerTest
    {
        #region Members and setup
        private IPluginExecutionContext _fakeExecutionContext;
        private IOrganizationService _fakeOrganizationService;
        private PluginContextManager _manager;

        public PluginContextManagerTest()
        {
            _fakeExecutionContext = A.Fake<IPluginExecutionContext>();
            _fakeOrganizationService = A.Fake<IOrganizationService>();

            A.CallTo(() => _fakeExecutionContext.BusinessUnitId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeExecutionContext.Depth).Returns(3);
            A.CallTo(() => _fakeExecutionContext.MessageName).Returns("Associate");

            A.CallTo(() => _fakeExecutionContext.UserId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeExecutionContext.PrimaryEntityName).Returns("mgxrm_customentity");
            A.CallTo(() => _fakeExecutionContext.PrimaryEntityId).Returns(Guid.NewGuid());

            A.CallTo(() => _fakeExecutionContext.CorrelationId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeExecutionContext.OrganizationId).Returns(Guid.NewGuid());
            A.CallTo(() => _fakeExecutionContext.OrganizationName).Returns("Org1");

            A.CallTo(() => _fakeExecutionContext.Stage).Returns((int)SdkMessageProcessingStep_Stage.Postoperation);
            A.CallTo(() => _fakeExecutionContext.Mode).Returns((int)SdkMessageProcessingStep_Mode.Asynchronous);
            A.CallTo(() => _fakeExecutionContext.OrganizationName).Returns("Org1");

            A.CallTo(() => _fakeExecutionContext.InputParameters).Returns(new ParameterCollection());
            A.CallTo(() => _fakeExecutionContext.OutputParameters).Returns(new ParameterCollection());

            var fakeParentContext = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => fakeParentContext.PrimaryEntityName).Returns("mgxrm_parent");
            var fakeGrandParentContext = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => fakeGrandParentContext.PrimaryEntityName).Returns("mgxrm_grandparent");
            A.CallTo(() => _fakeExecutionContext.ParentContext).Returns(fakeParentContext);
            A.CallTo(() => fakeParentContext.ParentContext).Returns(fakeGrandParentContext);
            A.CallTo(() => fakeGrandParentContext.ParentContext).Returns(null);

            _manager = new PluginContextManager(_fakeExecutionContext, _fakeOrganizationService);
        }
        #endregion

        [Fact]
        public void Context_And_Service_Set_In_Constructor()
        {
            Assert.Same(_fakeExecutionContext, _manager.Context);
            Assert.Same(_fakeOrganizationService, _manager.Service);
        }

        [Fact]
        public void Expected_Values_Returned()
        {

            /*
                public bool CalledFromParent(string entityLogicalName)
                {
                    return Context != null && (Context.PrimaryEntityName == entityLogicalName || CalledFromParent(entityLogicalName));
                }
                public ParameterCollection InputParams => Context.InputParameters;
                public ParameterCollection OutputParams => Context.InputParameters;
             */

            var manager = new PluginContextManager(_fakeExecutionContext, _fakeOrganizationService);
            
            Assert.Equal(_fakeExecutionContext.MessageName, manager.Message);
            Assert.Equal(_fakeExecutionContext.Depth, manager.Depth);
            Assert.Equal(_fakeExecutionContext.UserId, manager.UserId);
            Assert.Equal(_fakeExecutionContext.PrimaryEntityName, manager.PrimaryEntityName);
            Assert.Equal(_fakeExecutionContext.PrimaryEntityId, manager.PrimaryEntityId);
            Assert.Equal(_fakeExecutionContext.CorrelationId, manager.CorrelationId);
            Assert.Equal(_fakeExecutionContext.OrganizationId, manager.OrganizationId);
            Assert.Equal(_fakeExecutionContext.OrganizationName, manager.OrganizationName);

            Assert.Equal((SdkMessageProcessingStep_Stage)_fakeExecutionContext.Stage, manager.Stage);
            Assert.Equal((SdkMessageProcessingStep_Mode)_fakeExecutionContext.Mode, manager.Mode);

            Assert.Same(_fakeExecutionContext.InputParameters, manager.InputParams);
            Assert.Same(_fakeExecutionContext.OutputParameters, manager.OutputParams);
        }

        [Theory]
        [InlineData("mgxrm_parent",true)]
        [InlineData("mgxrm_grandparent", true)]
        [InlineData("mgxrm_notPresent", false)]
        public void CalledFromParent_Returns_True_If_Hierarchical_Context_Exists_With_Entity(string entityName, bool expectedReturnValue)
        {
            Assert.Equal(_manager.CalledFromParent(entityName),expectedReturnValue);
        }
    }
}
