using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Domain.Dto.Request;

namespace Infrastructure.Extensions
{
    public static class SearchStringExtension
    {
        public static IQueryable<T> SearchString<T>(this IQueryable<T> input, List<SearchObjectForCondition> searchCds)
        {

            if (searchCds == null || !searchCds.Any())
                return input;


            var param = Expression.Parameter(typeof(T), "item");
            Expression combineSearch = null;

            foreach (var item in searchCds)
            {
                if (string.IsNullOrEmpty(item.Field) || string.IsNullOrEmpty(item.Value))
                    continue;

                PropertyInfo getter = typeof(T).GetProperty(item.Field);
                if (getter == null || getter.PropertyType != typeof(string))
                    continue;

                var property = Expression.Property(param, getter.Name);
                var body = Expression.Call(
                    property,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                    Expression.Constant(item.Value));

                combineSearch = combineSearch == null ? body : Expression.OrElse(body, combineSearch);
            }

            if (combineSearch != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(combineSearch, param);
                input = input.Where(lambda);
            }

            return input;
        }
    }
}