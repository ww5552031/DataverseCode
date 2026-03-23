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
    public async Task Build_FetchXml_With_LinkEntity_All_Filter_Should_Be_Correct()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "fullname" };
    
        var linkFilter = new FilterBuilder("and")
                            .Condition("name", FetchOperator.Equal, "Contoso");
    
        var link = new LinkEntityBuilder("account", "primarycontactid", "contactid", "account", LinkType.All)
                        .Filter(linkFilter);
    
        var filter = new FilterBuilder("and")
                        .LinkEntity(link);
    
        var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .Filter(filter)
                            .Build();
    
        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
    }

    [TestMethod]
    public async Task Build_FetchXml_NotAny_LinkEntity_In_Filter_Should_Be_Correct()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "fullname" };
    
        var linkedEntityName = "account";
        var link = new LinkEntityBuilder(linkedEntityName, "primarycontactid", "contactid", "acct", LinkType.NotAny)
                        .Filter(new FilterBuilder("and")
                            .Condition("name", FetchOperator.NotNull));
    
        var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .Filter(new FilterBuilder("and")
                                .LinkEntity(link))
                            .Build();
    
        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
    }

    [TestMethod]
    public async Task Build_FetchXml_LinkEntity_In_Filter_Should_Be_Correct()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "fullname" };
    
        var accountLinkFilter = new FilterBuilder("and")
            .Condition("name", FetchOperator.NotNull);
    
        var accountLink = new LinkEntityBuilder("account", "primarycontactid", "contactid", null, LinkType.Any)
            .Filter(accountLinkFilter);
    
        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)
                .Select(attributes)
                .Filter(new FilterBuilder("or").LinkEntity(accountLink)
                .Condition("statecode", FetchOperator.Equal, "1"))
            .Build();
    
        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 10);
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
    public async Task Build_FetchXml_With_ValueOf_And_LinkEntity_Should_Be_Correct()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "contactid", "fullname" };

        var filter = new FilterBuilder("and")
                        .ConditionValueOf("fullname", FetchOperator.NotEqual, "acct.name");

        var link = new LinkEntityBuilder("account", "accountid", "parentcustomerid", "acct", LinkType.Outer)
                        .Select("name");

        var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .Filter(filter)
                            .LinkEntity(link)
                            .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 13);
        entityCollection.ForEach(entity =>
       {
           var contactId = entity.GetAttributeValue<Guid>("contactid");
           var fullname = entity.GetAttributeValue<string>("fullname");
           var accountName = entity.GetAttributeValue<AliasedValue>("acct.name")?.Value;

           Console.WriteLine($"ContactId: {contactId}, Fullname: {fullname}, Account Name: {accountName}");
       });
    }

    [TestMethod]
    public async Task Build_With_ConditionValueOf_Should_Contain_ValueOf()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "firstname" };

        var filter = new FilterBuilder()
                        .ConditionValueOf("firstname", FetchOperator.NotEqual, "lastname");

        var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .Filter(filter)
                            .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 17);
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
    public async Task AccountRepository_OrConditionTest()
    {
        var entityLogicName = "account";
        var attributes = new string[] { "name", "address1_city" };

        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)
            .Select(attributes)
            .Filter(new FilterBuilder("or")
                .Condition("address1_city", FetchOperator.Equal, "Redmond")
                .Condition("address1_city", FetchOperator.Equal, "Seattle")
                .Condition("address1_city", FetchOperator.Equal, "Bellevue"))
            .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 0);
    }

    [TestMethod]
    public async Task Build_LinkEntity_WithNullCondition()
    {
        var entityLogicName = "contact";
        var attributes = new string[] { "fullname" };
        var linkedEntityName = "account";

        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)
                .Select(attributes)
                .Filter(new FilterBuilder("and")
                .Condition("a", "fax", FetchOperator.Null))
                .LinkEntity(new LinkEntityBuilder(linkedEntityName, "accountid", "parentcustomerid", "a", LinkType.Outer))
            .Build();

        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count > 0);
    }



    [TestMethod]
    public async Task AccountRepository_AndConditionInTest()
    {
        var entityLogicName = "account";
        var attributes = new string[] { "name", "address1_city" };
        var cities = new string[] { "Redmond", "Seattle", "Bellevue" };

        var fetchXml = new FetchXmlBuilder()
                            .Entity(entityLogicName)
                            .Select(attributes)
                            .Filter(new FilterBuilder("and")
                                .Condition("address1_city", FetchOperator.In, cities))
                            .Build();
                            
        var fetchExpression = new FetchExpression(fetchXml);
        var entityCollection = await service!.RetrieveMultipleAsync(fetchExpression);                   
        Assert.IsNotNull(entityCollection);
        Assert.IsTrue(entityCollection.Entities.Count == 0);
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
