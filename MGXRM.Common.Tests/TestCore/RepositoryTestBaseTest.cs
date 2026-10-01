using System;
using System.Linq;
using FakeItEasy;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;
using Xunit.Sdk;

namespace MGXRM.Common.Tests.TestCore
{
    public class RepositoryTestBaseTest : RepositoryTestBase<RepositoryDecorator>
    {
        private const string EntityName = "account";
        private const string FetchXml = "<fetch><entity name='account'><attribute name='name' /></entity></fetch>";

        private static readonly EntityReference Record = new EntityReference(EntityName, Guid.NewGuid());
        private static readonly EntityReference OtherRecord = new EntityReference(EntityName, Guid.NewGuid());

        public RepositoryTestBaseTest() : base(repository => new RepositoryDecorator(repository))
        {
        }

        #region Arranging what the service returns

        [Fact]
        public void A_Query_Returns_Nothing_Until_Records_Are_Arranged()
        {
            Assert.Empty(Repository.RetrieveByAttribute(EntityName, "name", "acme"));
        }

        [Fact]
        public void ServiceRetrieves_Sets_What_A_Query_Returns()
        {
            var record = new Entity(EntityName, Guid.NewGuid());
            ServiceRetrieves(record);

            Assert.Equal(record.Id, Assert.Single(Repository.RetrieveByAttribute(EntityName, "name", "acme")).Id);
        }

        [Fact]
        public void ServiceReturnsRecord_Sets_What_Retrieve_Returns()
        {
            var record = new Entity(EntityName, Guid.NewGuid());
            ServiceReturnsRecord(record);

            Assert.Same(record, Repository.Retrieve(EntityName, record.Id, new ColumnSet(true)));
        }

        [Fact]
        public void FetchAll_Returns_Nothing_Until_Records_Are_Arranged()
        {
            Assert.Empty(Repository.FetchAll(FetchXml));
        }

        [Fact]
        public void ServiceFetches_Sets_What_FetchAll_Returns()
        {
            var record = new Entity(EntityName, Guid.NewGuid());
            ServiceFetches(record);

            Assert.Equal(record.Id, Assert.Single(Repository.FetchAll(FetchXml)).Id);
        }

        #endregion

        #region Writes

        [Fact]
        public void AssertCreated_Passes_When_A_Matching_Record_Was_Created()
        {
            Repository.Create(new Entity(EntityName) { ["name"] = "acme" });

            AssertCreated(e => e.LogicalName == EntityName && (string)e["name"] == "acme");
        }

        [Fact]
        public void AssertCreated_Fails_When_Nothing_Was_Created()
        {
            Assert.Throws<ExpectationException>(() => AssertCreated(e => e.LogicalName == EntityName));
        }

        [Fact]
        public void AssertCreated_Fails_When_The_Created_Record_Does_Not_Match()
        {
            Repository.Create(new Entity(EntityName) { ["name"] = "acme" });

            Assert.Throws<ExpectationException>(() => AssertCreated(e => (string)e["name"] == "other"));
        }

        [Fact]
        public void AssertUpdated_Passes_When_A_Matching_Record_Was_Updated()
        {
            Repository.Update(new Entity(EntityName, Record.Id) { ["name"] = "acme" });

            AssertUpdated(e => e.Id == Record.Id && (string)e["name"] == "acme");
        }

        [Fact]
        public void AssertUpdated_Fails_When_Nothing_Was_Updated()
        {
            Assert.Throws<ExpectationException>(() => AssertUpdated(e => e.Id == Record.Id));
        }

        [Fact]
        public void AssertUpdated_Fails_When_The_Updated_Record_Does_Not_Match()
        {
            Repository.Update(new Entity(EntityName, Record.Id));

            Assert.Throws<ExpectationException>(() => AssertUpdated(e => e.Id == OtherRecord.Id));
        }

        [Fact]
        public void AssertDeleted_Passes_When_An_Entity_Was_Deleted()
        {
            Repository.Delete(new Entity(EntityName, Record.Id));

            AssertDeleted(EntityName, Record.Id);
        }

        [Fact]
        public void AssertDeleted_Passes_When_A_Reference_Was_Deleted()
        {
            Repository.Delete(Record);

            AssertDeleted(EntityName, Record.Id);
        }

        [Fact]
        public void AssertDeleted_Fails_When_A_Different_Record_Was_Deleted()
        {
            Repository.Delete(OtherRecord);

            Assert.Throws<ExpectationException>(() => AssertDeleted(EntityName, Record.Id));
        }

        [Fact]
        public void AssertStatusChanged_Passes_When_The_Status_Was_Changed()
        {
            Repository.ChangeStatus(new Entity(EntityName, Record.Id), 1, 2);

            AssertStatusChanged(Record, 1, 2);
        }

        [Fact]
        public void AssertStatusChanged_Fails_When_Nothing_Was_Changed()
        {
            Assert.Throws<ExpectationException>(() => AssertStatusChanged(Record, 1, 2));
        }

        [Fact]
        public void AssertStatusChanged_Fails_When_The_Status_Differs()
        {
            Repository.ChangeStatus(new Entity(EntityName, Record.Id), 1, 3);

            Assert.Throws<ExpectationException>(() => AssertStatusChanged(Record, 1, 2));
        }

        [Fact]
        public void AssertStatusChanged_Fails_When_A_Different_Record_Was_Changed()
        {
            Repository.ChangeStatus(new Entity(EntityName, OtherRecord.Id), 1, 2);

            Assert.Throws<ExpectationException>(() => AssertStatusChanged(Record, 1, 2));
        }

        [Fact]
        public void AssertAssigned_Passes_When_The_Record_Was_Assigned()
        {
            var user = new EntityReference("systemuser", Guid.NewGuid());
            Repository.AssignRecord(Record, user);

            AssertAssigned(Record, user);
        }

