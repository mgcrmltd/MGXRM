using MGXRM.Common.EarlyBounds;
using System.Linq;
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
    }
    public class ContactModel : ModelBase<Contact>, IContactModel
    {
        private readonly IContactRepository _contacts;

        /// <summary>
        /// Wires up its own contact repository. This is the constructor the controller uses, and the one a
        /// test uses when it is happy for the real queries to run - against a seeded CRM, for instance.
        /// </summary>
        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository)
            : this(images, context, repository, new ContactRepository(repository))
        {
        }

        /// <summary>
        /// Takes the contact repository instead of building one, for a test that wants to arrange what the
        /// queries return. Add a constructor like this for each dependency that turns out to need faking.
        /// </summary>
        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository,
            IContactRepository contacts) : base(images, context, repository)
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

        /// <summary>
        /// Rejects an email address another contact is already using. The record being updated is excluded,
        /// otherwise an update that does not change the email would reject itself.
        /// </summary>
        public void EnforceEmailIsUnique()
        {
            if (!Images.IsBeingSetOrUpdated(Contact.Fields.EmailAddress1)) return;

            var email = Images.GetLatestString(Contact.Fields.EmailAddress1);
            if (string.IsNullOrWhiteSpace(email)) return;

            if (_contacts.GetByEmail(email).Any(c => c.Id != Context.PrimaryEntityId))
                throw new InvalidPluginExecutionException($"{email} is already used by another contact.");
        }
    }
}