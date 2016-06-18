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
                return this["Name"] + " " + this["SurName"];
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
