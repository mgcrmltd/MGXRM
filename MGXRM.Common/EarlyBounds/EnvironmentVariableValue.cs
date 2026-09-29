using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace MGXRM.Common.EarlyBounds
{
    /// <summary>
    /// Hand-written early bound type for the environment variable value entity - the half that holds what
    /// this particular environment overrode the definition's default with. A variable left at its default
    /// has no record here at all, which is why it has to be outer joined.
    /// </summary>
    [DataContract]
    [EntityLogicalName(EntityLogicalName)]
    public class EnvironmentVariableValue : Entity
    {
        public const string EntityLogicalName = "environmentvariablevalue";

        public static class Fields
        {
            public const string EnvironmentVariableValueId = "environmentvariablevalueid";
            public const string EnvironmentVariableDefinitionId = "environmentvariabledefinitionid";
            public const string Value = "value";
        }

        public EnvironmentVariableValue() : base(EntityLogicalName) { }

        public EnvironmentVariableValue(Guid id) : base(EntityLogicalName, id) { }

        [AttributeLogicalName(Fields.EnvironmentVariableValueId)]
        public override Guid Id
        {
            get => base.Id;
            set => EnvironmentVariableValueId = value;
        }

        [AttributeLogicalName(Fields.EnvironmentVariableValueId)]
        public Guid? EnvironmentVariableValueId
        {
            get => GetAttributeValue<Guid?>(Fields.EnvironmentVariableValueId);
            set
            {
                SetAttributeValue(Fields.EnvironmentVariableValueId, value);
                if (value.HasValue)
                    base.Id = value.Value;
            }
        }

        [AttributeLogicalName(Fields.EnvironmentVariableDefinitionId)]
        public EntityReference EnvironmentVariableDefinitionId
        {
            get => GetAttributeValue<EntityReference>(Fields.EnvironmentVariableDefinitionId);
            set => SetAttributeValue(Fields.EnvironmentVariableDefinitionId, value);
        }

        [AttributeLogicalName(Fields.Value)]
        public string Value
        {
            get => GetAttributeValue<string>(Fields.Value);
            set => SetAttributeValue(Fields.Value, value);
        }
    }
}
