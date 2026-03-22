---
name: fetchxmlbuilder
description: 根据FetchXML查询字符串使用，使用FetchXmlBuilder构建查询。
agent: "ask"
model: GPT-5 mini (copilot)
argument-hint: "FetchXML=FetchXMLQuery"
---
## 任务描述
   1. 首先使用#tool:search找到 [FetchXmlBuilder](../../train.dataverse.comm/FetchXmlBuilder.cs)。
   2. 根据用户输入的FetchXML查询字符串，使用FetchXmlBuilder构建查询代码。
   3. 基于上一步使用FetchXmlBuilder构建查询代码,
  ### 注意事项
   - 如果如果FetchXmlBuilder 已经有枚举参数重载，优先使用枚举参数重载。
   - 如果FetchXmlBuilder类无法满足某些FetchXML查询的构建需求，只需要请告知用户。
   - 处理不同类型的FetchXML查询，包括基本查询、分页查询、链接实体查询和聚合查询。
   - 输出的代码应该是的能执行单元测试C#代码片段。单元测试框架只能是MSTest。
  
   

## 输入参数
- FetchXML查询字符串: ${input:FetchXML:请输入FetchXML查询字符串}

## 输出
- 构建的查询对象的完整代码


## 简单示例
### 输入:
```xml
<fetch top="10">
  <entity name="account">
    <filter type="and">
      <condition attribute="statecode" operator="eq" value="0" />
    </filter>
  </entity>
</fetch>
```

### 输出:

```csharp
     var entityLogicName = "account";
     var fetchXml = new FetchXmlBuilder()
                        .Entity(entityLogicName)
                        .Filter(new FilterBuilder("and")
                        .Condition("statecode", FetchOperator.Equal, stateCode.ToString()))
                        .Top(10).Build();
```

## 分页示例
### 输入:
```xml
<fetch count="3" page="1">
  <entity name="account">
    <attribute name="name" />
    <order attribute="name" descending="true" />
    <order attribute="accountid" descending="true" />
  </entity>
</fetch>
```
### 输出:
```csharp
    var entityLogicName = "account";
    var pageNumber = 1;
    var pageSize = 3;
    var pagingCookie = string.Empty;
    var attributes = new string[] { "name" };
    var fetchXml = new FetchXmlBuilder()
                      .Entity(entityLogicName)
                      .Select(attributes)
                      .OrderBy("name", true)
                      .OrderBy("accountid", true)
                      .Page(pageNumber,pageSize,pagingCookie)
                      .Build();
```

## LinkEntity示例
### 输入:
```xml
<fetch top='5'>
<entity name='account'>
<attribute name='name' />
<link-entity name='contact' from='contactid' to='primarycontactid' link-type='inner' alias='contact'>
<attribute name='fullname' />
</link-entity>
</entity>
</fetch>
```
### 输出:
```csharp
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
```

## 聚合示例
### 输入:
```xml
<fetch aggregate='true'>
<entity name='account'>
  <attribute name='numberofemployees' alias='Average' aggregate='avg' />
  <attribute name='numberofemployees' alias='Count' aggregate='count' />
  <attribute name='numberofemployees' alias='ColumnCount' aggregate='countcolumn' />
  <attribute name='numberofemployees' alias='Maximum' aggregate='max' />
  <attribute name='numberofemployees' alias='Minimum' aggregate='min' />
  <attribute name='numberofemployees' alias='Sum' aggregate='sum' />
</entity>
</fetch>
```

### 输出:
```csharp
  var entityLogicName = "account";
    var entityLogicName = "account";
        var fetchXml = new FetchXmlBuilder()
            .Entity(entityLogicName)
                .SelectAggregate("numberofemployees", "Average","avg")
                .SelectAggregate("numberofemployees", "Count", "count")
                .SelectAggregate("numberofemployees", "ColumnCount", "countcolumn")
                .SelectAggregate("numberofemployees", "Maximum", "max")
                .SelectAggregate("numberofemployees", "Minimum", "min")
                .SelectAggregate("numberofemployees", "Sum", "sum")
            .Build();
```

## 按日期部分分组示例
### 输入:
```xml  
<fetch aggregate='true'>
<entity name='account'>
<attribute name='numberofemployees' alias='Total' aggregate='sum' />
<attribute name='createdon' alias='Day' groupby='true' dategrouping='day' />
<attribute name='createdon' alias='Week' groupby='true' dategrouping='week' />
<attribute name='createdon' alias='Month' groupby='true' dategrouping='month' />
<attribute name='createdon' alias='Year' groupby='true' dategrouping='year' />
<attribute name='createdon' alias='FiscalPeriod' groupby='true' dategrouping='fiscal-period' />
<attribute name='createdon' alias='FiscalYear' groupby='true' dategrouping='fiscal-year' />
<order alias='Month' />
</entity>
</fetch>
```
### 输出:
```csharp
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
```

## FetchXMLBuilder Class
- FetchXmlBuilder是一个用于构建FetchXML查询的类。 
   [FetchXmlBuilder](../../train.dataverse.comm/FetchXmlBuilder.cs)
 
