using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.REST
{
    public class ModelToClient
    {
        public string formToken { get; set; } = "";
        public int sequence { get; set; } = 0;
        public string action { get; set; } = "";
        public bool ok { get; set; }
        public string keyObject { get; set; }
        public string keyObjectUp { get; set; }
        public string keyObjectDown { get; set; }
        public string[] keysObjectInternal { get; set; }
        public bool goBack { get; set; }

        public bool wasModified { get; set; }
        public bool wasNew { get; set; }
        public bool wasDeleting { get; set; }

        public bool isModified { get; set; }
        public bool isNew { get; set; }
        public bool isDeleting { get; set; }

        public Dictionary<string, string> data { get; set; }
        public Dictionary<string, string> props { get; set; }

        public List<BusinessTool> tools { get; set; }
        public Dictionary<string, List<Tuple<string, string>>> listItems { get; set; }
        public List<string> refreshLists { get; set; }

        public string title { get; set; }

        public string errorMessage { get; set; }
        public string errorColection { get; set; }
        public string errorColectionKey { get; set; }
        public string errorProperty { get; set; }
        public string normalMessage { get; set; }
        public string resultData { get; set; }
        public string resultOpenURL { get; set; }

        public int filterPosition { get; set; }
        public bool refreshFilterTable { get; set; }
        public bool refreshFields { get; set; }
        public bool refreshAllLists { get; set; }

        public BusinessObjectPermisson permisson = new BusinessObjectPermisson();

        public ModelToClient()
        {
            this.ok = true;
            this.normalMessage = "";
            this.errorMessage = "";
            this.errorColection = "";
            this.errorColectionKey = "";
            this.errorProperty = "";
            this.goBack = false;
            this.filterPosition = -1;
            this.refreshFilterTable = false;

            this.data = null;
            this.props = null;
            this.tools = null;
            this.refreshLists = null;

            this.keyObjectUp = "";
            this.keyObjectDown = "";
        }

        public static ModelToClient ErrorResponse(string message)
        {
            ModelToClient model = new ModelToClient();

            model.ok = false;
            model.errorMessage = message;

            return model;
        }
    }

    public class BusinessObjectPermisson
    {
        public bool modify { get; set; }
        public bool delete { get; set; }
        public bool add { get; set; }
    }

    public class BusinessTool
    {
        public string name = "";
        public string caption = "";
        public string type = "";
        public string colName = "";
        public string method = "";
        public string parameter = "";
        public string urlBase = "";
        public bool visible = true;
        public string title = "";
        public string explanation = "";

        public BusinessTool(string name, string caption, string type)
        {
            this.name = name;
            this.caption = caption;
            this.type = type;

            if (!(this.type == "addColForm"
                || this.type == "addColModal"
                || this.type == "doMethod"
                || this.type == "goURL"
                || this.type == "openModal"))
            {
                throw new Exception("Incorrect type: " + type);
            }
        }
    }
}
