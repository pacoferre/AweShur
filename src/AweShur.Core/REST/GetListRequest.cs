using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class GetListRequest
    {
        public string objectName { get; set; }
        public string listName { get; set; }
        public string parameter { get; set; }
        public string other { get; set; }
    }
}
