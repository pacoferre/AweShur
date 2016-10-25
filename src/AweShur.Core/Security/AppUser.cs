using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Security
{
    public class AppUserDecorator : BusinessBaseDecorator
    {
        protected override void SetCustomProperties()
        {
            Properties["password"].NoChecking = true;

            Singular = "User";
            Plural = "Users";

            Properties["email"].IsOnlyOnNew = true;
            Properties["email"].Type = PropertyInputType.email;
            Properties["password"].Type = PropertyInputType.password;
        }
    }

    public partial class AppUser : BusinessBase
    {
        public AppUser()
        {

        }

        public AppUser(bool noDB) : base(noDB)
        {
        }

        public override string Description
        {
            get
            {
                return this["name"] + " " + this["surname"];
            }
        }

        private string newPassword = null;

        public override bool Validate()
        {
            bool isValid = base.Validate();

            if (isValid)
            {
                if (IsNew && newPassword.NoNullString() == "")
                {
                    LastErrorMessage = "Password must be set";
                    isValid = false;
                }
            }

            return isValid;
        }

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
                        newPassword = value.ToString();
                        base[property] = "NWRE";
                    }
                }
                else
                {
                    base[property] = value;
                }
            }
        }

        protected override void AfterStoreToDB(bool wasNew, bool wasModified, bool wasDeleting)
        {
            if (newPassword != null)
            {
                string enc = PasswordDerivedString(this["idAppUser"].NoNullString(), newPassword.ToString());

                CurrentDB.Execute("update AppUser set password = @password Where idAppUser = @id", new { password = enc, id = this["idAppUser"].NoNullString() });

                newPassword = null;

                ReadFromDB();
            }
        }

        public string ShortDateFormat
        {
            get
            {
                return "dd/MM/yyyy"; // Pending translation.
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
