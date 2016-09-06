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
        public List<object[]> result { get; set; }
        public string fastsearch { get; set; } = "";
        public int sortIndex { get; set; } = 1;
        public string sortDir { get; set; } = "a";
        public Dictionary<string, string> data { get; set; } = null;
        public int topRecords { get; set; } = 100;
    }
}
