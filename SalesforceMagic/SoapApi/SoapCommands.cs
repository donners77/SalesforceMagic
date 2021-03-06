﻿using System.Collections.Generic;
using System.Linq;
using SalesforceMagic.Configuration;
using SalesforceMagic.Entities;
using SalesforceMagic.Http;
using SalesforceMagic.ORM.BaseRequestTemplates;
using SalesforceMagic.SoapApi.Enum;
using SalesforceMagic.SoapApi.RequestTemplates;

namespace SalesforceMagic.SoapApi
{
    internal static class SoapCommands
    {
        internal static string Login(string username, string password)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                LoginTemplate = new LoginRequestTemplate(username, password)
            });
        }

        internal static string Query(string query, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryTemplate = new QueryTemplate(query)
            }, BuildXmlHeader(session));
        }

        public static string Search(string searchString, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                SearchTemplate = new SearchTemplate(searchString)
            }, BuildXmlHeader(session));
        }

        internal static string Retrieve<T>(string[] ids, SalesforceSession session) where T : SObject
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                RetrieveTemplate = new RetrieveTemplate
                {
                    Type = typeof(T),
                    Ids = ids
                }
            }, BuildXmlHeader(session));
        }

        internal static string Delete(string[] ids, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                DeleteTemplate = new DeleteTemplate
                {
                    Ids = ids
                }
            }, BuildXmlHeader(session));
        }

        public static string QueryMore(string queryLocator, SalesforceSession session)
        {
            return XmlRequestGenerator.GenerateRequest(new XmlBody
            {
                QueryMoreTemplate = new QueryMoreTemplate(queryLocator)
            }, BuildXmlHeader(session));
        }

        public static string CrudOperation<T>(CrudOperation<T> operation, SalesforceSession session) where T : SObject
        {
            XmlBody body = GetCrudBody(operation);
            return XmlRequestGenerator.GenerateRequest(body, BuildXmlHeader(session));
        }

        private static XmlBody GetCrudBody<T>(CrudOperation<T> operation) where T : SObject
        {
            XmlBody body = new XmlBody();

            switch (operation.OperationType)
            {
                case CrudOperations.Insert:
                    body.InsertTemplate = new BasicCrudTemplate
                    {
                        SObjects = GetCrudItems(operation.Items, CrudOperations.Insert)
                    };
                    break;
                case CrudOperations.Upsert:
                    body.UpsertTemplate = new UpsertTemplate
                    {
                        SObjects = GetCrudItems(operation.Items, CrudOperations.Upsert),
                        ExternalIdFieldName = operation.ExternalIdField
                    };
                    break;
                case CrudOperations.Update:
                    body.UpdateTemplate = new BasicCrudTemplate
                    {
                        SObjects = GetCrudItems(operation.Items, CrudOperations.Update)
                    };
                    break;
                case CrudOperations.Delete:
                    body.DeleteTemplate = new DeleteTemplate
                    {
                        Ids = operation.Items.Select(i => i.Id).Distinct().ToArray()
                    };
                    break;
            }

            return body;
        }

        private static IEnumerable<T> GetCrudItems<T>(IEnumerable<T> items, CrudOperations type) where T : SObject
        {
            // TODO: Need to find a better way to have the operationtype 
            //       available during xml serialization of the objects, this is not the best
            return items.Select(x =>
            {
                x.OperationType = type;
                return x;
            }).ToArray();
        }

        private static XmlHeader BuildXmlHeader(SalesforceSession session)
        {
            var header = new XmlHeader
            {
                SessionHeader = new SessionHeader
                {
                    SessionId = session.SessionId
                }
            };

            if (session.BatchSize.HasValue)
            {
                header.QueryOptions = new QueryOptionsHeader
                {
                    BatchSize = session.BatchSize.Value
                };
            }

            return header;
        }
    }
}
