using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Common.Framework.Repositories
{
    public interface IEnvironmentVariableRepository : IRepository
    {
        string GetString(string schemaName);
        int? GetWholeNumber(string schemaName);
        decimal? GetDecimal(string schemaName);
        bool? GetBoolean(string schemaName);
        Guid? GetGuid(string schemaName);
    }

    public class EnvironmentVariableRepository : RepositoryDecorator, IEnvironmentVariableRepository
    {
        private delegate bool TryParser<T>(string value, out T parsed);

        private const string CurrentValueAlias = "currentvalue";

        private static readonly string[] YesValues = { "yes", "true", "1" };
        private static readonly string[] NoValues = { "no", "false", "0" };

        private readonly Dictionary<string, string> _valuesBySchemaName =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public EnvironmentVariableRepository(IRepository repository) : base(repository)
        {
        }

        public string GetString(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentException("An environment variable schema name is required.", nameof(schemaName));

            if (!_valuesBySchemaName.ContainsKey(schemaName))
                _valuesBySchemaName[schemaName] = CurrentValueOrDefault(schemaName);

            return _valuesBySchemaName[schemaName];
        }

        public int? GetWholeNumber(string schemaName)
        {
            return ParsedAs<int>(schemaName, TryParseWholeNumber, "a whole number");
        }

        public decimal? GetDecimal(string schemaName)
        {
            return ParsedAs<decimal>(schemaName, TryParseDecimal, "a number");
        }

        public bool? GetBoolean(string schemaName)
        {
            return ParsedAs<bool>(schemaName, TryParseYesOrNo, "a yes or no value");
        }

        public Guid? GetGuid(string schemaName)
        {
            return ParsedAs<Guid>(schemaName, Guid.TryParse, "a guid");
        }

        private T? ParsedAs<T>(string schemaName, TryParser<T> tryParse, string expected) where T : struct
        {
            var value = GetString(schemaName);
            if (value == null)
                return null;

            T parsed;
            if (!tryParse(value.Trim(), out parsed))
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' holds '{value}', which is not {expected}.");

            return parsed;
        }

        private static bool TryParseWholeNumber(string value, out int parsed)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool TryParseDecimal(string value, out decimal parsed)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed);
        }

        private static bool TryParseYesOrNo(string value, out bool parsed)
        {
            var lowered = value.ToLowerInvariant();
            parsed = YesValues.Contains(lowered);
            return parsed || NoValues.Contains(lowered);
        }

        private string CurrentValueOrDefault(string schemaName)
        {
            var definition = RetrieveMultiple(DefinitionWithCurrentValue(schemaName)).FirstOrDefault();

            if (definition == null)
                throw new InvalidPluginExecutionException(
                    $"There is no environment variable definition with the schema name '{schemaName}'.");

            return FirstConfigured(CurrentValueOf(definition), DefaultValueOf(definition));
        }

        private static string FirstConfigured(params string[] values)
        {
            return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        private static string CurrentValueOf(Entity definition)
        {
            var alias = $"{CurrentValueAlias}.{EnvironmentVariableValue.Fields.Value}";
            return definition.GetAttributeValue<AliasedValue>(alias)?.Value as string;
        }

        private static string DefaultValueOf(Entity definition)
        {
            return definition.GetAttributeValue<string>(EnvironmentVariableDefinition.Fields.DefaultValue);
        }

        private static QueryExpression DefinitionWithCurrentValue(string schemaName)
        {
            var query = new QueryExpression(EnvironmentVariableDefinition.EntityLogicalName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(EnvironmentVariableDefinition.Fields.DefaultValue)
            };
            query.Criteria.AddCondition(EnvironmentVariableDefinition.Fields.SchemaName, ConditionOperator.Equal,
                schemaName);

            var currentValue = query.AddLink(
                EnvironmentVariableValue.EntityLogicalName,
                EnvironmentVariableDefinition.Fields.EnvironmentVariableDefinitionId,
                EnvironmentVariableValue.Fields.EnvironmentVariableDefinitionId,
                JoinOperator.LeftOuter);
            currentValue.EntityAlias = CurrentValueAlias;
            currentValue.Columns = new ColumnSet(EnvironmentVariableValue.Fields.Value);

            return query;
        }
    }
}
