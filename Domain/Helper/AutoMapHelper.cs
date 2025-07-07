using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Helper
{
    public static class AutoMapperHelper
    {
        public static TDestination AutoMap<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null) return default;

            var destination = new TDestination();
            var sourceProps = typeof(TSource).GetProperties();
            var destProps = typeof(TDestination).GetProperties();

            foreach (var destProp in destProps)
            {
                var sourceProp = sourceProps.FirstOrDefault(x =>
                    x.Name == destProp.Name &&
                    destProp.PropertyType.IsAssignableFrom(x.PropertyType)
                );

                if (sourceProp != null)
                {
                    var value = sourceProp.GetValue(source, null);
                    destProp.SetValue(destination, value, null);
                }
            }

            return destination;
        }
    }
}
