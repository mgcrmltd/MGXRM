using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using MGXRM.Common.Modules.CustomerSupport.Models;

namespace MGXRM.Common.Modules.CustomerSupport.Controllers
{
    public class ContactController : PluginControllerBase<Contact>
    {
        private IContactModel _model;

        public ContactController(IServiceProvider provider) : base(provider)
        {
            _model = new ContactModel(ImageManager, ContextManager, Repository);
        }

        public ContactController(IContactModel fakeModel) : base()
        {
            _model = fakeModel;
        }

        public override void PreCreate()
        {
            _model.MakeSurnameUppercase();
            _model.EnforceEmailIsUnique();
        }

        public override void PreUpdate()
        {
            _model.MakeSurnameUppercase();
            _model.EnforceEmailIsUnique();
        }

        public override void PostUpdateSync()
        {
            _model.EnforceSurnameRequired();
        }
    }
}
