using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Core
{
    public class Ref<T> where T : class
    {
        public Ref() { }
        public Ref(T value) { Value = value; }
        public T Value { get; set; }
        public static implicit operator T(Ref<T> r) { return r.Value; }
        public static implicit operator Ref<T>(T value) {  return new Ref<T>(value); }
    }
}
