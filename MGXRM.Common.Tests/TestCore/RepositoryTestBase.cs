using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Linq;
using FakeItEasy;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace MGXRM.Common.Tests.TestCore
{
    public abstract class RepositoryTestBase<TRepository> where TRepository : IRepository
    {
        private static readonly string[] PagingAttributesAddedByFetchAll = { "page", "count", "paging-cookie" };

        protected IOrganizationService Service { get; }
        protected TRepository Repository { get; }

        protected RepositoryTestBase(Func<IRepository, TRepository> decorate)
        {
            Service = A.Fake<IOrganizationService>();
            ServiceRetrieves();
            ServiceFetches();
            Repository = decorate(new Repository(Service));
        }

        [Fact]
        public void Is_A_RepositoryDecorator()
        {
            Assert.IsAssignableFrom<RepositoryDecorator>(Repository);
        }

        [Fact]
        public void Decorates_The_Repository_It_Is_Given()
        {
            Assert.Same(Service, Repository.Service);
        }

        protected void ServiceRetrieves(params Entity[] records)
        {
            A.CallTo(() => Service.RetrieveMultiple(A<QueryBase>._)).Returns(CollectionOf(records));
        }

        protected void ServiceReturnsRecord(Entity record)
        {
            A.CallTo(() => Service.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._)).Returns(record);
        }

        protected void ServiceFetches(params Entity[] records)
        {
            var response = new RetrieveMultipleResponse();
            response.Results["EntityCollection"] = CollectionOf(records);
            A.CallTo(() => Service.Execute(A<OrganizationRequest>.That.Matches(
                    request => request is RetrieveMultipleRequest, "a RetrieveMultipleRequest")))
                .Returns(response);
        }

        protected void AssertCreated(Expression<Func<Entity, bool>> matches)
        {
            A.CallTo(() => Service.Create(A<Entity>.That.Matches(matches))).MustHaveHappenedOnceExactly();
        }

        protected void AssertUpdated(Expression<Func<Entity, bool>> matches)
        {
            A.CallTo(() => Service.Update(A<Entity>.That.Matches(matches))).MustHaveHappenedOnceExactly();
        }

        protected void AssertDeleted(string logicalName, Guid id)
        {
            A.CallTo(() => Service.Delete(logicalName, id)).MustHaveHappenedOnceExactly();
        }

        protected void AssertStatusChanged(EntityReference record, int state, int status)
        {
            A.CallTo(() => Service.Execute(A<OrganizationRequest>.That.Matches(
                    request => IsStatusChange(request, record, state, status),
                    $"a SetStateRequest moving {Describe(record)} to state {state}, status {status}")))
                .MustHaveHappenedOnceExactly();
        }

        protected void AssertAssigned(EntityReference record, EntityReference assignee)
        {
            A.CallTo(() => Service.Execute(A<OrganizationRequest>.That.Matches(
                    request => IsAssignment(request, record, assignee),
                    $"an AssignRequest giving {Describe(record)} to {Describe(assignee)}")))
                .MustHaveHappenedOnceExactly();
        }

        protected void AssertRetrieved(string logicalName, Guid id)
        {
            A.CallTo(() => Service.Retrieve(logicalName, id, A<ColumnSet>._)).MustHaveHappenedOnceExactly();
        }

        protected void AssertSingleQueryByAttribute(string entityName, string attribute, object value)
        {
            AssertSingleQueryByAttributes(entityName, new[] { attribute }, new[] { value });
        }

        protected void AssertSingleQueryByAttributes(string entityName, string[] attributes, object[] values)
        {
            A.CallTo(() => Service.RetrieveMultiple(A<QueryBase>.That.Matches(
                    query => IsQueryByAttributes(query, entityName, attributes, values),
                    $"a QueryByAttribute on {entityName} where {string.Join(", ", attributes)} = {string.Join(", ", values)}")))
                .MustHaveHappenedOnceExactly();
        }

        protected QueryExpression SingleQueryExpression()
        {
            return Assert.Single(ArgumentsSentTo(nameof(IOrganizationService.RetrieveMultiple))
                .OfType<QueryExpression>());
        }

        protected XElement SingleFetchXml()
        {
            var fetch = Assert.Single(ArgumentsSentTo(nameof(IOrganizationService.Execute))
                .OfType<RetrieveMultipleRequest>()
                .Select(request => request.Query)
                .OfType<FetchExpression>());

            var root = XElement.Parse(fetch.Query);
            root.Attributes().Where(a => PagingAttributesAddedByFetchAll.Contains(a.Name.LocalName)).Remove();
            return root;
        }

        protected void AssertNothingWritten()
        {
            A.CallTo(() => Service.Create(A<Entity>._)).MustNotHaveHappened();
            A.CallTo(() => Service.Update(A<Entity>._)).MustNotHaveHappened();
            A.CallTo(() => Service.Delete(A<string>._, A<Guid>._)).MustNotHaveHappened();
            A.CallTo(() => Service.Associate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .MustNotHaveHappened();
            A.CallTo(() => Service.Disassociate(A<string>._, A<Guid>._, A<Relationship>._, A<EntityReferenceCollection>._))
                .MustNotHaveHappened();
            A.CallTo(() => Service.Execute(A<OrganizationRequest>.That.Matches(
                    request => !(request is RetrieveMultipleRequest), "a request other than RetrieveMultipleRequest")))
                .MustNotHaveHappened();
        }

        private object[] ArgumentsSentTo(string serviceMethod)
        {
            return Fake.GetCalls(Service)
                .Where(call => call.Method.Name == serviceMethod)
                .Select(call => call.Arguments[0])
                .ToArray();
        }

        private static EntityCollection CollectionOf(Entity[] records)
        {
            var collection = new EntityCollection();
            collection.Entities.AddRange(records);
            return collection;
        }

        private static bool IsStatusChange(OrganizationRequest request, EntityReference record, int state, int status)
        {
            var setState = request as SetStateRequest;
            return setState != null
                   && SameRecord(setState.EntityMoniker, record)
                   && setState.State?.Value == state
                   && setState.Status?.Value == status;
        }

        private static bool IsAssignment(OrganizationRequest request, EntityReference record, EntityReference assignee)
        {
            var assign = request as AssignRequest;
            return assign != null
                   && SameRecord(assign.Target, record)
                   && SameRecord(assign.Assignee, assignee);
        }

        private static bool IsQueryByAttributes(QueryBase query, string entityName, string[] attributes, object[] values)
        {
            var byAttribute = query as QueryByAttribute;
            return byAttribute != null
                   && byAttribute.EntityName == entityName
                   && byAttribute.Attributes.SequenceEqual(attributes)
                   && byAttribute.Values.SequenceEqual(values);
        }

        private static bool SameRecord(EntityReference actual, EntityReference expected)
        {
            return actual != null && actual.LogicalName == expected.LogicalName && actual.Id == expected.Id;
        }

        private static string Describe(EntityReference record)
        {
            return $"{record.LogicalName} {record.Id}";
        }
    }
}
