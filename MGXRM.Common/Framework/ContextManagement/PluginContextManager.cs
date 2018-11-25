using System;
using System.Threading;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ContextManagement
{
    public class PluginContextManager<T> : IContextManager<T>  where T : Entity
    {
        #region Members and Constructors
        
        public IPluginExecutionContext Context { get; }

        public PluginContextManager(IPluginExecutionContext context, IOrganizationService service)
        {
            Context = context;
            Service = service;
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
                                   && Context.PreEntityImages.Contains("PreImage")) ? Context.PreEntityImages["PreImage"] as T : null;

        public T TargetImage => (Context.InputParameters != null
                                   && Context.InputParameters.Contains("Target")) ? Context.InputParameters["Target"] as T : null;

        public T PostImage => (Context.PostEntityImages != null
                                      && Context.PostEntityImages.Contains("PostImage")) ? Context.PostEntityImages["PostImage"] as T: null;

        #endregion
    }
}