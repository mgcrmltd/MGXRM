using System;
using MGXRM.Common.Controllers;

namespace MGXRM.Plugins.PluginExecution
{
    [CrmPluginRegistration(FindDuplicateContacts.UniqueName,
        Id = "1ef533ea-2baf-4ecf-8ca1-992a29c8c8a9")]
    public class FindDuplicateContacts : CustomApi
    {
        public const string UniqueName = "mgxrm_FindDuplicateContacts";

        public FindDuplicateContacts() : base(typeof(FindDuplicateContacts), UniqueName)
        {
        }

        protected override void ExecuteCustomApi(LocalPluginContext localContext)
        {
            if (localContext == null)
                throw new ArgumentNullException(nameof(localContext));

            new FindDuplicateContactsController(localContext.ServiceProvider).Execute();
        }
    }
}
