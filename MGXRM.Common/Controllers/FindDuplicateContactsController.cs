using System;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;

namespace MGXRM.Common.Controllers
{
    public class FindDuplicateContactsController : CustomApiControllerBase<Contact>
    {
        public const string EmailAddressParameter = "EmailAddress";
        public const string ExcludeContactIdParameter = "ExcludeContactId";
        public const string DuplicateCountProperty = "DuplicateCount";
        public const string FirstMatchProperty = "FirstMatch";

        private readonly IContactModel _model;

        public FindDuplicateContactsController(IServiceProvider provider) : base(provider)
        {
            _model = new ContactModel(ImageManager, ContextManager, Repository);
        }

        public FindDuplicateContactsController(IContextManager<Contact> contextManager, IContactModel model)
            : base(contextManager)
        {
            _model = model;
        }

        public override void Execute()
        {
            var email = Request.RequireString(EmailAddressParameter);
            var excludeContactId = Request.GetGuid(ExcludeContactIdParameter);

            var duplicates = _model.FindDuplicatesByEmail(email, excludeContactId);

            Response.SetInteger(DuplicateCountProperty, duplicates.Count);

            if (duplicates.Any())
                Response.SetEntityReference(FirstMatchProperty, duplicates.First().ToEntityReference());
        }
    }
}
