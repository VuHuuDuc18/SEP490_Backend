using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Core
{
    public class CheckError
    {
        public bool isError {  get; set; }
        public bool IsError { get; internal set; }
        public string Code { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
    }
}