        [Fact]
        public void AssertAssigned_Fails_When_Nothing_Was_Assigned()
        {
            Assert.Throws<ExpectationException>(
                () => AssertAssigned(Record, new EntityReference("systemuser", Guid.NewGuid())));
        }

        [Fact]
        public void AssertAssigned_Fails_When_Assigned_To_Someone_Else()
        {
            Repository.AssignRecord(Record, new EntityReference("systemuser", Guid.NewGuid()));

            Assert.Throws<ExpectationException>(
                () => AssertAssigned(Record, new EntityReference("systemuser", Guid.NewGuid())));
        }

        #endregion

        #region Reads

        [Fact]
        public void AssertRetrieved_Passes_When_The_Record_Was_Retrieved()
        {
            Repository.Retrieve(EntityName, Record.Id, new ColumnSet(true));

            AssertRetrieved(EntityName, Record.Id);
        }

        [Fact]
        public void AssertRetrieved_Fails_When_A_Different_Record_Was_Retrieved()
        {
            Repository.Retrieve(EntityName, OtherRecord.Id, new ColumnSet(true));

            Assert.Throws<ExpectationException>(() => AssertRetrieved(EntityName, Record.Id));
        }

        [Fact]
        public void AssertSingleQueryByAttribute_Passes_When_Queried_By_That_Attribute()
        {
            Repository.RetrieveByAttribute(EntityName, "name", "acme");

            AssertSingleQueryByAttribute(EntityName, "name", "acme");
        }

        [Fact]
        public void AssertSingleQueryByAttribute_Fails_When_The_Value_Differs()
        {
            Repository.RetrieveByAttribute(EntityName, "name", "acme");

            Assert.Throws<ExpectationException>(() => AssertSingleQueryByAttribute(EntityName, "name", "other"));
        }

        [Fact]
        public void AssertSingleQueryByAttributes_Passes_When_Queried_By_Those_Attributes()
        {
            Repository.RetrieveByAttributes(EntityName, new[] { "name", "statecode" }, new object[] { "acme", 0 });

            AssertSingleQueryByAttributes(EntityName, new[] { "name", "statecode" }, new object[] { "acme", 0 });
        }

        [Fact]
        public void AssertSingleQueryByAttributes_Fails_When_The_Query_Has_Other_Attributes()
        {
            Repository.RetrieveByAttributes(EntityName, new[] { "name", "statecode" }, new object[] { "acme", 0 });

            Assert.Throws<ExpectationException>(() => AssertSingleQueryByAttribute(EntityName, "name", "acme"));
        }

        [Fact]
        public void AssertSingleQueryByAttributes_Fails_When_Queried_On_Another_Entity()
        {
            Repository.RetrieveByAttributes("contact", new[] { "name" }, new object[] { "acme" });

            Assert.Throws<ExpectationException>(
                () => AssertSingleQueryByAttributes(EntityName, new[] { "name" }, new object[] { "acme" }));
        }

        [Fact]
        public void SingleQueryExpression_Returns_The_Query_Sent()
        {
            var sent = new QueryExpression(EntityName);
            sent.Criteria.AddCondition("name", ConditionOperator.Equal, "acme");
            Repository.RetrieveMultiple(sent);

            var query = SingleQueryExpression();

            Assert.Equal(EntityName, query.EntityName);
            Assert.Equal("acme", query.Criteria.Conditions.Single().Values.Single());
        }

        [Fact]
        public void SingleQueryExpression_Fails_When_No_Query_Was_Sent()
        {
            Assert.ThrowsAny<XunitException>(() => SingleQueryExpression());
        }

        [Fact]
        public void SingleQueryExpression_Fails_When_More_Than_One_Query_Was_Sent()
        {
            Repository.RetrieveMultiple(new QueryExpression(EntityName));
            Repository.RetrieveMultiple(new QueryExpression(EntityName));

            Assert.ThrowsAny<XunitException>(() => SingleQueryExpression());
        }

        [Fact]
        public void SingleFetchXml_Returns_The_Fetch_Without_The_Paging_FetchAll_Adds()
        {
            Repository.FetchAll(FetchXml);

            var fetch = SingleFetchXml();

            Assert.Empty(fetch.Attributes());
            Assert.Equal(EntityName, (string)fetch.Element("entity").Attribute("name"));
        }

        [Fact]
        public void SingleFetchXml_Fails_When_Nothing_Was_Fetched()
        {
            Assert.ThrowsAny<XunitException>(() => SingleFetchXml());
        }

        #endregion

        #region Nothing written

        [Fact]
        public void AssertNothingWritten_Passes_After_Only_Reads()
        {
            Repository.RetrieveByAttribute(EntityName, "name", "acme");
            Repository.RetrieveMultiple(new QueryExpression(EntityName));
            Repository.Retrieve(EntityName, Record.Id, new ColumnSet(true));
            Repository.FetchAll(FetchXml);

            AssertNothingWritten();
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_A_Create()
        {
            Repository.Create(new Entity(EntityName));

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_An_Update()
        {
            Repository.Update(new Entity(EntityName, Record.Id));

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_A_Delete()
        {
            Repository.Delete(Record);

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_A_Status_Change()
        {
            Repository.ChangeStatus(new Entity(EntityName, Record.Id), 1, 2);

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_An_Assignment()
        {
            Repository.AssignRecord(Record, new EntityReference("systemuser", Guid.NewGuid()));

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        [Fact]
        public void AssertNothingWritten_Fails_After_An_Associate()
        {
            Service.Associate(EntityName, Record.Id, new Relationship("account_contacts"), new EntityReferenceCollection());

            Assert.Throws<ExpectationException>(() => AssertNothingWritten());
        }

        #endregion
    }
}
