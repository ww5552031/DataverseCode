using System.Text;

namespace train.dataverse.comm;

// <summary>
// 目的：构建 Dataverse/CRM 的 FetchXML 查询，支持链式调用、选择字段、过滤、联接、排序、聚合与分页。
// 主要类型：FetchOperator（条件枚举）、LinkType（连接类型）及其扩展方法用于映射为 FetchXML 字符串。
// 核心类：FetchXmlBuilder（顶层生成器），FilterBuilder（条件/子过滤），LinkEntityBuilder（联接实体），OrderByBuilder（排序）。
// 用法要点：链式调用设置实体/字段/过滤/链接/排序，最后 Build() 输出完整的 FetchXML 字符串。
// 注意：只有 paging-cookie 使用了 XML 转义，建议对所有外部输入做 XML 转义以防注入或格式错误。
// </summary>

public enum FetchOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    LessThan,
    GreaterEqual,
    LessEqual,
    Like,
    NotLike,
    In,
    NotIn,
    Between,
    NotBetween,
    Null,
    NotNull,
    Yesterday,
    Today,
    Tomorrow,
    LastWeek,
    ThisWeek,
    NextWeek,
    LastMonth,
    ThisMonth,
    NextMonth,
    LastYear,
    ThisYear,
    NextYear,
    LastXHours,
    NextXHours,
    LastXDays,
    NextXDays,
    LastXWeeks,
    NextXWeeks,
    LastXMonths,
    NextXMonths,
    LastXYears,
    NextXYears,
    EqualUserId,
    NotEqualUserId,
    EqualBusinessId,
    NotEqualBusinessId,
    BeginsWith,
    NotBeginWith
}

public enum LinkType
{
    Inner,
    Outer,
    Natural,
    Exists
}

public static class LinkTypeExtensions
{
    public static string ToLinkTypeString(this LinkType linkType)
    {
        switch (linkType)
        {
            case LinkType.Inner:
                return "inner";
            case LinkType.Outer:
                return "outer";
            case LinkType.Natural:
                return "natural";
            case LinkType.Exists:
                return "exists";
            default:
                return "inner";
        }
    }
}

public static class FetchOperatorExtensions
{
    public static string ToOperatorString(this FetchOperator op)
    {
        switch (op)
        {
            case FetchOperator.Equal: return "eq";
            case FetchOperator.NotEqual: return "ne";
            case FetchOperator.GreaterThan: return "gt";
            case FetchOperator.LessThan: return "lt";
            case FetchOperator.GreaterEqual: return "ge";
            case FetchOperator.LessEqual: return "le";
            case FetchOperator.Like: return "like";
            case FetchOperator.NotLike: return "not-like";
            case FetchOperator.In: return "in";
            case FetchOperator.NotIn: return "not-in";
            case FetchOperator.Between: return "between";
            case FetchOperator.NotBetween: return "not-between";
            case FetchOperator.Null: return "null";
            case FetchOperator.NotNull: return "not-null";
            case FetchOperator.Yesterday: return "yesterday";
            case FetchOperator.Today: return "today";
            case FetchOperator.Tomorrow: return "tomorrow";
            case FetchOperator.LastWeek: return "last-week";
            case FetchOperator.ThisWeek: return "this-week";
            case FetchOperator.NextWeek: return "next-week";
            case FetchOperator.LastMonth: return "last-month";
            case FetchOperator.ThisMonth: return "this-month";
            case FetchOperator.NextMonth: return "next-month";
            case FetchOperator.LastYear: return "last-year";
            case FetchOperator.ThisYear: return "this-year";
            case FetchOperator.NextYear: return "next-year";
            case FetchOperator.LastXHours: return "last-x-hours";
            case FetchOperator.NextXHours: return "next-x-hours";
            case FetchOperator.LastXDays: return "last-x-days";
            case FetchOperator.NextXDays: return "next-x-days";
            case FetchOperator.LastXWeeks: return "last-x-weeks";
            case FetchOperator.NextXWeeks: return "next-x-weeks";
            case FetchOperator.LastXMonths: return "last-x-months";
            case FetchOperator.NextXMonths: return "next-x-months";
            case FetchOperator.LastXYears: return "last-x-years";
            case FetchOperator.NextXYears: return "next-x-years";
            case FetchOperator.EqualUserId: return "eq-userid";
            case FetchOperator.NotEqualUserId: return "ne-userid";
            case FetchOperator.EqualBusinessId: return "eq-businessid";
            case FetchOperator.NotEqualBusinessId: return "ne-businessid";
            case FetchOperator.BeginsWith: return "begins-with";
            case FetchOperator.NotBeginWith: return "not-begin-with";
            default: return "eq";
        }
    }
}

