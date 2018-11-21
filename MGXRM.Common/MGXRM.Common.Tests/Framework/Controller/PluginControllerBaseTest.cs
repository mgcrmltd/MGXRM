using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using Microsoft.Xrm.Sdk;
using Xunit;
using FakeItEasy;

namespace MGXRM.Common.Tests.Framework.Controller
{
    public class PluginControllerBaseTest
    {
        [Fact]
        public void Context_Set_In_Constructor()
        {
            var context = GetFakeContext(SdkMessageProcessingStep_Mode.Synchronous);
            var controller = new TestControllerBaseClass(context);
            Assert.Same(context, controller.BaseExecutionContext);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostCreateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostCreateAsync")]
        public void PostCreateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostCreate());
            Assert.Equal(expectedMessage, ex.Message);
        }

       [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostUpdateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostUpdateAsync")]
        public void PostUpdateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostUpdate());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostSetStateDynamicEntitySync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostSetStateDynamicEntityAsync")]
        public void PostSetStateDynamicEntityCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostSetStateDynamicEntity());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostSetStateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostSetStateAsync")]
        public void PostSetStateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostSetState());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostAssignSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostAssignAsync")]
        public void PostAssignCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostAssign());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostCloseSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostCloseAsync")]
        public void PostCloseCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            var context = GetFakeContext(mode);
            var controller = new TestControllerBaseClass(context);
            var ex = Assert.Throws<Exception>(() => controller.PostClose());
            Assert.Equal(expectedMessage, ex.Message);
        }

        private IPluginExecutionContext GetFakeContext(SdkMessageProcessingStep_Mode mode)
        {
            var context = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => context.Mode).Returns((int)mode);
            return context;
        }
    }

    public class TestControllerBaseClass : PluginControllerBase<Entity>
    {
        public IPluginExecutionContext BaseExecutionContext => base.Context;
        
        public TestControllerBaseClass(IPluginExecutionContext context) : base(context)
        {
        }

        public override void PostCreateSync()
        {
            throw new Exception("PostCreateSync");
        }

        public override void PostCreateAsync()
        {
            throw new Exception("PostCreateAsync");
        }

        public override void PostUpdateSync()
        {
            throw new Exception("PostUpdateSync");
        }

        public override void PostUpdateAsync()
        {
            throw new Exception("PostUpdateAsync");
        }

        public override void PostSetStateDynamicEntitySync()
        {
            throw new Exception("PostSetStateDynamicEntitySync");
        }

        public override void PostSetStateDynamicEntityAsync()
        {
            throw new Exception("PostSetStateDynamicEntityAsync");
        }

        public override void PostSetStateSync()
        {
            throw new Exception("PostSetStateSync");
        }

        public override void PostSetStateAsync()
        {
            throw new Exception("PostSetStateAsync");
        }

        public override void PostAssignSync()
        {
            throw new Exception("PostAssignSync");
        }

        public override void PostAssignAsync()
        {
            throw new Exception("PostAssignAsync");
        }

        public override void PostCloseSync()
        {
            throw new Exception("PostCloseSync");
        }
        public override void PostCloseAsync()
        {
            throw new Exception("PostCloseAsync");
        }
    }
}