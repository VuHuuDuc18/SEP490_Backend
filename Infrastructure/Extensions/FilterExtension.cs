using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Domain.Dto.Request;

namespace Infrastructure.Extensions
{
    public static class FilterExtension
    {
        public static IQueryable<T> Filter<T>(this IQueryable<T> input, List<SearchObjectForCondition> filter)
        {
            if (filter == null || !filter.Any())
                return input;

            foreach (var item in filter)
            {
                if (string.IsNullOrEmpty(item.Field) || item.Value == null)
                    continue;

                PropertyInfo getter = typeof(T).GetProperty(item.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (getter == null)
                    continue;

                var param = Expression.Parameter(typeof(T), "item");
                object convertedValue = null;

                try
                {
                    if (getter.PropertyType == typeof(Guid))
                    {
                        if (Guid.TryParse(item.Value, out var guidValue))
                            convertedValue = guidValue;
                        else
                            continue; // Skip if Guid parsing fails
                    }
                    else
                    {
                        convertedValue = Convert.ChangeType(item.Value, getter.PropertyType);
                    }

                    var body = Expression.Equal(
                        Expression.Property(param, getter.Name),
                        Expression.Constant(convertedValue, getter.PropertyType));

                    var lambda = Expression.Lambda<Func<T, bool>>(body, param);
                    input = input.Where(lambda);
                }
                catch
                {
                    // Skip if conversion or expression building fails
                    continue;
                }
            }
            return input;
        }
    }
}