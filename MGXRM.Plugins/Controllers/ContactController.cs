using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;
using MGXRM.Common.Framework.Model;
using MGXRM.Common.Framework.Repositories;

namespace MGXRM.Plugins.Controllers
{
    public class ContactController : PluginControllerBase<Contact>
    {
        private IContactModel _model;

        public ContactController(IServiceProvider provider) : base(provider)
        {
            _model = new ContactModel(ImageManager, ContextManager, Repository, new ContactRepository(Repository));
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
