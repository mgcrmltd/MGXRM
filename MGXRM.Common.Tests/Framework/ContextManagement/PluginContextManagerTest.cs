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
        private PluginContextManager<Entity> _manager;

        public PluginContextManagerTest()
        {
            FakeContext();
        }

        private void FakeContext(bool addImages = true)
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

            var inputParams = new ParameterCollection ();
            if (addImages)
            {
                inputParams.Add("Target", new Entity {Id = Guid.NewGuid()});
            }

            A.CallTo(() => _fakeExecutionContext.InputParameters).Returns(inputParams);
            A.CallTo(() => _fakeExecutionContext.OutputParameters).Returns(new ParameterCollection());

            var preImages = new EntityImageCollection();
            var postImages = new EntityImageCollection();

            if (addImages)
            {
                preImages.Add("PreImage", new Entity { Id = Guid.NewGuid()});
                postImages.Add("PostImage", new Entity { Id = Guid.NewGuid() });
            }

            A.CallTo(() => _fakeExecutionContext.PreEntityImages).Returns(preImages);
            A.CallTo(() => _fakeExecutionContext.PostEntityImages).Returns(postImages);

            var fakeParentContext = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => fakeParentContext.PrimaryEntityName).Returns("mgxrm_parent");
            var fakeGrandParentContext = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => fakeGrandParentContext.PrimaryEntityName).Returns("mgxrm_grandparent");
            A.CallTo(() => _fakeExecutionContext.ParentContext).Returns(fakeParentContext);
            A.CallTo(() => fakeParentContext.ParentContext).Returns(fakeGrandParentContext);
            A.CallTo(() => fakeGrandParentContext.ParentContext).Returns(null);

            _manager = new PluginContextManager<Entity>(_fakeExecutionContext, _fakeOrganizationService);
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
            Assert.Equal(_fakeExecutionContext.MessageName, _manager.Message);
            Assert.Equal(_fakeExecutionContext.Depth, _manager.Depth);
            Assert.Equal(_fakeExecutionContext.UserId, _manager.UserId);
            Assert.Equal(_fakeExecutionContext.PrimaryEntityName, _manager.PrimaryEntityName);
            Assert.Equal(_fakeExecutionContext.PrimaryEntityId, _manager.PrimaryEntityId);
            Assert.Equal(_fakeExecutionContext.CorrelationId, _manager.CorrelationId);
            Assert.Equal(_fakeExecutionContext.OrganizationId, _manager.OrganizationId);
            Assert.Equal(_fakeExecutionContext.OrganizationName, _manager.OrganizationName);

            Assert.Equal((SdkMessageProcessingStep_Stage)_fakeExecutionContext.Stage, _manager.Stage);
            Assert.Equal((SdkMessageProcessingStep_Mode)_fakeExecutionContext.Mode, _manager.Mode);

            Assert.Same(_fakeExecutionContext.InputParameters, _manager.InputParams);
            Assert.Same(_fakeExecutionContext.OutputParameters, _manager.OutputParams);

            Assert.Equal(_fakeExecutionContext.PreEntityImages["PreImage"].Id, _manager.PreImage.Id);
            Assert.Equal(_fakeExecutionContext.PostEntityImages["PostImage"].Id, _manager.PostImage.Id);
            Assert.Equal((_fakeExecutionContext.InputParameters["Target"] as Entity).Id, _manager.TargetImage.Id);
        }

        [Theory]
        [InlineData("mgxrm_parent",true)]
        [InlineData("mgxrm_grandparent", true)]
        [InlineData("mgxrm_notPresent", false)]
        public void CalledFromParent_Returns_True_If_Hierarchical_Context_Exists_With_Entity(string entityName, bool expectedReturnValue)
        {
            Assert.Equal(_manager.CalledFromParentEntityContext(entityName),expectedReturnValue);
        }

        [Fact]
        public void Images_Return_Null_If_Not_Present()
        {
            FakeContext(false);
            Assert.Null(_manager.PreImage);
            Assert.Null(_manager.TargetImage);
            Assert.Null(_manager.PostImage);
        }

        [Fact]
        public void Images_Returns_Null_If_Collection_Not_Present()
        {
            FakeContext(false);
            A.CallTo(() => _fakeExecutionContext.InputParameters).Returns(null);
            A.CallTo(() => _fakeExecutionContext.PreEntityImages).Returns(null);
            A.CallTo(() => _fakeExecutionContext.PostEntityImages).Returns(null);
            Assert.Null(_manager.PreImage);
            Assert.Null(_manager.TargetImage);
            Assert.Null(_manager.PostImage);
        }

    }
}


