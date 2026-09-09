using System;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Controller;

namespace MGXRM.Plugins.Controllers
{
    public class ContactController : PluginControllerBase<Contact>
    {
        public ContactController(IServiceProvider provider) : base(provider) { }
    }
}
