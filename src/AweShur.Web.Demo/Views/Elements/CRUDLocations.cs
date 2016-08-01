using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web.Demo.Views.Elements
{
    public class CRUDLocations
    {
        public static Dictionary<string, CRUDLocationItem> Locations { get; set; }

        public static CRUDLocationItem Item(string key)
        {
            CRUDLocationItem item;

            if (!Locations.TryGetValue(key, out item))
            {
                item = new CRUDLocationItem { Folder = "Simple", ObjectName = key };

                Locations.Add(key, item);
            }

            if (item.eref == "")
            {
                item.eref = key.ToLower();
                item.Initialize();
            }

            return item;
        }

    }

    public class CRUDLocationItem
    {
        public string Folder { get; set; }
        public string ControlName { get; set; } = "";
        public string ObjectName { get; set; } = "";
        public string componentName { get; set; } = "";
        public string listhref { get; set; } = "";
        public string edithref { get; set; } = "";
        public string listelement { get; set; } = "";
        public string editelement { get; set; } = "";
        public string eref { get; set; } = "";

        public void Initialize()
        {
            if (ControlName == "")
            {
                ControlName = Folder;
            }

            componentName = "app-" + eref;

            if (Folder == "Simple")
            {
                eref = "simple";
            }

            if (ObjectName == "")
            {
                ObjectName = ControlName;
            }

            listhref = "/Elements/Load/" + Folder + "/" + ControlName + "ListItem/" + ObjectName;
            edithref = "/Elements/Load/" + Folder + "/" + ControlName + "Edit/" + ObjectName;
            listelement = "aswd-" + eref + "listitem";
            editelement = "aswd-" + eref + "edit";
        }
    }
}
