using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Framework.ContextManagement
{
    public class WorkflowContextManager : IContextManager
    {
        public IWorkflowContext Context { get; }
        public WorkflowContextManager(IWorkflowContext context, IOrganizationService service)
        {
            Context = context;
            Service = service;
        }

        public IOrganizationService Service { get; }
        public Guid UserId => Context.UserId;
        public int Depth => Context.Depth;
        public string Message => Context.MessageName;
        public string PrimaryEntityName => Context.PrimaryEntityName;
        public Guid PrimaryEntityId => Context.PrimaryEntityId;
        public Guid CorrelationId => Context.CorrelationId;
        public string OrganizationName => Context.OrganizationName;
        public Guid OrganizationId => Context.OrganizationId;

        public SdkMessageProcessingStep_Stage Stage => SdkMessageProcessingStep_Stage.Postoperation;
        public SdkMessageProcessingStep_Mode Mode => (SdkMessageProcessingStep_Mode)Context.Mode;
        public bool CalledFromParentEntityContext(string entityLogicalName)
        {
            return CalledFromParentEntityContext(entityLogicalName, Context.ParentContext);
        }

        private static bool CalledFromParentEntityContext(string entityName, IWorkflowContext context)
        {
            if (context == null)
                return false;
            return context.PrimaryEntityName == entityName || CalledFromParentEntityContext(entityName, context.ParentContext);
        }
        public ParameterCollection InputParams => Context.InputParameters;
        public ParameterCollection OutputParams => Context.OutputParameters;
    }
}