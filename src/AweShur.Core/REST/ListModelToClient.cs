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
        public IEnumerable<dynamic> data { get; set; }
    }
}
