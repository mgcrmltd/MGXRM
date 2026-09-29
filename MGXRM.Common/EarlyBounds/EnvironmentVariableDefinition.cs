using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace MGXRM.Common.EarlyBounds
{
    /// <summary>
    /// Hand-written early bound type for the environment variable definition entity - the half of an
    /// environment variable that ships with the solution and carries the default value. Only the attributes
    /// this solution actually uses are declared.
    /// </summary>
    [DataContract]
    [EntityLogicalName(EntityLogicalName)]
    public class EnvironmentVariableDefinition : Entity
    {
        public const string EntityLogicalName = "environmentvariabledefinition";

        public static class Fields
        {
            public const string EnvironmentVariableDefinitionId = "environmentvariabledefinitionid";
            public const string SchemaName = "schemaname";
            public const string DisplayName = "displayname";
            public const string DefaultValue = "defaultvalue";
            public const string Type = "type";
            public const string ValueSchema = "valueschema";
        }

        public EnvironmentVariableDefinition() : base(EntityLogicalName) { }

        public EnvironmentVariableDefinition(Guid id) : base(EntityLogicalName, id) { }

        [AttributeLogicalName(Fields.EnvironmentVariableDefinitionId)]
        public override Guid Id
        {
            get => base.Id;
            set => EnvironmentVariableDefinitionId = value;
        }

        [AttributeLogicalName(Fields.EnvironmentVariableDefinitionId)]
        public Guid? EnvironmentVariableDefinitionId
        {
            get => GetAttributeValue<Guid?>(Fields.EnvironmentVariableDefinitionId);
            set
            {
                SetAttributeValue(Fields.EnvironmentVariableDefinitionId, value);
                if (value.HasValue)
                    base.Id = value.Value;
            }
        }

        /// <summary>
        /// The unique name a variable is looked up by, including the publisher prefix.
        /// </summary>
        [AttributeLogicalName(Fields.SchemaName)]
        public string SchemaName
        {
            get => GetAttributeValue<string>(Fields.SchemaName);
            set => SetAttributeValue(Fields.SchemaName, value);
        }

        [AttributeLogicalName(Fields.DisplayName)]
        public string DisplayName
        {
            get => GetAttributeValue<string>(Fields.DisplayName);
            set => SetAttributeValue(Fields.DisplayName, value);
        }

        /// <summary>
        /// Used when this environment has no value record of its own. Stored as text whatever the declared
        /// <see cref="Type"/> is.
        /// </summary>
        [AttributeLogicalName(Fields.DefaultValue)]
        public string DefaultValue
        {
            get => GetAttributeValue<string>(Fields.DefaultValue);
            set => SetAttributeValue(Fields.DefaultValue, value);
        }

        [AttributeLogicalName(Fields.Type)]
        public OptionSetValue Type
        {
            get => GetAttributeValue<OptionSetValue>(Fields.Type);
            set => SetAttributeValue(Fields.Type, value);
        }

        [AttributeLogicalName(Fields.ValueSchema)]
        public string ValueSchema
        {
            get => GetAttributeValue<string>(Fields.ValueSchema);
            set => SetAttributeValue(Fields.ValueSchema, value);
        }
    }

    /// <summary>
    /// The declared type of an environment variable. Note that the value is stored as text regardless, so
    /// this says how to read it rather than how it is held.
    /// </summary>
    public enum EnvironmentVariableDefinition_Type
    {
        String = 100000000,
        Number = 100000001,
        Boolean = 100000002,
        Json = 100000003,
        DataSource = 100000004,
        Secret = 100000005
    }
}
