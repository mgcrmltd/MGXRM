using System;
using System.Collections.Generic;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Model
{
    public interface IContactModel
    {
        void MakeSurnameUppercase();
        void EnforceSurnameRequired();
        void EnforceEmailIsUnique();
        void FindDuplicatesByEmail();
    }
    public class ContactModel : ModelBase<Contact>, IContactModel
    {
        public const string EnforceUniqueEmailVariable = "mgxrm_EnforceUniqueContactEmail";

        public const string EmailAddressParameter = "EmailAddress";
        public const string ExcludeContactIdParameter = "ExcludeContactId";
        public const string DuplicateCountProperty = "DuplicateCount";
        public const string FirstMatchProperty = "FirstMatch";

        private readonly IContactRepository _contacts;

        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository)
            : this(images, context, repository, new ContactRepository(repository))
        {
        }

        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository,
            IContactRepository contacts)
            : this(images, context, repository, contacts, new EnvironmentVariableRepository(repository))
        {
        }

        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository,
            IContactRepository contacts, IEnvironmentVariableRepository environmentVariables)
            : base(images, context, repository, environmentVariables)
        {
            _contacts = contacts;
        }

        public void MakeSurnameUppercase()
        {
            if (!Images.IsBeingSetOrUpdated(Contact.Fields.LastName)) return;
            if (Images.IsBeingSetAsNull(Contact.Fields.LastName)) return;
            
            Images.SetOrUpdate(Contact.Fields.LastName, 
                Images.GetLatestString(Contact.Fields.LastName).ToUpper());
        }

        public void EnforceSurnameRequired()
        {
            if(Images.IsBeingSetAsNull(Contact.Fields.LastName)) throw new InvalidPluginExecutionException();
        }

        public void EnforceEmailIsUnique()
        {
            if (!Images.IsBeingSetOrUpdated(Contact.Fields.EmailAddress1)) return;

            var email = Images.GetLatestString(Contact.Fields.EmailAddress1);
            if (string.IsNullOrWhiteSpace(email)) return;

            if (EnvironmentVariables.GetBoolean(EnforceUniqueEmailVariable) == false) return;

            if (DuplicatesByEmail(email, Context.PrimaryEntityId).Any())
                throw new InvalidPluginExecutionException($"{email} is already used by another contact.");
        }

        public void FindDuplicatesByEmail()
        {
            var email = Request.RequireString(EmailAddressParameter);
            var excludeContactId = Request.GetGuid(ExcludeContactIdParameter);

            var duplicates = DuplicatesByEmail(email, excludeContactId);

            Response.SetInteger(DuplicateCountProperty, duplicates.Count);

            if (duplicates.Any())
                Response.SetEntityReference(FirstMatchProperty, duplicates.First().ToEntityReference());
        }

        private List<Contact> DuplicatesByEmail(string email, Guid? excludeContactId)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new List<Contact>();

            return _contacts.GetByEmail(email)
                .Where(c => !excludeContactId.HasValue || c.Id != excludeContactId.Value)
                .ToList();
        }
    }
}