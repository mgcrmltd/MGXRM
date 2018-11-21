using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class PluginControllerBase<T> : IPluginEvents where T : Entity
    {
        protected IPluginExecutionContext Context { get; }
        protected SdkMessageProcessingStep_Mode Synchronous => (SdkMessageProcessingStep_Mode)Context.Mode;

        protected PluginControllerBase(IPluginExecutionContext context)
        {
            Context = context;
        }

        public virtual void PreCreate()
        {
        }

        public void PostCreate()
        {
            
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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

        public virtual void RetrieveMultiple()
        {
        }

        public virtual void PreClose()
        {
        }

        public void PostClose()
        {
            if(Synchronous == SdkMessageProcessingStep_Mode.Synchronous)
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