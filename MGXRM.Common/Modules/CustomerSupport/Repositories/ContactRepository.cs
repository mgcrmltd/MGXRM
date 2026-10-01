using System.Collections.Generic;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;

namespace MGXRM.Common.Modules.CustomerSupport.Repositories
{
    public interface IContactRepository : IRepository
    {
        List<Contact> GetByEmail(string email);
    }

    public class ContactRepository : RepositoryDecorator, IContactRepository
    {
        public ContactRepository(IRepository repository) : base(repository)
        {
        }

        public List<Contact> GetByEmail(string email)
        {
            return RetrieveByAttribute(Contact.EntityLogicalName, Contact.Fields.EmailAddress1, email)
                .Select(c => c.ToEntity<Contact>())
                .ToList();
        }
    }
}
