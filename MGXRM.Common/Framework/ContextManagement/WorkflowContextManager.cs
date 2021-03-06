﻿using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;

namespace MGXRM.Common.Framework.ContextManagement
{
    public class WorkflowContextManager<T> : IContextManager<T> where T : Entity
    {
        #region Members and Constructors
        public IWorkflowContext Context { get; }
        public WorkflowContextManager(IWorkflowContext context, IOrganizationService service)
        {
            Context = context;
            Service = service;
        }
        #endregion

        #region Interface Implementations
        public IOrganizationService Service { get; }
        public IServiceProvider ServiceProvider => throw new InvalidPluginExecutionException("No service provider on Workflows");
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
        public T PreImage => (Context.PreEntityImages != null
                                   && Context.PreEntityImages.Contains("PreBusinessEntity")) ? Context.PreEntityImages["PreBusinessEntity"].ToEntity<T>() : null;
        public T PostImage => (Context.PostEntityImages != null
                                    && Context.PostEntityImages.Contains("PostBusinessEntity")) ? Context.PostEntityImages["PostBusinessEntity"].ToEntity<T>() : null;
        public T TargetImage => (Context.InputParameters != null
                                      && Context.InputParameters.Contains("Target")) ? (Context.InputParameters["Target"] as Entity).ToEntity<T>() : null;
        #endregion
    }
}