using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using MGXRM.Common.Framework.Interfaces;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
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
        
        public List<Entity> FetchAll(string fetchXml)
        {
            var entities = new List<Entity>();
            const int fetchCount = 5000;
            var pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                var xml = InsertPageAndCookieIntoFetchXml(fetchXml, pagingCookie, pageNumber, fetchCount);

                var fetchRequest1 = new RetrieveMultipleRequest
                {
                    Query = new FetchExpression(xml)
                };

                var returnCollection =
                    ((RetrieveMultipleResponse)Service.Execute(fetchRequest1)).EntityCollection;
                if (returnCollection.Entities.Count > 0)
                    entities.AddRange(returnCollection.Entities);

                if (!returnCollection.MoreRecords) break;

                pageNumber++;
                pagingCookie = returnCollection.PagingCookie;
            }

            return entities;
        }
        
        private string InsertPageAndCookieIntoFetchXml(string xml, string cookie, int page, int count)
        {
            var stringReader = new StringReader(xml);
            var reader = new XmlTextReader(stringReader);
            var doc = new XmlDocument();
            doc.Load(reader);

            return InsertPageAndCookieIntoFetchXml(doc, cookie, page, count);
        }

        private string InsertPageAndCookieIntoFetchXml(XmlDocument doc, string cookie, int page, int count)
        {
            var attrs = doc.DocumentElement.Attributes;
            if (cookie != null)
            {
                var pagingAttr = doc.CreateAttribute("paging-cookie");
                pagingAttr.Value = cookie;
                attrs.Append(pagingAttr);
            }
            var pageAttr = doc.CreateAttribute("page");
            pageAttr.Value = Convert.ToString(page);
            attrs.Append(pageAttr);

            var countAttr = doc.CreateAttribute("count");
            countAttr.Value = Convert.ToString(count);
            attrs.Append(countAttr);

            var sb = new StringBuilder(1024);
            var stringWriter = new StringWriter(sb);

            var writer = new XmlTextWriter(stringWriter);
            doc.WriteTo(writer);
            writer.Close();

            return sb.ToString();
        }
    }
}
