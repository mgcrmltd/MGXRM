using System;
using System.Collections.Generic;
using FakeItEasy;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Repositories
{
    public class RepositoryTest
    {
        protected const string EntityName = "mgxrm_Entity";
        protected const string FieldName = "mgxrm_description";

        [Fact]
        public void Service_Returns_Passed_In_Service()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            IRepository repo = new Repository(fakeOrgService);
            Assert.Equal(repo.Service, fakeOrgService);
        }

        [Fact]
        public void Create_Calls_Service_Create()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var id = Guid.NewGuid();
            A.CallTo(() => fakeOrgService.Create(A<Entity>._)).Returns(id);
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName);
            var createId = repo.Create(entity);
            A.CallTo(() => fakeOrgService.Create(entity)).MustHaveHappened();
            Assert.Equal(id, createId);
        }

        [Fact]
        public void Create_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Create(A<Entity>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.Create(entity));
        }
        
        [Fact]
        public void Delete_Calls_Service_Delete_Correctly()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Delete(A<string>._, A<Guid>._));
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName);
            repo.Delete(entity);
            A.CallTo(() => fakeOrgService.Delete(entity.LogicalName, entity.Id)).MustHaveHappened();
        }

        [Fact]
        public void Delete_EntityRef_Calls_Service_Delete_Correctly()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Delete(A<string>._, A<Guid>._));
            IRepository repo = new Repository(fakeOrgService);
            var entityRef = new EntityReference(EntityName, Guid.NewGuid());
            repo.Delete(entityRef);
            A.CallTo(() => fakeOrgService.Delete(entityRef.LogicalName, entityRef.Id)).MustHaveHappened();
        }

        [Fact]
        public void Delete_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Delete(A<string>._, A<Guid>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            var entityRef = new EntityReference(EntityName, Guid.NewGuid());
            Assert.Throws<InvalidPluginExecutionException>(() => repo.Delete(entityRef));
        }

        [Fact]
        public void Update_Calls_Service_Update()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Update(A<Entity>._));
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName);
            repo.Update(entity);
            A.CallTo(() => fakeOrgService.Update(entity)).MustHaveHappened();
        }

        [Fact]
        public void Update_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Update(A<Entity>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.Update(entity));
        }

        [Fact]
        public void Retrieve_Calls_Service_Retrieve()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var id = Guid.NewGuid();
            A.CallTo(() => fakeOrgService.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._)).Returns(new Entity(EntityName));
            IRepository repo = new Repository(fakeOrgService);
            repo.Retrieve(EntityName, id, new ColumnSet(FieldName));
            A.CallTo(() => fakeOrgService.Retrieve(EntityName,id,A<ColumnSet>.That.Matches(
                x => x.Columns.Count == 1 && x.Columns.Contains(FieldName)))).MustHaveHappened();
        }

        [Fact]
        public void Retrieve_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Retrieve(A<string>._, A<Guid>._, A<ColumnSet>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            var id = Guid.NewGuid();
            Assert.Throws<InvalidPluginExecutionException>(() => repo.Retrieve(EntityName, id, new ColumnSet(FieldName)));
        }

        [Fact]
        public void RetrieveMultiple_Returns_List()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var collection = new EntityCollection();
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Returns(collection);
            IRepository repo = new Repository(fakeOrgService);
            var query = new QueryExpression();
            var list = repo.RetrieveMultiple(query);
            A.CallTo(() => fakeOrgService.RetrieveMultiple(query)).MustHaveHappened();
            Assert.IsType<List<Entity>>(list);
            Assert.Equal(collection.Entities.Count, list.Count);
        }

        [Fact]
        public void RetrieveMultiple_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            var id = Guid.NewGuid();
            var query = new QueryExpression();
            Assert.Throws<InvalidPluginExecutionException>(() => repo.RetrieveMultiple(query));
        }

        [Fact]
        public void RetrieveByAttribute_Forms_Query_And_Returns_List()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var collection = new EntityCollection();
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Returns(collection);
            IRepository repo = new Repository(fakeOrgService);
            var list = repo.RetrieveByAttribute(EntityName, FieldName, "val");
            A.CallTo(() => fakeOrgService.RetrieveMultiple((A<QueryByAttribute>.That.Matches(
                x => x.EntityName == EntityName
                     && x.Attributes.Count == 1
                     && x.Attributes.Contains(FieldName)
                     && x.Values.Count == 1
                     && x.Values.Contains("val") 
                     && x.ColumnSet.AllColumns)))).MustHaveHappened();

            Assert.IsType<List<Entity>>(list);
            Assert.Equal(collection.Entities.Count, list.Count);
        }

        [Fact]
        public void RetrieveByAttribute_Takes_ColumnSet()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var collection = new EntityCollection();
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Returns(collection);
            IRepository repo = new Repository(fakeOrgService);
            var list = repo.RetrieveByAttribute(EntityName, FieldName, "val", new ColumnSet("name"));
            A.CallTo(() => fakeOrgService.RetrieveMultiple((A<QueryByAttribute>.That.Matches(
                x => x.EntityName == EntityName
                     && x.Attributes.Count == 1
                     && x.Attributes.Contains(FieldName)
                     && x.Values.Count == 1
                     && x.Values.Contains("val") 
                     && x.ColumnSet.Columns.Count == 1
                     && x.ColumnSet.Columns.Contains("name"))))).MustHaveHappened();

            Assert.IsType<List<Entity>>(list);
            Assert.Equal(collection.Entities.Count, list.Count);
        }

        [Fact]
        public void RetrieveByAttribute_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.RetrieveByAttribute(EntityName, FieldName, "val", new ColumnSet("name")));
        }

        [Fact]
        public void RetrieveByAttributes_Forms_Query_And_Returns_List()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var collection = new EntityCollection();
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Returns(collection);
            IRepository repo = new Repository(fakeOrgService);
            var list = repo.RetrieveByAttributes(EntityName, new[] {"field1", "field2"}, new object[] {"val1", "val2"});
            A.CallTo(() => fakeOrgService.RetrieveMultiple((A<QueryByAttribute>.That.Matches(
                x => x.EntityName == EntityName
                     && x.Attributes.Count == 2
                     && x.Attributes.Contains("field1")
                     && x.Attributes.Contains("field2")
                     && x.Values.Count == 2
                     && x.Values.Contains("val1")
                     && x.Values.Contains("val2")
                     && x.ColumnSet.AllColumns)))).MustHaveHappened();

            Assert.IsType<List<Entity>>(list);
            Assert.Equal(collection.Entities.Count, list.Count);
        }

        [Fact]
        public void RetrieveByAttributes_Takes_ColumnSet()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            var collection = new EntityCollection();
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            collection.Entities.Add(new Entity(EntityName));
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Returns(collection);
            IRepository repo = new Repository(fakeOrgService);
            var list = repo.RetrieveByAttributes(EntityName, new[] { "field1", "field2" }, new object[] { "val1", "val2" }, new ColumnSet("name"));
            A.CallTo(() => fakeOrgService.RetrieveMultiple((A<QueryByAttribute>.That.Matches(
                x => x.EntityName == EntityName
                     && x.Attributes.Count == 2
                     && x.Attributes.Contains("field1")
                     && x.Attributes.Contains("field2")
                     && x.Values.Count == 2
                     && x.Values.Contains("val1")
                     && x.Values.Contains("val2")
                     && x.ColumnSet.Columns.Count == 1
                     && x.ColumnSet.Columns.Contains("name"))))).MustHaveHappened();

            Assert.IsType<List<Entity>>(list);
            Assert.Equal(collection.Entities.Count, list.Count);
        }

        [Fact]
        public void RetrieveByAttributes_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.RetrieveMultiple(A<QueryBase>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.RetrieveByAttributes(EntityName, new[] { "field1", "field2" }, new object[] { "val1", "val2" }, new ColumnSet("name")));
        }

        [Fact]
        public void ChangeStatus_Executes_SetStateRequest()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Execute(A<SetStateRequest>._)).Returns(new OrganizationResponse());
            IRepository repo = new Repository(fakeOrgService);
            var entity = new Entity(EntityName) {Id = Guid.NewGuid()};
            repo.ChangeStatus(entity, 1,2);
            A.CallTo(() => fakeOrgService.Execute(A<SetStateRequest>.That.Matches(x =>
                x.EntityMoniker.LogicalName == entity.LogicalName
                && x.EntityMoniker.Id == entity.Id
                && x.State.Value == 1
                && x.Status.Value == 2))).MustHaveHappened();
        }

        [Fact]
        public void ChangeStatus_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Execute(A<SetStateRequest>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.ChangeStatus(new Entity(),1,2));
        }

        [Fact]
        public void AssignRecord_Executes_AssignRequest()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Execute(A<AssignRequest>._)).Returns(new AssignResponse());
            IRepository repo = new Repository(fakeOrgService);
            var entity = new EntityReference(EntityName) { Id = Guid.NewGuid() };
            var assignee = new EntityReference("contact", Guid.NewGuid());
            repo.AssignRecord(entity, assignee);
            A.CallTo(() => fakeOrgService.Execute(A<AssignRequest>.That.Matches(x =>
                x.Target.LogicalName == entity.LogicalName
                && x.Target.Id == entity.Id
                && x.Assignee.LogicalName == assignee.LogicalName
                && x.Assignee.Id == assignee.Id))).MustHaveHappened();
        }

        [Fact]
        public void AssignRecord_Throws_Exception()
        {
            var fakeOrgService = A.Fake<IOrganizationService>();
            A.CallTo(() => fakeOrgService.Execute(A<AssignRequest>._)).Throws<InvalidPluginExecutionException>();
            IRepository repo = new Repository(fakeOrgService);
            Assert.Throws<InvalidPluginExecutionException>(() => repo.AssignRecord(new EntityReference(), new EntityReference()));
        }
    }
}
