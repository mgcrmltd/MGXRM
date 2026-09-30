using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ContextManagement
{
    /// <summary>
    /// The image aliases that <see cref="PluginContextManager{T}"/> reads from the plugin execution context.
    /// Plugin step registrations must use these names for their images, otherwise the images will never be
    /// picked up by the framework.
    /// </summary>
    public static class PluginContextManager
    {
        public const string PreImageAlias = "PreImage";
        public const string PostImageAlias = "PostImage";
    }

    public class PluginContextManager<T> : IContextManager<T> where T : Entity
    {
        #region Members and Constructors

        public IPluginExecutionContext Context { get; }
        public IServiceProvider ServiceProvider { get; }

        public PluginContextManager(IPluginExecutionContext context, IOrganizationService service)
        {
            ServiceProvider = null;
            Context = context;
            Service = service;
        }

        public PluginContextManager(IServiceProvider serviceProvider, IOrganizationService service)
        {
            ServiceProvider = serviceProvider;
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            Service = service;
        }

        public PluginContextManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the Organization Service.
            Service = factory.CreateOrganizationService(Context.UserId);
        }

        #endregion

        #region Interface Implementations
        public IOrganizationService Service { get; }
        public Guid UserId => Context.UserId;
        public int Depth => Context.Depth;
        public string Message => Context.MessageName;
        public string PrimaryEntityName => Context.PrimaryEntityName;
        public Guid PrimaryEntityId => Context.PrimaryEntityId;
        public Guid CorrelationId => Context.CorrelationId;
        public string OrganizationName => Context.OrganizationName;
        public Guid OrganizationId => Context.OrganizationId;
        public SdkMessageProcessingStep_Stage Stage => (SdkMessageProcessingStep_Stage)Context.Stage;
        public SdkMessageProcessingStep_Mode Mode => (SdkMessageProcessingStep_Mode)Context.Mode;
        public bool CalledFromParentEntityContext(string entityLogicalName)
        {
            return CalledFromParentEntityContext(entityLogicalName, Context.ParentContext);
        }

        private static bool CalledFromParentEntityContext(string entityName, IPluginExecutionContext context)
        {
            if (context == null)
                return false;
            return context.PrimaryEntityName == entityName || CalledFromParentEntityContext(entityName, context.ParentContext);
        }

        public ParameterCollection InputParams => Context.InputParameters;
        public ParameterCollection OutputParams => Context.OutputParameters;

        public T PreImage => (Context.PreEntityImages != null
                                   && Context.PreEntityImages.Contains(PluginContextManager.PreImageAlias)) ? Context.PreEntityImages[PluginContextManager.PreImageAlias].ToEntity<T>() : null;

        public T TargetImage => (Context.InputParameters != null
                                   && Context.InputParameters.Contains("Target")
                                   && Context.InputParameters["Target"] is Entity)
            ? ((Entity)Context.InputParameters["Target"]).ToEntity<T>()
            : null;

        public T PostImage => (Context.PostEntityImages != null
                                      && Context.PostEntityImages.Contains(PluginContextManager.PostImageAlias)) ? Context.PostEntityImages[PluginContextManager.PostImageAlias].ToEntity<T>() : null;

        #endregion
    }
}