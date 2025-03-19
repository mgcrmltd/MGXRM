using System;
using System.Collections.Generic;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Common.Framework.Repositories
{
    public class RepositoryDecorator : IRepository
    {
        readonly IRepository _repository;
        public IOrganizationService Service => _repository.Service;

        public RepositoryDecorator(IRepository repository)
        {
            _repository = repository;
        }

        public Guid Create(Entity entity)
        {
            return _repository.Create(entity);
        }

        public void Delete(Entity entity)
        {
            _repository.Delete(entity);
        }

        public void Delete(EntityReference entityRef)
        {
            _repository.Delete(entityRef);
        }

        public void Update(Entity target)
        {
            _repository.Update(target);
        }

        public Entity Retrieve(string entity, Guid id, ColumnSet columnSet)
        {
            return _repository.Retrieve(entity, id, columnSet);
        }

        public List<Entity> RetrieveMultiple(QueryBase query)
        {
            return _repository.RetrieveMultiple(query);
        }

        public List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue)
        {
            return _repository.RetrieveByAttribute(entityName, attribute, attributeValue);
        }

        public List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue, ColumnSet columns)
        {
            return _repository.RetrieveByAttribute(entityName, attribute, attributeValue, columns);
        }

        public List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues)
        {
            return _repository.RetrieveByAttributes(entityName, attributes, attributeValues);
        }

        public List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues, ColumnSet columns)
        {
            return _repository.RetrieveByAttributes(entityName, attributes, attributeValues, columns);
        }

        public void ChangeStatus(Entity target, int statecode, int statuscode)
        {
            _repository.ChangeStatus(target, statecode, statuscode);
        }

        public void AssignRecord(EntityReference entity, EntityReference assignee)
        {
            _repository.AssignRecord(entity, assignee);
        }

        public List<Entity> FetchAll(string fetchXml)
        {
            return _repository.FetchAll(fetchXml);
        }
    }
}