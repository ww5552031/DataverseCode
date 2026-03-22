using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using train.dataverse.comm;
using train.dataverse.comm.FetchDal;

namespace train.testproj;

[TestClass]
public sealed class FetchXmlBuilderTest
{
   private  ServiceClient? service;

    [TestInitialize]
    public void Initialize()
    {
        var builder = new ConfigurationBuilder()
       .SetBasePath(Directory.GetCurrentDirectory())
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        var dataverseConnection = builder.Build().
                                    GetConnectionString("DataverseConnection");
        service = new ServiceClient(dataverseConnection);
    }

    [TestMethod]
    public async Task WhoIAmRequestTest()
    {
        if (service == null){
            Assert.Fail("Service client is not initialized.");
        }
        var result = service.ExecuteAsync(new Microsoft.Crm.Sdk.Messages.WhoAmIRequest());
    }

    [TestMethod]
    public async Task AccountRepository_BasicTest()
    {
        if (service == null){
            Assert.Fail("Service client is not initialized.");
        }
        var repository = new AccountRepository(service);
        var accounts = await repository.GetAccountByStateCode(0);
        Assert.IsNotNull(accounts);
        Assert.IsTrue(accounts.Count <= 10);
    }

    [TestMethod]
    public async Task AccountRepository_PageByPageTest()
    {
        if (service == null){
            Assert.Fail("Service client is not initialized.");
        }
        string pagingCookie = string.Empty;
        var repository = new AccountRepository(service);
        var accounts = await repository.GetAccountPageByPage(1, 2, pagingCookie);
        Assert.IsNotNull(accounts);
        Assert.IsTrue(accounts.entities.Count <= 3);
        Assert.IsNotNull(accounts.pagingCookie);
    }

    [TestMethod]
    public async Task AccountRepository_LinkContactTest()
    {
        if (service == null){
            Assert.Fail("Service client is not initialized.");
        }
        var repository = new AccountRepository(service);
        var accounts = await repository.GetAccountLinkContact();
        if (accounts.Count > 0)
        {
            var firstAccount = accounts[0];
            
            Assert.IsNotNull(firstAccount.GetAttributeValue<AliasedValue>("contact.fullname"));
            Console.WriteLine($"First account's linked contact fullname: {firstAccount.GetAttributeValue<AliasedValue>("contact.fullname").Value}");
        }
    
        Assert.IsNotNull(accounts);
        Assert.IsTrue(accounts.Count <= 5);
    }

    [TestMethod]
    public async Task AccountRepository_contactIsNullTest()
    {
        var entityLogicName = "account";
        var attributes = new string[] { "name" };
        var linkedEntityName = "contact";

        var fetchXml = new FetchXmlBuilder()
                        .Entity(entityLogicName)
                        .Select(attributes)
                        .OrderBy("name", false)
                        .LinkEntity(new LinkEntityBuilder(linkedEntityName, "parentcustomerid", "accountid", "contact", LinkType.Outer))
                        .Filter(new FilterBuilder("and")
                                .Condition("contact", "parentcustomerid", FetchOperator.Null))
                        .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 0);
    }

    [TestMethod]
    public async Task AccountRepository_AggregateTest()
    {
        var entityLogicName = "account";
        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)// 开启聚合
                .SelectAggregate("numberofemployees", "Average","avg")
                .SelectAggregate("numberofemployees", "Count", "count")
                .SelectAggregate("numberofemployees", "ColumnCount", "countcolumn")
                .SelectAggregate("numberofemployees", "Maximum", "max")
                .SelectAggregate("numberofemployees", "Minimum", "min")
                .SelectAggregate("numberofemployees", "Sum", "sum")
            .Build();
        
        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        entityCollection.ForEach(entity =>
        {
            var avg = entity.GetAttributeValue<AliasedValue>("Average")?.Value;
            var count = entity.GetAttributeValue<AliasedValue>("Count")?.Value;
            var countColumn = entity.GetAttributeValue<AliasedValue>("ColumnCount")?.Value;
            var max = entity.GetAttributeValue<AliasedValue>("Maximum")?.Value;
            var min = entity.GetAttributeValue<AliasedValue>("Minimum")?.Value;
            var sum = entity.GetAttributeValue<AliasedValue>("Sum")?.Value;

            Console.WriteLine($"Average: {avg}, Count: {count}, CountColumn: {countColumn}, Max: {max}, Min: {min}, Sum: {sum}");
        });
    }

    [TestMethod]
    public async Task AccountRepository_GroupByTest()
    {
        var entityLogicName = "account";
        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)
                .SelectAggregate("numberofemployees", "Total", "sum")
                .SelectGroupBy("createdon", "Day", "day")
                .SelectGroupBy("createdon", "Week", "week")
                .SelectGroupBy("createdon", "Month", "month")
                .SelectGroupBy("createdon", "Year", "year")
                .SelectGroupBy("createdon", "FiscalPeriod", "fiscal-period")
                .SelectGroupBy("createdon", "FiscalYear", "fiscal-year")
                .OrderByAlias("Month")
            .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        entityCollection.ForEach(entity =>
        {
            var total = entity.GetAttributeValue<AliasedValue>("Total")?.Value;
            var day = entity.GetAttributeValue<AliasedValue>("Day")?.Value;
            var week = entity.GetAttributeValue<AliasedValue>("Week")?.Value;
            var month = entity.GetAttributeValue<AliasedValue>("Month")?.Value;
            var year = entity.GetAttributeValue<AliasedValue>("Year")?.Value;
            var fiscalPeriod = entity.GetAttributeValue<AliasedValue>("FiscalPeriod")?.Value;
            var fiscalYear = entity.GetAttributeValue<AliasedValue>("FiscalYear")?.Value;

            Console.WriteLine($"Total: {total}, Day: {day}, Week: {week}, Month: {month}, Year: {year}, FiscalPeriod: {fiscalPeriod}, FiscalYear: {fiscalYear}");
        });
    }


}
