using AweShur.Core.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBaseDecorator
    {
        public Tuple<Dictionary<string, DataViewColumn>, string> GetFilterColumnsAndFromClause(string ElementName = "")
        {
            Tuple<Dictionary<string, DataViewColumn>, string> resp
                = new Tuple<Dictionary<string, DataViewColumn>, string>
                    (new Dictionary<string, DataViewColumn>(2), tableNameEncapsulated);

            resp.Item1.Add("ID", new DataViewColumn(ListProperties[0]));
            resp.Item1.Add("C1", new DataViewColumn(FirstStringProperty));

            return resp;
        }
    }
}
