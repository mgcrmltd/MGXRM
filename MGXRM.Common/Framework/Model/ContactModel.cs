using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;

namespace MGXRM.Common.Framework.Model
{
    public interface IContactModel
    {
        void MakeSurnameUppercase();
        void EnforceSurnameRequired();
    }
    public class ContactModel : ModelBase<Contact>, IContactModel 
    {
        public ContactModel(IImageManager<Contact> images, IContextManager<Contact> context, IRepository repository) : base(images, context, repository)
        {
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
    }
}