public class FetchXmlBuilder
{
    private string _entityName=string.Empty;
    private List<string> _attributes = new List<string>();
    private List<FilterBuilder> _filters = new List<FilterBuilder>();
    private List<LinkEntityBuilder> _linkEntities = new List<LinkEntityBuilder>();
    private List<OrderByBuilder> _orderBys = new List<OrderByBuilder>();
    private int? _top = null;
    private int? _page = null;
    private string _pagingCookie = null!;
    private int? _count = null;
    private bool _aggregate = false;

    private bool _returnTotalRecordCount = false;

    public FetchXmlBuilder ReturnTotalRecordCount(bool value = true)
    {
        _returnTotalRecordCount = value;
        return this;
    }

    public FetchXmlBuilder Entity(string entityName)
    {
        _entityName = entityName;
        return this;
    }

    public FetchXmlBuilder Select(params string[] attributes)
    {
        _attributes.AddRange(attributes);
        return this;
    }

    public FetchXmlBuilder SelectAggregate(string attribute, string alias, string aggregateType)
    {
        _attributes.Add($"<attribute name='{attribute}' alias='{alias}' aggregate='{aggregateType}' />");
        _aggregate = true;
        return this;
    }

    public FetchXmlBuilder SelectAggregate(string attribute, string alias, string aggregateType, bool distinct = false)
    {
        var distinctAttr = distinct ? " distinct='true'" : "";
        _attributes.Add($"<attribute name='{attribute}' alias='{alias}' aggregate='{aggregateType}'{distinctAttr} />");
        _aggregate = true;
        return this;
    }
    

    /// <summary>
    /// 删除了两参数版本，保留带 dategrouping 的版本以避免重载歧义
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="alias">别名</param>
    /// <returns></returns>
    public FetchXmlBuilder SelectGroupBy(string attribute, string alias = null)
    {
        var aliasAttr = string.IsNullOrEmpty(alias) ? "" : $" alias='{alias}'";
        _attributes.Add($"<attribute name='{attribute}'{aliasAttr} groupby='true' />");
        _aggregate = true;
        return this;
    }


    /// <summary>
    /// 新增：支持 groupby + dategrouping 的 SelectGroupBy
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="alias">别名</param>
    /// <param name="dateGrouping">日期分组</param>
    /// <returns></returns>
    public FetchXmlBuilder SelectGroupBy(string attribute, string alias = null, string dateGrouping = null)
    {
        var aliasAttr = string.IsNullOrEmpty(alias) ? "" : $" alias='{alias}'";
        var dateGroupingAttr = string.IsNullOrEmpty(dateGrouping) ? "" : $" dategrouping='{dateGrouping}'";
        _attributes.Add($"<attribute name='{attribute}'{aliasAttr} groupby='true'{dateGroupingAttr} />");
        _aggregate = true;
        return this;
    }

    public FetchXmlBuilder Filter(FilterBuilder filter)
    {
        _filters.Add(filter);
        return this;
    }

    public FetchXmlBuilder Top(int top)
    {
        _top = top;
        return this;
    }

    public FetchXmlBuilder Page(int page, int? count = null, string pagingCookie = null)
    {
        _page = page;
        _count = count;
        _pagingCookie = pagingCookie;
        return this;
    }

    public FetchXmlBuilder LinkEntity(LinkEntityBuilder link)
    {
        _linkEntities.Add(link);
        return this;
    }

    public FetchXmlBuilder OrderBy(string attribute, bool descending = false)
    {
        _orderBys.Add(new OrderByBuilder(attribute, descending));
        return this;
    }

