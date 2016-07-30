using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AweShur.Core.Security
{
    public class AppUserNoDBDecorator : AweShur.Core.Security.AppUserDecorator
    {
        public override void SetProperties(string tableName, int dbNumber)
        {
            this.tableName = tableName;

            SetCustomProperties();
        }

        protected override void SetCustomProperties()
        {
            //idAppUser   int Unchecked
            //email nvarchar(200)   Unchecked
            //password    nvarchar(100)   Unchecked
            //name    nvarchar(50)    Unchecked
            //surname nvarchar(50)    Unchecked
            //su  bit Unchecked
            //deactivated bit Unchecked
            PropertyDefinition temp =
                (Properties["idAppUser"]
                    = new PropertyDefinition("idAppUser", "ID", BasicType.Number, 
                        PropertyInputType.number, true));
            temp.DataType = typeof(int);
            temp.DefaultValue = 1;

            temp = (Properties["email"]
                    = new PropertyDefinition("email", "Email", BasicType.Text, PropertyInputType.text));
            temp.MaxLength = 200;
            temp.DataType = typeof(string);
            temp.DefaultValue = "";

            temp = (Properties["password"]
                    = new PropertyDefinition("password", "Password", BasicType.Text, PropertyInputType.password));
            temp.MaxLength = 100;
            temp.DataType = typeof(string);
            temp.DefaultValue = "";

            temp = (Properties["name"]
                    = new PropertyDefinition("name", "Name", BasicType.Text, PropertyInputType.text));
            temp.MaxLength = 50;
            temp.DataType = typeof(string);
            temp.DefaultValue = "";

            temp = (Properties["surname"]
                    = new PropertyDefinition("surname", "Surname", BasicType.Text, PropertyInputType.text));
            temp.MaxLength = 50;
            temp.DataType = typeof(string);
            temp.DefaultValue = "";

            temp = (Properties["su"]
                    = new PropertyDefinition("su", "SU", BasicType.Bit, PropertyInputType.checkbox));
            temp.DataType = typeof(bool);
            temp.DefaultValue = false;

            temp = (Properties["deactivated"]
                    = new PropertyDefinition("deactivated", "Deactivated", BasicType.Bit, PropertyInputType.checkbox));
            temp.DataType = typeof(bool);
            temp.DefaultValue = false;

            base.SetCustomProperties();

            PostSetCustomProperties();
        }
    }

    public class AppUserNoDB: AweShur.Core.Security.AppUser
    {
        public AppUserNoDB() : base("AppUserNoDB", true)
        {
        }

        public override void StoreToDB()
        {
            // Nothing to do.
            //base.StoreToDB();
        }

        public static void LoginWindows(HttpContext context)
        {
            AppUser usu = null;

            UseAppUserNoDB = true;

            usu = (AppUser)BusinessBaseProvider.Instance.CreateObject("AppUserNoDB");

            usu.SetNew();

            usu["idAppUser"] = 1;

            usu["name"] = "User";
            usu["surname"] = context.User.Identity.Name;

            usu.IsNew = false;

            SetAppUser(usu, context);
        }
    }
}
