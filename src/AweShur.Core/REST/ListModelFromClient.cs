﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class ListModelFromClient
    {
        public string oname { get; set; } = "";
        public string filterName { get; set; } = "";
        public int sortIndex { get; set; } = 0;
        public string sortDir { get; set; } = "";
        public bool dofastsearch { get; set; } = false;
        public string fastsearch { get; set; } = "";
        public Dictionary<string, string> data { get; set; } = new Dictionary<string, string>();
        public int topRecords { get; set; } = 100;
        public bool first = true;
    }
}
