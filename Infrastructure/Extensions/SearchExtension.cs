
using Domain.Dto.Request;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Infrastructure.Extensions
{
    public static class SearchExtension
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> input, List<SearchObjectForCondition> searchCds)
        {
            foreach (var item in searchCds)
            {
                PropertyInfo getter = typeof(T).GetProperty(item.Field);
                var param = Expression.Parameter(typeof(T),item.Value);
                var property = Expression.PropertyOrField(param,getter.Name);

                if (getter != null)
                {
                    //var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var body = Expression.Call(
                        property,
                        typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                        Expression.Constant(item.Value)                       
                        );

                    MethodCallExpression result = Expression.Call(
                         typeof(Queryable),
                         "where",
                         new[] { typeof(T) },
                         input.Expression,
                         Expression.Lambda<Func<T, bool>>(body, param)
                         );

                    input = input.Provider.CreateQuery<T>(result);
                }
            }

            return input;
        }
    }
}
