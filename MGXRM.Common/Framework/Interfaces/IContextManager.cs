using System;
using MGXRM.Common.EarlyBounds;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IContextManager<T> where T : Entity
    {
        IOrganizationService Service { get; }
        Guid UserId { get; }
        int Depth { get; }
        string Message { get; }
        string PrimaryEntityName { get; }
        Guid PrimaryEntityId { get; }
        Guid CorrelationId { get; }
        string OrganizationName { get; }
        Guid OrganizationId { get; }
        SdkMessageProcessingStep_Stage Stage { get; }
        SdkMessageProcessingStep_Mode Mode { get; }
        bool CalledFromParentEntityContext(string entityLogicalName);
        ParameterCollection InputParams { get; }
        ParameterCollection OutputParams { get; }
        T PreImage { get; }
        T PostImage { get; }
        T TargetImage { get; }
    }
}