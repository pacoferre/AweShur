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
            string folder = Folder;
            string controlName = ControlName;
            string objectName = ObjectName;

            if (controlName == "")
            {
                controlName = folder;
            }

            componentName = "app-" + eref;

            if (folder == "Simple")
            {
                eref = "simple";
            }

            if (objectName == "")
            {
                objectName = controlName;
            }

            listhref = "/Elements/Load/" + folder + "/" + controlName + "ListItem/" + objectName;
            edithref = "/Elements/Load/" + folder + "/" + controlName + "Edit/" + objectName;
            listelement = "aswd-" + eref + "listitem";
            editelement = "aswd-" + eref + "edit";
        }
    }
}
