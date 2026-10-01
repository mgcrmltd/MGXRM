using System.Collections.Generic;
using System.Linq;
using MGXRM.Common.EarlyBounds;
using MGXRM.Common.Framework.Interfaces;

namespace MGXRM.Common.Framework.Repositories
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
