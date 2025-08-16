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
                Expression propertyAccess = Expression.Property(param, getter.Name);
                object convertedValue = null;

                try
                {
                    if (getter.PropertyType == typeof(Guid) || getter.PropertyType == typeof(Guid?))
                    {
                        if (Guid.TryParse(item.Value, out var guidValue))
                            convertedValue = guidValue;
                        else
                            continue;
                    }
                    else if (getter.PropertyType == typeof(DateTime) || getter.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(item.Value, out var dateValue))
                        {
                            convertedValue = dateValue.Date;

                            Expression dateProperty;
                            if (getter.PropertyType == typeof(DateTime?))
                            {
                                var valueProperty = Expression.Property(propertyAccess, "Value");
                                dateProperty = Expression.Property(valueProperty, nameof(DateTime.Date));
                            }
                            else
                            {
                                dateProperty = Expression.Property(propertyAccess, nameof(DateTime.Date));
                            }

                            var dateComparison = Expression.Equal(
                                dateProperty,
                                Expression.Constant(dateValue.Date, typeof(DateTime)));

                            var dateLambda = Expression.Lambda<Func<T, bool>>(dateComparison, param);
                            input = input.Where(dateLambda);
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        Type targetType = Nullable.GetUnderlyingType(getter.PropertyType) ?? getter.PropertyType;
                        convertedValue = Convert.ChangeType(item.Value, targetType);
                    }

                    var comparison = Expression.Equal(
                        propertyAccess,
                        Expression.Constant(convertedValue, getter.PropertyType));

                    var lambda = Expression.Lambda<Func<T, bool>>(comparison, param);
                    input = input.Where(lambda);
                }
                catch
                {
                    continue;
                }
            }

            return input;
        }
    }
}