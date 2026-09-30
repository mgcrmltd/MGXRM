using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Model;

namespace MGXRM.Plugins.Controllers
{
    public class FindDuplicateContactsController : CustomApiControllerBase<Contact>
    {
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
            _model.FindDuplicatesByEmail();
        }
    }
}
