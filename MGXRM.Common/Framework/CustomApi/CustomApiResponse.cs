using System;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.CustomApi
{
    public interface ICustomApiResponse
    {
        bool Contains(string name);

        void SetString(string name, string value);
        void SetBoolean(string name, bool value);
        void SetInteger(string name, int value);
        void SetDecimal(string name, decimal value);
        void SetFloat(string name, double value);
        void SetDateTime(string name, DateTime value);
        void SetGuid(string name, Guid value);
        void SetMoney(string name, Money value);
        void SetOptionSet(string name, OptionSetValue value);
        void SetEntityReference(string name, EntityReference value);
        void SetEntity(string name, Entity value);
        void SetEntityCollection(string name, EntityCollection value);
        void SetStringArray(string name, string[] value);
    }

    public class CustomApiResponse : ICustomApiResponse
    {
        private readonly ParameterCollection _parameters;

        public CustomApiResponse(ParameterCollection parameters)
        {
            _parameters = parameters;
        }

        public bool Contains(string name)
        {
            return _parameters != null && _parameters.Contains(name);
        }

        public void SetString(string name, string value) => Set(name, value);
        public void SetBoolean(string name, bool value) => Set(name, value);
        public void SetInteger(string name, int value) => Set(name, value);
        public void SetDecimal(string name, decimal value) => Set(name, value);
        public void SetFloat(string name, double value) => Set(name, value);
        public void SetDateTime(string name, DateTime value) => Set(name, value);
        public void SetGuid(string name, Guid value) => Set(name, value);
        public void SetMoney(string name, Money value) => Set(name, value);
        public void SetOptionSet(string name, OptionSetValue value) => Set(name, value);
        public void SetEntityReference(string name, EntityReference value) => Set(name, value);
        public void SetEntity(string name, Entity value) => Set(name, value);
        public void SetEntityCollection(string name, EntityCollection value) => Set(name, value);
        public void SetStringArray(string name, string[] value) => Set(name, value);

        private void Set(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("A response property name is required.", nameof(name));

            if (_parameters == null)
                throw new InvalidPluginExecutionException(
                    $"There are no output parameters to set '{name}' on.");

            _parameters[name] = value;
        }
    }
}
