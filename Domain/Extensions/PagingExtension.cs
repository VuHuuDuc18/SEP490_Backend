using Domain.Dto.Request;
using Domain.Dto.Response;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Extensions
{
    public static class PagingExtension
    {
        public static async Task<PaginationSet<T>> Pagination<T>(this IQueryable<T> sourse, int pageIndex, int pageSize, SearchObjectForCondition sortExpressions)
        {
            var totalCount = sourse.Count();
            var param = Expression.Parameter(typeof(T), "item");

            var sortExpression = Expression.Lambda<Func<T, object>>
                (Expression.Convert(Expression.Property(param, sortExpressions.Field), typeof(object)), param);

            if (sortExpressions == null || sortExpressions.Value != "asc")
            {
                sourse = sourse.OrderByDescending(sortExpression);
            }
            else
            {
                sourse = sourse.OrderBy(sortExpression);
            }

            var items = await sourse.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = new PaginationSet<T>
            {
                PageIndex = pageIndex,
                Count = items?.Count ?? 0,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Items = items
            };
            return result;
        }
    }
}
