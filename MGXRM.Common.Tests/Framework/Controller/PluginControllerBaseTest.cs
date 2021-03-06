﻿using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using Microsoft.Xrm.Sdk;
using Xunit;
using FakeItEasy;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.Interfaces;

namespace MGXRM.Common.Tests.Framework.Controller
{
    public class PluginControllerBaseTest
    {
        private IOrganizationServiceFactory _fakeOrgFactory;
        private IOrganizationService _fakeService;
        private IServiceProvider _fakeServiceProvider;
        private IPluginExecutionContext _fakeContext;


        public PluginControllerBaseTest()
        {
            _fakeService = A.Fake<IOrganizationService>();
            _fakeOrgFactory = A.Fake<IOrganizationServiceFactory>();
            _fakeServiceProvider = A.Fake<IServiceProvider>();
            _fakeContext = A.Fake<IPluginExecutionContext>();
            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IOrganizationServiceFactory))).Returns(_fakeOrgFactory);
            A.CallTo(() => _fakeServiceProvider.GetService(typeof(IPluginExecutionContext))).Returns(_fakeContext);
            A.CallTo(() => _fakeOrgFactory.CreateOrganizationService(A<Guid?>._)).Returns(_fakeService);
        }

        //[Fact]
        //public void Context_Set_In_Constructor()
        //{
        //    SetContextMode(SdkMessageProcessingStep_Mode.Synchronous);
        //     var controller = new TestControllerBaseClass(_fakeServiceProvider);
        //    Assert.Same(context, ((PluginContextManager<Entity>)controller.BaseContext).Context);
        //}

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostCreateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostCreateAsync")]
        public void PostCreateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
            var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostCreate());
            Assert.Equal(expectedMessage, ex.Message);
        }

        
        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostUpdateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostUpdateAsync")]
        public void PostUpdateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostUpdate());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostSetStateDynamicEntitySync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostSetStateDynamicEntityAsync")]
        public void PostSetStateDynamicEntityCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostSetStateDynamicEntity());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostSetStateSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostSetStateAsync")]
        public void PostSetStateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostSetState());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostAssignSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostAssignAsync")]
        public void PostAssignCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostAssign());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PostCloseSync")]
        [InlineData(SdkMessageProcessingStep_Mode.Asynchronous, "PostCloseAsync")]
        public void PostCloseCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PostClose());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreCreate")]
        public void PreCreateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreCreate());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreUpdate")]
        public void PreUpdateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreUpdate());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreSetStateDynamicEntity")]
        public void PreSetStateDynamicEntityCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreSetStateDynamicEntity());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreSetState")]
        public void PreSetStateCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreSetState());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreAssign")]
        public void PreAssignCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
             var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreAssign());
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [InlineData(SdkMessageProcessingStep_Mode.Synchronous, "PreClose")]
        public void PreCloseCallsCorrectSyncMethod(SdkMessageProcessingStep_Mode mode, string expectedMessage)
        {
            SetContextMode(mode);
            var controller = new TestControllerBaseClass(_fakeServiceProvider);
            var ex = Assert.Throws<Exception>(() => controller.PreClose());
            Assert.Equal(expectedMessage, ex.Message);
        }
        
        private void SetContextMode(SdkMessageProcessingStep_Mode mode)
        {
            A.CallTo(() => _fakeContext.Mode).Returns((int)mode);
        }
    }

    public class TestControllerBaseClass : PluginControllerBase<Entity>
    {
        public IContextManager<Entity> BaseContext => base.ContextManager;
        
        public TestControllerBaseClass(IServiceProvider provider) : base(provider)
        {
        }

        public override void PostCreateSync()
        {
            base.PostCreateSync();
            throw new Exception("PostCreateSync");
        }

        public override void PostCreateAsync()
        {
            base.PostCreateAsync();
            throw new Exception("PostCreateAsync");
        }

        public override void PostUpdateSync()
        {
            base.PostUpdateSync();
            throw new Exception("PostUpdateSync");
        }

        public override void PostUpdateAsync()
        {
            base.PostUpdateAsync();
            throw new Exception("PostUpdateAsync");
        }

        public override void PostSetStateDynamicEntitySync()
        {
            base.PostSetStateDynamicEntitySync();
            throw new Exception("PostSetStateDynamicEntitySync");
        }

        public override void PostSetStateDynamicEntityAsync()
        {
            base.PostSetStateDynamicEntityAsync();
            throw new Exception("PostSetStateDynamicEntityAsync");
        }

        public override void PostSetStateSync()
        {
            base.PostSetStateSync();
            throw new Exception("PostSetStateSync");
        }

        public override void PostSetStateAsync()
        {
            base.PostSetStateAsync();
            throw new Exception("PostSetStateAsync");
        }

        public override void PostAssignSync()
        {
            base.PostAssignSync();
            throw new Exception("PostAssignSync");
        }

        public override void PostAssignAsync()
        {
            base.PostAssignAsync();
            throw new Exception("PostAssignAsync");
        }

        public override void PostCloseSync()
        {
            base.PostCloseSync();
            throw new Exception("PostCloseSync");
        }
        public override void PostCloseAsync()
        {
            base.PostCloseAsync();
            throw new Exception("PostCloseAsync");
        }

        public override void PreCreate()
        {
            base.PreCreate();
            throw new Exception("PreCreate");
        }

        public override void PreUpdate()
        {
            base.PreUpdate();
            throw new Exception("PreUpdate");
        }

        public override void PreSetStateDynamicEntity()
        {
            base.PreSetStateDynamicEntity();
            throw new Exception("PreSetStateDynamicEntity");
        }

        public override void PreSetState()
        {
            base.PreSetState();
            throw new Exception("PreSetState");
        }

        public override void PreAssign()
        {
            base.PreAssign();
            throw new Exception("PreAssign");
        }

        public override void PreClose()
        {
            base.PreClose();
            throw new Exception("PreClose");
        }
    }
}