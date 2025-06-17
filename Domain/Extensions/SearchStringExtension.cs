
using Domain.Dto.Request;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace DEMO.Domain.Extensions
{
    public static class SearchStringExtension
    {
        public static IQueryable<T> SearchString<T>(this IQueryable<T> input, List<SearchObjectForCondition> searchCds)
        {
            var param = Expression.Parameter(typeof(T));
            Expression conbineSearch = null;

            if (searchCds.Count > 0)
            {
                foreach (var item in searchCds)
                {
                    PropertyInfo getter = typeof(T).GetProperty(item.Field);

                    var property = Expression.PropertyOrField(param, getter.Name);

                    if (getter != null)
                    {
                        //var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var body = Expression.Call(
                            property,
                            method: typeof(string).GetMethod("Contains", new[] { typeof(string) }),
                            Expression.Constant(item.Value)
                            );

                        if (conbineSearch == null)
                        {
                            conbineSearch = body;
                        }
                        else
                        {
                            conbineSearch = Expression.OrElse(body, conbineSearch);
                        }


                    }
                }

                MethodCallExpression result = Expression.Call(
                             typeof(Queryable),
                             "where",
                             new[] { typeof(T) },
                             input.Expression,
                             Expression.Lambda<Func<T, bool>>(conbineSearch, param)
                             );
                input = input.Provider.CreateQuery<T>(result);
            }


            return input;
        }
    }
}
