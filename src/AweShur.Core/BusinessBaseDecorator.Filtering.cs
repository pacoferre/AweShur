using AweShur.Core.DataViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBaseDecorator
    {
        public virtual FilterBase GetFilter(string filterName)
        {
            return new FilterBase(this, DBNumber);
        }
    }
}
