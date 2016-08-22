using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class ModelFromClient
    {
        public string oname { get; set; } = "";
        public string formToken { get; set; } = "";
        public int sequence { get; set; } = 0;
        public string action { get; set; } = "";

        public List<string> listNames { get; set; }
        public List<string> dataNames { get; set; }
        public ModelFromClientData root { get; set; }
    }

    public class ModelFromClientData
    {
        public string key { get; set; } = "";
        public Dictionary<string, string> data { get; set; }
        public List<ModelFromClientCollection> children { get; set; }
    }

    public class ModelFromClientCollection
    {
        public string path { get; set; } = "";
        public List<string> dataNames { get; set; }
        public List<ModelFromClientData> elements { get; set; }
    }
}

