using MGXRM.Common.EarlyBounds;
using System.Linq;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Framework.Repositories;
using MGXRM.Common.Modules.CustomerSupport.Repositories;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Modules.CustomerSupport.Models
{
    public interface IContactModel
    {
        void MakeSurnameUppercase();
        void EnforceSurnameRequired();
        void EnforceEmailIsUnique();
    }
    public class ContactModel : ModelBase<Contact>, IContactModel
    {
        public const string EnforceUniqueEmailVariable = "mgxrm_EnforceUniqueContactEmail";

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

            var otherContacts = _contacts.GetByEmail(email).Where(c => c.Id != Context.PrimaryEntityId);
            if (otherContacts.Any())
                throw new InvalidPluginExecutionException($"{email} is already used by another contact.");
        }
    }
}