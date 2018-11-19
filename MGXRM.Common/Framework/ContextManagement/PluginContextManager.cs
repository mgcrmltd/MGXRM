using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.ContextManagement
{
    public class PluginContextManager : IContextManager
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
        public bool CalledFromParent(string entityLogicalName)
        {
            return CalledFromParentPluginOfEntityType(entityLogicalName, Context.ParentContext);
        }

        private static bool CalledFromParentPluginOfEntityType(string entityName, IPluginExecutionContext context)
        {
            if (context == null)
                return false;
            return context.PrimaryEntityName == entityName || CalledFromParentPluginOfEntityType(entityName, context.ParentContext);
        }

        public ParameterCollection InputParams => Context.InputParameters;
        public ParameterCollection OutputParams => Context.OutputParameters;
        #endregion
    }
}