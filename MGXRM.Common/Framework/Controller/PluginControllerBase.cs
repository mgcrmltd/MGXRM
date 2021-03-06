﻿using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ContextManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class PluginControllerBase<T> : ControllerBase<T>, IPluginEvents where T : Entity
    {
        protected PluginControllerBase(IServiceProvider provider, IOrganizationService service)
            : base(new PluginContextManager<T>(provider, service)) { }

        protected PluginControllerBase(IServiceProvider provider)
            : base(new PluginContextManager<T>(provider)) { }

        public virtual void PreCreate()
        {
        }

        public void PostCreate()
        {

            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostCreateSync();
            else
                PostCreateAsync();
        }

        public virtual void PostCreateSync()
        {
        }

        public virtual void PostCreateAsync()
        {
        }

        public virtual void PreSetState()
        {
        }

        public void PostSetState()
        {
            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostSetStateSync();
            else
                PostSetStateAsync();
        }

        public virtual void PostSetStateSync()
        {
        }

        public virtual void PostSetStateAsync()
        {
        }

        public virtual void PreSetStateDynamicEntity()
        {
        }

        public void PostSetStateDynamicEntity()
        {
            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostSetStateDynamicEntitySync();
            else
                PostSetStateDynamicEntityAsync();
        }

        public virtual void PostSetStateDynamicEntitySync()
        {
        }

        public virtual void PostSetStateDynamicEntityAsync()
        {
        }

        public virtual void PreUpdate()
        {
        }

        public void PostUpdate()
        {
            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostUpdateSync();
            else
                PostUpdateAsync();
        }

        public virtual void PostUpdateSync()
        {
        }

        public virtual void PostUpdateAsync()
        {
        }

        public void PostAssign()
        {
            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostAssignSync();
            else
                PostAssignAsync();
        }

        public virtual void PreAssign()
        {
        }

        public virtual void PostAssignSync()
        {
        }

        public virtual void PostAssignAsync()
        {
        }

        public virtual void PreClose()
        {
        }

        public void PostClose()
        {
            if (Mode == SdkMessageProcessingStep_Mode.Synchronous)
                PostCloseSync();
            else
                PostCloseAsync();
        }

        public virtual void PostCloseSync()
        {
        }

        public virtual void PostCloseAsync()
        {
        }
    }
}