using System;
using System.Collections.Generic;
using System.Linq;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Common.Framework.Repositories
{
    public class Repository : IRepository
    {
        #region Members and constructors
        public IOrganizationService Service { get; }

        public Repository(IOrganizationService service)
        {
            Service = service;
        }
        #endregion

        public Guid Create(Entity entity)
        {
            return Service.Create(entity);
        }

        public void Delete(Entity entity)
        {
            Service.Delete(entity.LogicalName, entity.Id);
        }

        public void Delete(EntityReference entityRef)
        {
            Service.Delete(entityRef.LogicalName, entityRef.Id);
        }

        public void Update(Entity target)
        {
            Service.Update(target);
        }

        public Entity Retrieve(string entity, Guid id, ColumnSet columnSet)
        {
            return Service.Retrieve(entity, id, columnSet);
        }

        public List<Entity> RetrieveMultiple(QueryBase query)
        {
            return Service.RetrieveMultiple(query).Entities.ToList();
        }
        
        public List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue)
        {
            return RetrieveByAttribute(entityName,attribute, attributeValue, new ColumnSet(true));
        }

        public List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue, ColumnSet columns)
        {
            var query = new QueryByAttribute(entityName);
            query.Attributes.Add(attribute);
            query.Values.Add(attributeValue);
            query.ColumnSet = columns;
            return RetrieveMultiple(query);
        }

        public List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues)
        {
            return RetrieveByAttributes(entityName, attributes, attributeValues, new ColumnSet(true));
        }

        public List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues, ColumnSet columns)
        {
            var query = new QueryByAttribute(entityName);
            query.Attributes.AddRange(attributes);
            query.Values.AddRange(attributeValues);
            query.ColumnSet = columns;
            return RetrieveMultiple(query);
        }

        public void ChangeStatus(Entity target, int statecode, int statuscode)
        {
            var setState = new SetStateRequest()
            {
                EntityMoniker = target.ToEntityReference(),
                State = new OptionSetValue(statecode),
                Status = new OptionSetValue(statuscode)

            };
            Service.Execute(setState);
        }

        public void AssignRecord(EntityReference entity, EntityReference assignee)
        {
            var assign = new AssignRequest
            {
                Assignee = assignee,
                Target = entity
            };
            Service.Execute(assign);
        }
    }
}
