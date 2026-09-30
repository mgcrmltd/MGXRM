using System;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.CustomApi
{
    public interface ICustomApiRequest
    {
        bool Contains(string name);

        EntityReference Target { get; }

        string GetString(string name);
        bool? GetBoolean(string name);
        int? GetInteger(string name);
        decimal? GetDecimal(string name);
        double? GetFloat(string name);
        DateTime? GetDateTime(string name);
        Guid? GetGuid(string name);
        Money GetMoney(string name);
        OptionSetValue GetOptionSet(string name);
        EntityReference GetEntityReference(string name);
        Entity GetEntity(string name);
        EntityCollection GetEntityCollection(string name);
        string[] GetStringArray(string name);

        EntityReference RequireTarget();
        string RequireString(string name);
        bool RequireBoolean(string name);
        int RequireInteger(string name);
        decimal RequireDecimal(string name);
        DateTime RequireDateTime(string name);
        Guid RequireGuid(string name);
        Money RequireMoney(string name);
        OptionSetValue RequireOptionSet(string name);
        EntityReference RequireEntityReference(string name);
        Entity RequireEntity(string name);
        EntityCollection RequireEntityCollection(string name);
        string[] RequireStringArray(string name);
    }

    public class CustomApiRequest : ICustomApiRequest
    {
        public const string TargetParameter = "Target";

        private readonly ParameterCollection _parameters;

        public CustomApiRequest(ParameterCollection parameters)
        {
            _parameters = parameters;
        }

        public bool Contains(string name)
        {
            return _parameters != null && _parameters.Contains(name) && _parameters[name] != null;
        }

        public EntityReference Target => GetEntityReference(TargetParameter);

        public string GetString(string name) => Of<string>(name);
        public bool? GetBoolean(string name) => NullableOf<bool>(name);
        public int? GetInteger(string name) => NullableOf<int>(name);
        public decimal? GetDecimal(string name) => NullableOf<decimal>(name);
        public double? GetFloat(string name) => NullableOf<double>(name);
        public DateTime? GetDateTime(string name) => NullableOf<DateTime>(name);
        public Guid? GetGuid(string name) => NullableOf<Guid>(name);
        public Money GetMoney(string name) => Of<Money>(name);
        public OptionSetValue GetOptionSet(string name) => Of<OptionSetValue>(name);
        public EntityReference GetEntityReference(string name) => Of<EntityReference>(name);
        public Entity GetEntity(string name) => Of<Entity>(name);
        public EntityCollection GetEntityCollection(string name) => Of<EntityCollection>(name);
        public string[] GetStringArray(string name) => Of<string[]>(name);

        public EntityReference RequireTarget() => RequireEntityReference(TargetParameter);

        public string RequireString(string name)
        {
            var value = GetString(name);
            if (string.IsNullOrWhiteSpace(value))
                throw Missing(name);
            return value;
        }

        public bool RequireBoolean(string name) => Required(NullableOf<bool>(name), name);
        public int RequireInteger(string name) => Required(NullableOf<int>(name), name);
        public decimal RequireDecimal(string name) => Required(NullableOf<decimal>(name), name);
        public DateTime RequireDateTime(string name) => Required(NullableOf<DateTime>(name), name);
        public Guid RequireGuid(string name) => Required(NullableOf<Guid>(name), name);
        public Money RequireMoney(string name) => Required(Of<Money>(name), name);
        public OptionSetValue RequireOptionSet(string name) => Required(Of<OptionSetValue>(name), name);
        public EntityReference RequireEntityReference(string name) => Required(Of<EntityReference>(name), name);
        public Entity RequireEntity(string name) => Required(Of<Entity>(name), name);
        public EntityCollection RequireEntityCollection(string name) => Required(Of<EntityCollection>(name), name);
        public string[] RequireStringArray(string name) => Required(Of<string[]>(name), name);

        private TValue Of<TValue>(string name) where TValue : class
        {
            if (!Contains(name))
                return null;

            var value = _parameters[name];
            var typed = value as TValue;
            if (typed == null)
                throw WrongType(name, value, typeof(TValue));

            return typed;
        }

        private TValue? NullableOf<TValue>(string name) where TValue : struct
        {
            if (!Contains(name))
                return null;

            var value = _parameters[name];
            if (!(value is TValue))
                throw WrongType(name, value, typeof(TValue));

            return (TValue)value;
        }

        private static TValue Required<TValue>(TValue? value, string name) where TValue : struct
        {
            if (!value.HasValue)
                throw Missing(name);
            return value.Value;
        }

        private static TValue Required<TValue>(TValue value, string name) where TValue : class
        {
            if (value == null)
                throw Missing(name);
            return value;
        }

        private static InvalidPluginExecutionException Missing(string name)
        {
            return new InvalidPluginExecutionException($"Request parameter '{name}' is required but was not supplied.");
        }

        private static InvalidPluginExecutionException WrongType(string name, object value, Type expected)
        {
            return new InvalidPluginExecutionException(
                $"Request parameter '{name}' is a {value.GetType().Name}, not a {expected.Name}.");
        }
    }
}
