# Early bound classes

Early bound classes give tables and columns typed names. This repository uses a custom generated style, not the CrmSvcUtil or `pac modelbuilder` output:
- one class per table
- a nested `Fields` class of logical-name constants
- typed properties over `GetAttributeValue` and `SetAttributeValue`
- only the columns the solution actually uses

Open an existing class in the discovered early bound folder before writing one, and match it exactly. The shape below is what it looked like when this skill was written.

## Creating or extending a class

If the table already has a class, add only the columns you need:
- a constant in `Fields`
- a property
- the matching lines in its test

Don't reorder or rewrite what is there. If the table has no class, create one:

```csharp
using System;
using System.Runtime.Serialization;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace <EarlyBoundNamespace>
{
    [DataContract]
    [EntityLogicalName(EntityLogicalName)]
    public class Account : Entity
    {
        public const string EntityLogicalName = "account";

        public static class Fields
        {
            public const string AccountId = "accountid";
            public const string Name = "name";
            public const string CreditLimit = "creditlimit";
            public const string CreditOnHold = "creditonhold";
        }

        public Account() : base(EntityLogicalName) { }

        public Account(Guid id) : base(EntityLogicalName, id) { }

        [AttributeLogicalName(Fields.AccountId)]
        public override Guid Id
        {
            get => base.Id;
            set => AccountId = value;
        }

        [AttributeLogicalName(Fields.AccountId)]
        public Guid? AccountId
        {
            get => GetAttributeValue<Guid?>(Fields.AccountId);
            set
            {
                SetAttributeValue(Fields.AccountId, value);
                base.Id = value ?? Guid.Empty;
            }
        }

        [AttributeLogicalName(Fields.Name)]
        public string Name
        {
            get => GetAttributeValue<string>(Fields.Name);
            set => SetAttributeValue(Fields.Name, value);
        }

        [AttributeLogicalName(Fields.CreditLimit)]
        public Money CreditLimit
        {
            get => GetAttributeValue<Money>(Fields.CreditLimit);
            set => SetAttributeValue(Fields.CreditLimit, value);
        }

        [AttributeLogicalName(Fields.CreditOnHold)]
        public bool? CreditOnHold
        {
            get => GetAttributeValue<bool?>(Fields.CreditOnHold);
            set => SetAttributeValue(Fields.CreditOnHold, value);
        }
    }
}
```

**Getting the logical names and types right.** Logical names are lower case. Use the column's real Dataverse type:

| Column type | Property type |
|---|---|
| Single or multiple lines of text | `string` |
| Whole number | `int?` |
| Decimal | `decimal?` |
| Currency | `Money` |
| Yes/No | `bool?` |
| Date | `DateTime?` |
| Choice | `OptionSetValue` |
| Lookup | `EntityReference` |
| Unique identifier | `Guid?` |

If you are not certain of a custom column's logical name or type, ask the user. Never guess `new_` or `mgxrm_` prefixes. A wrong name compiles, passes every unit test, and fails only in Dataverse.

Platform-managed columns, such as `createdon`, `modifiedon` and calculated columns, get read-only properties (a getter only).

**The primary key.** The `Id` override and the `<Table>Id` property keep `Entity.Id` and the primary key attribute in step. Keep both, renamed for the table.

## Early bound tests

Each class has a test in the early bound test folder, named `<Table>Test`. These tests are cheap and catch typos in logical names, the commonest early bound bug. Cover:

- **The default constructor** sets the logical name, and `EntityLogicalName` matches it.
- **The Guid constructor** sets the logical name and the id.
- **Each settable property** writes the expected attribute key, e.g. `Assert.Equal(5m, ((Money)account[Account.Fields.CreditLimit]).Value)`. For a reference type, assert that the same instance comes back.
- **`Id` and the primary key property stay in step,** whichever one is set, and setting the key to `null` empties `Id`.
- **Unset attributes return `null`** rather than throwing.
- **`ToEntity<T>()` from a late-bound `Entity`** reads through the typed properties.
- **Every `Fields` constant has its literal logical name,** checked with an `[InlineData(Account.Fields.CreditLimit, "creditlimit")]` theory. This pins the spelling independently of the constant.

When you extend a class, extend each of these tests for the new columns.
