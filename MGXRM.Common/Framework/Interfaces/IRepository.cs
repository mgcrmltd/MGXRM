using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace MGXRM.Common.Framework.Interfaces
{
    public interface IRepository
    {
        IOrganizationService Service { get; }
        Guid Create(Entity entity);
        void Delete(Entity entity);
        void Delete(EntityReference entityRef);
        void Update(Entity target);
        Entity Retrieve(string entity, Guid id, ColumnSet columnSet);
        List<Entity> RetrieveMultiple(QueryBase query);
        List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue);
        List<Entity> RetrieveByAttribute(string entityName, string attribute, object attributeValue, ColumnSet columns);
        List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues);
        List<Entity> RetrieveByAttributes(string entityName, string[] attributes, object[] attributeValues, ColumnSet columns);
        void ChangeStatus(Entity target, int statecode, int statuscode);
        void AssignRecord(EntityReference entity, EntityReference assignee);
        
        List<Entity> FetchAll(string fetchXml);
    }
}