    // FetchXmlBuilder 和 LinkEntityBuilder 中添加重载方法
    public FetchXmlBuilder OrderByAlias(string alias, bool descending = false)
    {
        _orderBys.Add(new OrderByBuilder(null, alias, descending));
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();
        sb.Append("<fetch");
        if (_returnTotalRecordCount) sb.Append(" returntotalrecordcount='true'");
        if (_top.HasValue) sb.Append($" top='{_top.Value}'");
        if (_page.HasValue) sb.Append($" page='{_page.Value}'");
        if (!string.IsNullOrEmpty(_pagingCookie)) sb.Append($" paging-cookie='{System.Security.SecurityElement.Escape(_pagingCookie)}'");
        if (_count.HasValue) sb.Append($" count='{_count.Value}'");
        if (_aggregate) sb.Append(" aggregate='true'");
        sb.Append(">");

        sb.Append($"<entity name='{_entityName}'>");

        foreach (var attr in _attributes)
        {
            if (attr.StartsWith("<attribute")) // 兼容聚合
                sb.Append(attr);
            else
                sb.Append($"<attribute name='{attr}' />");
        }

        foreach (var filter in _filters)
            sb.Append(filter.Build());

        foreach (var link in _linkEntities)
            sb.Append(link.Build());

        foreach (var order in _orderBys)
            sb.Append(order.Build());

        sb.Append("</entity>");
        sb.Append("</fetch>");
        return sb.ToString();
    }
}

public class FilterBuilder
{
    private string _type; // "and" or "or"
    private List<string> _conditions = new List<string>();
    private List<FilterBuilder> _subFilters = new List<FilterBuilder>();

    public FilterBuilder(string type = "and")
    {
        _type = type;
    }

    // 支持带 entityname 的 In/NotIn（多值）条件：IEnumerable<string> 重载
    public FilterBuilder Condition(string entityName, string attribute, FetchOperator op, IEnumerable<string> values)
    {
        return Condition(entityName, attribute, op, values?.ToArray() ?? Array.Empty<string>());
    }

    // 支持带 entityname 的 In/NotIn（多值）条件：可变参数重载
    public FilterBuilder Condition(string entityName, string attribute, FetchOperator op, params string[] values)
    {
        if (op != FetchOperator.In && op != FetchOperator.NotIn)
        {
            // 非 In/NotIn 操作符，走单值重载
            return Condition(entityName, attribute, op, values.FirstOrDefault() ?? string.Empty);
        }

        var sb = new StringBuilder();
        sb.Append($"<condition entityname='{entityName}' attribute='{attribute}' operator='{op.ToOperatorString()}'>");
        foreach (var value in values)
        {
            sb.Append($"<value>{value}</value>");
        }
        sb.Append("</condition>");
        _conditions.Add(sb.ToString());
        return this;
    }

    // 添加支持数组值的 Condition 方法
    public FilterBuilder Condition(string attribute, FetchOperator op, params string[] values)
    {
        if (op != FetchOperator.In && op != FetchOperator.NotIn)
        {
            // 如果不是 In/NotIn 操作符，则使用第一个值
            return Condition(attribute, op, values.FirstOrDefault() ?? string.Empty);
        }

        var sb = new StringBuilder();
        sb.Append($"<condition attribute='{attribute}' operator='{op.ToOperatorString()}'>");
        foreach (var value in values)
        {
            sb.Append($"<value>{value}</value>");
        }
        sb.Append("</condition>");
        _conditions.Add(sb.ToString());
        return this;
    }

    // 添加支持 IEnumerable<string> 的重载
    public FilterBuilder Condition(string attribute, FetchOperator op, IEnumerable<string> values)
    {
        return Condition(attribute, op, values.ToArray());
    }


    public FilterBuilder Condition(string attribute, string op, string value)
    {
        _conditions.Add($"<condition attribute='{attribute}' operator='{op}' value='{value}' />");
        return this;
    }

    // 添加使用枚举的新重载方法
    public FilterBuilder Condition(string attribute, FetchOperator op, string value)
    {
        _conditions.Add($"<condition attribute='{attribute}' operator='{op.ToOperatorString()}' value='{value}' />");
        return this;
    }

    public FilterBuilder Condition(string entityName, string attribute, FetchOperator op, string value)
    {
        _conditions.Add($"<condition entityname='{entityName}' attribute='{attribute}' operator='{op.ToOperatorString()}' value='{value}' />");
        return this;
    }

    public FilterBuilder SubFilter(FilterBuilder filter)
    {
        _subFilters.Add(filter);
        return this;
    }

