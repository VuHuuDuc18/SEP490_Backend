using Domain.Dto.Request;
using Domain.DTOs.Request.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extensions
{
    public static class IncludeObjectSearchStringExtension
    {
        public static IQueryable<OrderResponse> SearchStringIncludeObject(this IQueryable<OrderResponse> input, List<SearchObjectForCondition> searchCds)
        {
            if (searchCds == null || !searchCds.Any())
                return input;

            var param = Expression.Parameter(typeof(OrderResponse), "item");
            Expression combineSearch = null;

            foreach (var condition in searchCds)
            {
                if (string.IsNullOrEmpty(condition.Field) || string.IsNullOrEmpty(condition.Value))
                    continue;

                Expression property = param;
                Type currentType = typeof(OrderResponse);
                bool isValid = true;

                foreach (var part in condition.Field.Split('.'))
                {
                    PropertyInfo getter = currentType.GetProperty(part);
                    if (getter == null)
                    {
                        isValid = false;
                        break;
                    }
                    property = Expression.Property(property, getter);
                    currentType = getter.PropertyType;
                }

                if (!isValid || property.Type != typeof(string))
                    continue;

                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var body = Expression.Call(property, containsMethod, Expression.Constant(condition.Value));

                combineSearch = combineSearch == null ? body : Expression.OrElse(body, combineSearch);
            }

            if (combineSearch != null)
            {
                var lambda = Expression.Lambda<Func<OrderResponse, bool>>(combineSearch, param);
                input = input.Where(lambda);
            }

            return input;
        }
    }
}
