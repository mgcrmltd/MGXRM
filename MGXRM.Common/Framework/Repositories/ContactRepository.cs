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

    /// <summary>
    /// Adds contact specific queries on top of whatever <see cref="IRepository"/> it is given, so a model
    /// can ask a question in its own terms instead of assembling a query. Decorating rather than inheriting
    /// means a test can hand it either a faked repository or a real one over an in-memory CRM.
    /// </summary>
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