    public string Build()
    {
        var sb = new StringBuilder();
        sb.Append($"<filter type='{_type}'>");
        foreach (var cond in _conditions)
            sb.Append(cond);
        foreach (var sub in _subFilters)
            sb.Append(sub.Build());
        sb.Append("</filter>");
        return sb.ToString();
    }
}

public class LinkEntityBuilder
{
    private string _name;
    private string _from;
    private string _to;
    private string _alias;
    private LinkType _linkType = LinkType.Inner;
    private List<string> _attributes = new List<string>();
    private List<FilterBuilder> _filters = new List<FilterBuilder>();
    private List<LinkEntityBuilder> _linkEntities = new List<LinkEntityBuilder>();
    private List<OrderByBuilder> _orderBys = new List<OrderByBuilder>();
    private bool _aggregate = false;

    public LinkEntityBuilder(string name, string from, string to, string alias = null, LinkType linkType = LinkType.Inner)
    {
        _name = name;
        _from = from;
        _to = to;
        _alias = alias;
        _linkType = linkType;
    }

    public LinkEntityBuilder Select(params string[] attributes)
    {
        _attributes.AddRange(attributes);
        return this;
    }

    public LinkEntityBuilder SelectAggregate(string attribute, string alias, string aggregateType)
    {
        _attributes.Add($"<attribute name='{attribute}' alias='{alias}' aggregate='{aggregateType}' />");
        _aggregate = true;
        return this;
    }

     /// <summary>
     /// 新增：支持 groupby + dategrouping 的 SelectGroupBy
     /// </summary>
     /// <param name="attribute">属性名称</param>
     /// <param name="alias">别名</param>
     /// <param name="dateGrouping">日期分组</param>
     /// <returns></returns>
    public LinkEntityBuilder SelectGroupBy(string attribute, string alias = null, string dateGrouping = null)
    {
        var aliasAttr = string.IsNullOrEmpty(alias) ? "" : $" alias='{alias}'";
        var dateGroupingAttr = string.IsNullOrEmpty(dateGrouping) ? "" : $" dategrouping='{dateGrouping}'";
        _attributes.Add($"<attribute name='{attribute}'{aliasAttr} groupby='true'{dateGroupingAttr} />");
        _aggregate = true;
        return this;
    }


    public LinkEntityBuilder Filter(FilterBuilder filter)
    {
        _filters.Add(filter);
        return this;
    }

    public LinkEntityBuilder LinkEntity(LinkEntityBuilder link)
    {
        _linkEntities.Add(link);
        return this;
    }

    public LinkEntityBuilder OrderBy(string attribute, bool descending = false)
    {
        _orderBys.Add(new OrderByBuilder(attribute, descending));
        return this;
    }

    public string Build()
    {
        var aliasAttr = string.IsNullOrEmpty(_alias) ? "" : $" alias='{_alias}'";
        var aggregateAttr = _aggregate ? " aggregate='true'" : "";
        var sb = new StringBuilder();
        sb.Append($"<link-entity name='{_name}' from='{_from}' to='{_to}' link-type='{_linkType.ToLinkTypeString()}'{aliasAttr}{aggregateAttr}>");

        foreach (var attr in _attributes)
        {
            if (attr.StartsWith("<attribute")) // 兼容聚合
                sb.Append(attr);
            else
                sb.Append($"<attribute name='{attr}' />");
        }

        foreach (var filter in _filters)
            sb.Append(filter.Build());

        foreach (var link in _linkEntities)
            sb.Append(link.Build());

        foreach (var order in _orderBys)
            sb.Append(order.Build());

        sb.Append("</link-entity>");
        return sb.ToString();
    }
}

public class OrderByBuilder
{
    private string _attribute;
    private string _alias;
    private bool _descending;

    public OrderByBuilder(string attribute, bool descending)
    {
        _attribute = attribute;
        _descending = descending;
    }

    public OrderByBuilder(string attribute, string alias, bool descending)
    {
        _attribute = attribute;
        _alias = alias;
        _descending = descending;
    }

    public string Build()
    {
        if (!string.IsNullOrEmpty(_alias))
            return $"<order alias='{_alias}' descending='{_descending.ToString().ToLower()}' />";
        else
            return $"<order attribute='{_attribute}' descending='{_descending.ToString().ToLower()}' />";
    }
}
