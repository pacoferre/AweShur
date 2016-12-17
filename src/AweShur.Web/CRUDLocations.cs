using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Web
{
    public class CRUDLocations
    {
        public static string ComponentsPrefix { get; set; } = "cust";

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
        public static string DefaultListEditorComponentName = "";

        public string Folder { get; set; }
        public string ControlName { get; set; } = "";
        public string ObjectName { get; set; } = "";
        public string FilterName { get; set; } = "";
        public string componentName { get; set; } = "";
        public string edithref { get; set; } = "";
        public string filterhref { get; set; } = "";
        public string editelement { get; set; } = "";
        public string filterelement { get; set; } = "";
        public string eref { get; set; } = "";
        public string listEditorComponentName { get; set; } = DefaultListEditorComponentName;

        public void Initialize()
        {
            if (ControlName == "")
            {
                ControlName = Folder;
            }

            componentName = "app-" + eref;

            if (ObjectName == "")
            {
                ObjectName = ControlName;
            }

            edithref = "/Elements/Load/" + Folder + "/" + ControlName + "Edit/" + ObjectName;
            editelement = CRUDLocations.ComponentsPrefix + "-" + eref + "edit";
            filterhref = "/Elements/Load/" + Folder + "/" + ControlName + "Filter/" + ObjectName;
            filterelement = CRUDLocations.ComponentsPrefix + "-" + eref + "filter";
        }
    }
}
