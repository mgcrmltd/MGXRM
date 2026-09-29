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