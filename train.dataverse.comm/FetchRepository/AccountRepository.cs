using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace train.dataverse.comm.FetchDal
{
    public class AccountRepository
    {
        private const string entityLogicName = "account";
        private ServiceClient _serviceClient;

        public AccountRepository(ServiceClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        public async Task<List<Entity>> GetAccountByStateCode(int stateCode)
        {
            var fetchXml = new FetchXmlBuilder()
                                     .Entity(entityLogicName)
                                     .Filter(new FilterBuilder("and").Condition("statecode", "eq", stateCode.ToString()))
                                     .Top(10).Build();

            var fetchExpression = new FetchExpression(fetchXml);
            var entityCollection = await _serviceClient.RetrieveMultipleAsync(fetchExpression);
            if (entityCollection.IsEmpty())
                return new List<Entity>();
            else return entityCollection.Entities.ToList();
        }

        public async Task<(List<Entity> entities, string pagingCookie)> GetAccountPageByPage(int pageNumber,int pageSize, string pagingCookie)
        {

            var attributes = new string[] { "name" };
            var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                                .OrderBy("name", true)
                                .OrderBy("accountid", true)
                            .Page(pageNumber,pageSize,pagingCookie)
                            .Build();

            var fetchExpression = new FetchExpression(fetchXml);
            var entityCollection = await _serviceClient.RetrieveMultipleAsync(fetchExpression);
            if (entityCollection.IsEmpty())
                return (new List<Entity>(), pagingCookie);
            else
            {
                var pagingInfo = entityCollection.GetPagingInfo();
                if (pagingInfo.hasMore)
                {
                    pagingCookie = pagingInfo.pagingCookie;
                }
                else
                {
                    pagingCookie = string.Empty;
                }
                
                return (entityCollection.Entities.ToList(), pagingCookie);
            }
        }

        public async Task<List<Entity>> GetAccountLinkContact()
        {
            var attributes = new string[] { "name" };
            var linkedEntityName = "contact";
            var linkedAttributes = new string[] { "fullname" };
            var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .LinkEntity(new LinkEntityBuilder(linkedEntityName, "contactid", "primarycontactid", "contact", LinkType.Inner)
                                .Select(linkedAttributes))
                            .Top(5)
                            .Build();
            var fetchExpression = new FetchExpression(fetchXml);
            var entityCollection = await _serviceClient.RetrieveMultipleAsync(fetchExpression);
            if (entityCollection.IsEmpty())
                return new List<Entity>();
            else return entityCollection.Entities.ToList();
        }
    }
}