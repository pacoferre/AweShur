using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public IEnumerable<dynamic> Get()
        {
            return CurrentDB.Query("Select * From " + TableName
                + " Order By " + Definition.FirstStringProperty.PropertyName);
        }
    }
}
