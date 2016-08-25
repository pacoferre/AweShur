using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class ListModelFromClient
    {
        public string oname { get; set; } = "";
        public int sortIndex { get; set; } = 0;
        public string sortDir { get; set; } = "";
        public bool dofastsearch { get; set; } = false;
        public string fastsearch { get; set; } = "";
    }
}
