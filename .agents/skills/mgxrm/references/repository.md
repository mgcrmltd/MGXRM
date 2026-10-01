# Repositories and repository tests

Repositories turn questions into queries. The base `IRepository` covers generic CRUD:
- `Create`, `Update` and `Delete`
- `Retrieve`, `RetrieveMultiple` and `RetrieveByAttribute(s)`
- `ChangeStatus`, `AssignRecord` and `FetchAll`

A module repository adds methods that speak the module's language, e.g. `GetByEmail` or `GetOpenCasesFor(customer)`, so the model never builds a query.

Create a module repository when the model needs a query or write beyond a single obvious `IRepository` call, or when the same data access is used twice. One repository per table per module. Extend an existing one rather than adding a second.

## The class

```csharp
using System.Collections.Generic;
using System.Linq;
using <EarlyBoundNamespace>;
using <FrameworkNamespace>.Interfaces;
using <FrameworkNamespace>.Repositories;

namespace <ModuleNamespace>.Repositories
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
```

- **Shape:** inherit `RepositoryDecorator`, implement an interface that extends `IRepository`, and take the base `IRepository` in the constructor. Never take an `IOrganizationService` directly. Taking the base repository is what lets a model pass its own repository down the chain, and lets tests insert a fake at any level.
- **Build on the inherited methods,** such as `RetrieveByAttribute`, `RetrieveMultiple` and `FetchAll`, rather than reaching for `Service`.
- **Return early bound types** (`ToEntity<T>()`), so models work with typed properties.
- **Ask only for the columns you need** (`new ColumnSet(Contact.Fields.ContactId, ...)`) in queries that run often or touch large tables. `RetrieveByAttribute` without a column set retrieves every column.
- **The interface lives in the same file** as the class, matching the existing convention.

## Repository tests: `RepositoryTestBase<TRepository>`

Repository tests are behavioural. They check how the organization service is called, using a FakeItEasy fake, rather than running queries against an in-memory CRM. The base class wires a real `Repository` over the fake service, then wraps it in the repository under test. It also gives every test class two inherited tests:
- `Is_A_RepositoryDecorator`
- `Decorates_The_Repository_It_Is_Given`

```csharp
public class ContactRepositoryTest : RepositoryTestBase<ContactRepository>
{
    private const string Email = "bob@example.com";

    public ContactRepositoryTest() : base(repository => new ContactRepository(repository))
    {
    }

    [Fact]
    public void GetByEmail_Queries_Contacts_By_Email_Address()
    {
        Repository.GetByEmail(Email);

        AssertSingleQueryByAttribute(Contact.EntityLogicalName, Contact.Fields.EmailAddress1, Email);
    }

    [Fact]
    public void GetByEmail_Writes_Nothing()
    {
        Repository.GetByEmail(Email);

        AssertNothingWritten();
    }

    [Fact]
    public void GetByEmail_Returns_Each_Retrieved_Record_As_A_Contact()
    {
        var first = new Entity(Contact.EntityLogicalName, Guid.NewGuid());
        var second = new Entity(Contact.EntityLogicalName, Guid.NewGuid());
        ServiceRetrieves(first, second);

        var contacts = Repository.GetByEmail(Email);

        Assert.Equal(new[] { first.Id, second.Id }, contacts.Select(c => c.Id));
    }

    [Fact]
    public void GetByEmail_Returns_An_Empty_List_When_No_Contact_Has_The_Email()
    {
        Assert.Empty(Repository.GetByEmail(Email));
    }
}
```

Helpers available in the base class:

| Kind of method | Arrange | Assert |
|---|---|---|
| Query by attribute(s) | `ServiceRetrieves(records...)` | `AssertSingleQueryByAttribute(entity, attr, value)`, `AssertSingleQueryByAttributes(entity, attrs[], values[])` |
| QueryExpression | `ServiceRetrieves(records...)` | `SingleQueryExpression()` returns the query, so you can assert on its criteria, links and columns |
| FetchXml (`FetchAll`) | `ServiceFetches(records...)` | `SingleFetchXml()` returns an `XElement` without the paging attributes `FetchAll` adds |
| Retrieve by id | `ServiceReturnsRecord(record)` | `AssertRetrieved(entity, id)` |
| Create / Update | | `AssertCreated(e => ...)`, `AssertUpdated(e => ...)` |
| Delete | | `AssertDeleted(entity, id)` |
| Status / assign | | `AssertStatusChanged(record, state, status)`, `AssertAssigned(record, assignee)` |
| Read-only methods | | `AssertNothingWritten()` |

By default the fake service returns empty results, so tests only arrange what they care about.

**For every repository method, test:**
- the exact service call: entity, attributes, values, and the columns if specified
- the mapping of results back into early bound records, if it is a read
- the empty case
- `AssertNothingWritten()` for every read method
- for writes, the entity passed to the service, with the attributes that matter

**Placement and naming:** put the test in the module's mirrored `Repositories` test folder, named `<Repository>Test`, with tests named `<Method>_<Behaviour>`.
