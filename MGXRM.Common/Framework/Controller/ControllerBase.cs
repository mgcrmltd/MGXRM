using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.ImageManagement;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Controller
{
    public abstract class ControllerBase<T> where T : Entity
    {
        protected IContextManager<T> ContextManager;
        protected IImageManager<T> ImageManager;
        protected SdkMessageProcessingStep_Mode Mode => (SdkMessageProcessingStep_Mode)ContextManager.Mode;

        protected ControllerBase(IContextManager<T> contextManager)
        {
            ContextManager = contextManager;
            ImageManager = new ImageManager<T>(ContextManager.PreImage, ContextManager.TargetImage, ContextManager.PostImage);
        }
    }
}