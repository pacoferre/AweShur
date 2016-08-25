using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class ListModelToClient
    {
        public string plural { get; set; }
        public BusinessObjectPermisson permisson = new BusinessObjectPermisson();
        public List<object[]> data { get; set; }
        public string fastsearch { get; set; } = "";
        public int sortIndex { get; set; } = 1;
        public string sortDir { get; set; } = "a";
    }
}
