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

    /// <summary>
    /// Reads environment variables by schema name.
    /// <para>
    /// There is no message for this, so the values have to be queried. An environment variable is two
    /// records: the definition, which belongs to the solution and carries the default value, and
    /// optionally a value record holding what this particular environment overrode it with. The current
    /// value wins, and the definition's default is the fallback, so both come back from one query with an
    /// outer join.
    /// </para>
    /// <para>
    /// Every value is stored as text whatever the variable's declared type, which is why the typed methods
    /// below parse rather than cast. Booleans are the trap: Dataverse stores a Yes/No variable as the
    /// string "yes" or "no", not "true" or "false".
    /// </para>
    /// </summary>
    public class EnvironmentVariableRepository : RepositoryDecorator, IEnvironmentVariableRepository
    {
        private const string ValueAlias = "currentvalue";

        /// <summary>
        /// Values are read repeatedly by models in the same execution, so each schema name is only queried
        /// once per instance. Environment variables do not change mid transaction.
        /// </summary>
        private readonly Dictionary<string, string> _cache =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public EnvironmentVariableRepository(IRepository repository) : base(repository)
        {
        }

        /// <summary>
        /// The current value, or the definition's default when this environment has not overridden it, or
        /// null when neither is set. Throws when no definition exists at all, since a schema name that is
        /// not in the solution is a deployment problem or a typo rather than an absent value.
        /// </summary>
        public string GetString(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
                throw new ArgumentException("An environment variable schema name is required.", nameof(schemaName));

            string cached;
            if (_cache.TryGetValue(schemaName, out cached))
                return cached;

            var value = Retrieve(schemaName);
            _cache[schemaName] = value;
            return value;
        }

        public int? GetWholeNumber(string schemaName)
        {
            var value = GetString(schemaName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' holds '{value}', which is not a whole number.");

            return parsed;
        }

        public decimal? GetDecimal(string schemaName)
        {
            var value = GetString(schemaName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            decimal parsed;
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' holds '{value}', which is not a number.");

            return parsed;
        }

        /// <summary>
        /// Accepts what Dataverse actually stores for a Yes/No variable - "yes" and "no" - as well as the
        /// true/false and 1/0 forms that turn up when a value is set by code or by hand.
        /// </summary>
        public bool? GetBoolean(string schemaName)
        {
            var value = GetString(schemaName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            switch (value.Trim().ToLowerInvariant())
            {
                case "yes":
                case "true":
                case "1":
                    return true;
                case "no":
                case "false":
                case "0":
                    return false;
                default:
                    throw new InvalidPluginExecutionException(
                        $"Environment variable '{schemaName}' holds '{value}', which is not a yes or no value.");
            }
        }

        public Guid? GetGuid(string schemaName)
        {
            var value = GetString(schemaName);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Guid parsed;
            if (!Guid.TryParse(value.Trim(), out parsed))
                throw new InvalidPluginExecutionException(
                    $"Environment variable '{schemaName}' holds '{value}', which is not a guid.");

            return parsed;
        }

        private string Retrieve(string schemaName)
        {
            var definition = RetrieveMultiple(BuildQuery(schemaName)).FirstOrDefault();

            if (definition == null)
                throw new InvalidPluginExecutionException(
                    $"There is no environment variable definition with the schema name '{schemaName}'.");

            var current = definition
                .GetAttributeValue<AliasedValue>($"{ValueAlias}.{EnvironmentVariableValue.Fields.Value}");

            var value = current?.Value as string;
            if (string.IsNullOrWhiteSpace(value))
                value = definition.GetAttributeValue<string>(EnvironmentVariableDefinition.Fields.DefaultValue);

            // A variable set to blank is not configured, however the blank got there.
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// The definition filtered by schema name, with the value record outer joined so a variable that
        /// has never been overridden still comes back and can fall through to its default.
        /// </summary>
        private static QueryExpression BuildQuery(string schemaName)
        {
            var query = new QueryExpression(EnvironmentVariableDefinition.EntityLogicalName)
            {
                NoLock = true,
                ColumnSet = new ColumnSet(EnvironmentVariableDefinition.Fields.DefaultValue,
                    EnvironmentVariableDefinition.Fields.SchemaName)
            };
            query.Criteria.AddCondition(EnvironmentVariableDefinition.Fields.SchemaName, ConditionOperator.Equal,
                schemaName);

            var value = query.AddLink(EnvironmentVariableValue.EntityLogicalName,
                EnvironmentVariableDefinition.Fields.EnvironmentVariableDefinitionId,
                EnvironmentVariableValue.Fields.EnvironmentVariableDefinitionId,
                JoinOperator.LeftOuter);
            value.EntityAlias = ValueAlias;
            value.Columns = new ColumnSet(EnvironmentVariableValue.Fields.Value);

            return query;
        }
    }
}
