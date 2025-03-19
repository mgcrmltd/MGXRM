using System;
using System.Collections.Generic;
using MGXRM.Common.Framework.Interfaces;
using MGXRM.Common.Framework.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using FakeItEasy;
using Xunit;

namespace MGXRM.Common.Tests.Framework.Repositories
{
    public class RepositoryDecoratorTests
    {
        private readonly IRepository _fakeRepository;
        private readonly RepositoryDecorator _decorator;

        public RepositoryDecoratorTests()
        {
            _fakeRepository = A.Fake<IRepository>();
            _decorator = new RepositoryDecorator(_fakeRepository);
        }

        [Fact]
        public void Create_ShouldCallRepositoryCreate()
        {
            var entity = new Entity("account");
            var expectedId = Guid.NewGuid();
            A.CallTo(() => _fakeRepository.Create(entity)).Returns(expectedId);

            var result = _decorator.Create(entity);

            Assert.Equal(expectedId, result);
            A.CallTo(() => _fakeRepository.Create(entity)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Delete_ShouldCallRepositoryDelete_WithEntity()
        {
            var entity = new Entity("account");

            _decorator.Delete(entity);

            A.CallTo(() => _fakeRepository.Delete(entity)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Delete_ShouldCallRepositoryDelete_WithEntityReference()
        {
            var entityRef = new EntityReference("account", Guid.NewGuid());

            _decorator.Delete(entityRef);

            A.CallTo(() => _fakeRepository.Delete(entityRef)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Update_ShouldCallRepositoryUpdate()
        {
            var entity = new Entity("account");

            _decorator.Update(entity);

            A.CallTo(() => _fakeRepository.Update(entity)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void Retrieve_ShouldCallRepositoryRetrieve()
        {
            var id = Guid.NewGuid();
            var columnSet = new ColumnSet("name");
            var entity = new Entity("account") { Id = id };
            A.CallTo(() => _fakeRepository.Retrieve("account", id, columnSet)).Returns(entity);

            var result = _decorator.Retrieve("account", id, columnSet);

            Assert.Equal(entity, result);
            A.CallTo(() => _fakeRepository.Retrieve("account", id, columnSet)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void RetrieveMultiple_ShouldCallRepositoryRetrieveMultiple()
        {
            var query = new QueryExpression("account");
            var entities = new List<Entity> { new Entity("account") };
            A.CallTo(() => _fakeRepository.RetrieveMultiple(query)).Returns(entities);

            var result = _decorator.RetrieveMultiple(query);

            Assert.Equal(entities, result);
            A.CallTo(() => _fakeRepository.RetrieveMultiple(query)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void RetrieveByAttribute_ShouldCallRepositoryRetrieveByAttribute()
        {
            var entities = new List<Entity> { new Entity("account") };
            A.CallTo(() => _fakeRepository.RetrieveByAttribute("account", "name", "Test"))
                .Returns(entities);

            var result = _decorator.RetrieveByAttribute("account", "name", "Test");

            Assert.Equal(entities, result);
            A.CallTo(() => _fakeRepository.RetrieveByAttribute("account", "name", "Test"))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void ChangeStatus_ShouldCallRepositoryChangeStatus()
        {
            var entity = new Entity("account");

            _decorator.ChangeStatus(entity, 1, 2);

            A.CallTo(() => _fakeRepository.ChangeStatus(entity, 1, 2)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void AssignRecord_ShouldCallRepositoryAssignRecord()
        {
            var entityRef = new EntityReference("account", Guid.NewGuid());
            var assignee = new EntityReference("systemuser", Guid.NewGuid());

            _decorator.AssignRecord(entityRef, assignee);

            A.CallTo(() => _fakeRepository.AssignRecord(entityRef, assignee)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void FetchAll_ShouldCallRepositoryFetchAll()
        {
            var fetchXml = "<fetch></fetch>";
            var entities = new List<Entity> { new Entity("account") };
            A.CallTo(() => _fakeRepository.FetchAll(fetchXml)).Returns(entities);

            var result = _decorator.FetchAll(fetchXml);

            Assert.Equal(entities, result);
            A.CallTo(() => _fakeRepository.FetchAll(fetchXml)).MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}