using System.Linq.Expressions;
using eShop.Catalog.API.Models;

namespace eShop.Catalog.API.Helper.Builder;

/// <summary>
/// 筛选器构建类
/// </summary>
/// <typeparam name="TEntity">数据库实体类型</typeparam>

public class FilterBuilder<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// 筛选表达式列表
    /// </summary>
    protected readonly List<Expression<Func<TEntity, bool>>> FilterExpression = [];

    /// <summary>
    /// 构建筛选条件
    /// </summary>
    /// <returns>筛选条件表达式</returns>
    public Expression<Func<TEntity, bool>> Build()
    {
        if (FilterExpression.Count == 0)
        {
            return _ => true;
        }
        var result = FilterExpression[0];
        for (int i = 1; i < FilterExpression.Count; i++)
        {
            result = CombineFilters(result, FilterExpression[i]);
        }
        return result;
    }

    /// <summary>
    /// 组合筛选条件
    /// </summary>
    /// <returns>筛选条件表达式</returns>
    private Expression<Func<TEntity, bool>> CombineFilters(Expression<Func<TEntity, bool>> baseExpression, Expression<Func<TEntity, bool>> filterExpression)
    {
        var param = Expression.Parameter(typeof(TEntity), "param");
        var expressionBody = Expression.AndAlso(
            Expression.Invoke(baseExpression, param),
            Expression.Invoke(filterExpression, param));
        return Expression.Lambda<Func<TEntity, bool>>(expressionBody, param);
    }
}
