using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IRepository
    {
        IOrganizationService Service { get; }
        Entity Retrieve(string entity, Guid id, ColumnSet columnSet);
        Guid Create(Entity entity);
        void Delete(Entity entity);
        void Delete(EntityReference entityRef);
        void Update(Entity target);
        Guid Upsert(Entity entity, string[] customKeys);
        List<Entity> RetrieveMultiple(QueryBase query);
        IEnumerable<Entity> RetrieveAll(QueryExpression query);
        List<Entity> RetrieveByAttribute(string entity, string attribute, object attributeValue);
        List<Entity> RetrieveByAttribute(string entity, string attribute, object attributeValue, ColumnSet columns);
        List<Entity> RetrieveByAttributes(string entity, string[] attributes, object[] attributeValues);
        List<Entity> RetrieveByAttributes(string entity, string[] attributes, object[] attributeValues, ColumnSet columns);
        void ChangeStatus(Entity target, int statecode, int statuscode);
        void AssignRecord(EntityReference entity, EntityReference assignee);
        void AddItemToQueue(EntityReference item, EntityReference queueReference);
    }
}
