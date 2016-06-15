using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Security
{
    public class AppUser : BusinessBase
    {
        public AppUser() : base("AppUser")
        {

        }

        public string ShortDateFormat
        {
            get
            {
                return "dd-MM-yyyy"; // Pending traduction.
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
