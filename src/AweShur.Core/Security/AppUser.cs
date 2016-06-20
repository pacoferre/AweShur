using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Security
{
    public partial class AppUser : BusinessBase
    {
        public AppUser() : base("AppUser")
        {

        }

        public override string Description
        {
            get
            {
                return this["name"] + " " + this["surname"];
            }
        }

        private string _newPassword = null;
        public override object this[string property]
        {
            get
            {
                if (property == "password")
                {
                    return "";
                }
                return base[property];
            }
            set
            {
                if (property == "password")
                {
                    if (value.NoNullString() != "")
                    {
                        string enc = PasswordDerivedString(this["email"].NoNullString(), value.ToString());

                        if (base[property].NoNullString() != enc)
                        {
                            base[property] = value;
                            _newPassword = enc;
                        }
                    }
                }
                else
                {
                    base[property] = value;
                }
            }
        }


        public string ShortDateFormat
        {
            get
            {
                return "dd-MM-yyyy"; // Pending translation.
            }
        }

        public IFormatProvider Culture
        {
            get
            {
                return null;
            }
        }
    }
}
