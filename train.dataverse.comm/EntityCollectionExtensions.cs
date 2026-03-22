using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace train.dataverse.comm;

public static class EntityCollectionExtensions
{
	/// <summary>
	/// 安全地获取实体集合中的实体
	/// </summary>
	public static IEnumerable<Entity> GetEntitiesSafe(this EntityCollection collection)
	{
		return collection?.Entities ?? Enumerable.Empty<Entity>();
	}

	/// <summary>
	/// 将实体集合转换为指定类型的列表
	/// </summary>
	public static List<T> ToList<T>(this EntityCollection collection, Func<Entity, T> selector)
	{
		return collection.GetEntitiesSafe().Select(selector).ToList();
	}

	/// <summary>
	/// 获取分页信息
	/// </summary>
	public static (bool hasMore, string pagingCookie) GetPagingInfo(this EntityCollection collection)
	{
		return (collection.MoreRecords, collection.PagingCookie);
	}

	/// <summary>
	/// 获取记录总数
	/// </summary>
	public static int GetTotalRecordCount(this EntityCollection collection)
	{
		return collection?.TotalRecordCount ?? 0;
	}

	/// <summary>
	/// 检查实体集合是否为空
	/// </summary>
	public static bool IsEmpty(this EntityCollection collection)
	{
		return collection == null || collection.Entities.Count == 0;
	}

	/// <summary>
	/// 获取实体集合中的第一个实体，如果集合为空则返回 null
	/// </summary>
	public static Entity FirstOrDefault(this EntityCollection collection)
	{
		return collection.GetEntitiesSafe().FirstOrDefault()!;
	}

	/// <summary>
	/// 安全地执行针对每个实体的操作
	/// </summary>
	public static void ForEach(this EntityCollection collection, Action<Entity> action)
	{
		foreach (var entity in collection.GetEntitiesSafe())
		{
			action(entity);
		}
	}
}
