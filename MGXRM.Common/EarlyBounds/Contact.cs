using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace MGXRM.Common.EarlyBounds
{
    /// <summary>
    /// Hand-written early bound type for the contact entity. Only the attributes
    /// this solution actually uses are declared - add to <see cref="Fields"/> and
    /// the property list below as more are needed.
    /// </summary>
    [DataContract]
    [EntityLogicalName(EntityLogicalName)]
    public class Contact : Entity
    {
        public const string EntityLogicalName = "contact";

        /// <summary>
        /// Logical names of the declared columns, for use with ColumnSet, QueryExpression,
        /// and the IImageManager/IRepository methods that take an attribute name.
        /// </summary>
        public static class Fields
        {
            public const string ContactId = "contactid";
            public const string FirstName = "firstname";
            public const string MiddleName = "middlename";
            public const string LastName = "lastname";
            public const string FullName = "fullname";
            public const string EmailAddress1 = "emailaddress1";
            public const string Telephone1 = "telephone1";
            public const string MobilePhone = "mobilephone";
            public const string ParentCustomerId = "parentcustomerid";
            public const string OwnerId = "ownerid";
            public const string StateCode = "statecode";
            public const string StatusCode = "statuscode";
            public const string CreatedOn = "createdon";
            public const string ModifiedOn = "modifiedon";
        }

        public Contact() : base(EntityLogicalName) { }

        public Contact(Guid id) : base(EntityLogicalName, id) { }

        [AttributeLogicalName(Fields.ContactId)]
        public override Guid Id
        {
            get => base.Id;
            set => ContactId = value;
        }

        [AttributeLogicalName(Fields.ContactId)]
        public Guid? ContactId
        {
            get => GetAttributeValue<Guid?>(Fields.ContactId);
            set
            {
                SetAttributeValue(Fields.ContactId, value);
                base.Id = value ?? Guid.Empty;
            }
        }

        [AttributeLogicalName(Fields.FirstName)]
        public string FirstName
        {
            get => GetAttributeValue<string>(Fields.FirstName);
            set => SetAttributeValue(Fields.FirstName, value);
        }

        [AttributeLogicalName(Fields.MiddleName)]
        public string MiddleName
        {
            get => GetAttributeValue<string>(Fields.MiddleName);
            set => SetAttributeValue(Fields.MiddleName, value);
        }

        [AttributeLogicalName(Fields.LastName)]
        public string LastName
        {
            get => GetAttributeValue<string>(Fields.LastName);
            set => SetAttributeValue(Fields.LastName, value);
        }

        /// <summary>Calculated server side from the name parts - read only.</summary>
        [AttributeLogicalName(Fields.FullName)]
        public string FullName => GetAttributeValue<string>(Fields.FullName);

        [AttributeLogicalName(Fields.EmailAddress1)]
        public string EmailAddress1
        {
            get => GetAttributeValue<string>(Fields.EmailAddress1);
            set => SetAttributeValue(Fields.EmailAddress1, value);
        }

        [AttributeLogicalName(Fields.Telephone1)]
        public string Telephone1
        {
            get => GetAttributeValue<string>(Fields.Telephone1);
            set => SetAttributeValue(Fields.Telephone1, value);
        }

        [AttributeLogicalName(Fields.MobilePhone)]
        public string MobilePhone
        {
            get => GetAttributeValue<string>(Fields.MobilePhone);
            set => SetAttributeValue(Fields.MobilePhone, value);
        }

        [AttributeLogicalName(Fields.ParentCustomerId)]
        public EntityReference ParentCustomerId
        {
            get => GetAttributeValue<EntityReference>(Fields.ParentCustomerId);
            set => SetAttributeValue(Fields.ParentCustomerId, value);
        }

        [AttributeLogicalName(Fields.OwnerId)]
        public EntityReference OwnerId
        {
            get => GetAttributeValue<EntityReference>(Fields.OwnerId);
            set => SetAttributeValue(Fields.OwnerId, value);
        }

        [AttributeLogicalName(Fields.StateCode)]
        public OptionSetValue StateCode
        {
            get => GetAttributeValue<OptionSetValue>(Fields.StateCode);
            set => SetAttributeValue(Fields.StateCode, value);
        }

        [AttributeLogicalName(Fields.StatusCode)]
        public OptionSetValue StatusCode
        {
            get => GetAttributeValue<OptionSetValue>(Fields.StatusCode);
            set => SetAttributeValue(Fields.StatusCode, value);
        }

        /// <summary>Set by the platform on create - read only.</summary>
        [AttributeLogicalName(Fields.CreatedOn)]
        public DateTime? CreatedOn => GetAttributeValue<DateTime?>(Fields.CreatedOn);

        /// <summary>Set by the platform on update - read only.</summary>
        [AttributeLogicalName(Fields.ModifiedOn)]
        public DateTime? ModifiedOn => GetAttributeValue<DateTime?>(Fields.ModifiedOn);
    }
}
