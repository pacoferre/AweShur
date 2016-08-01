using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessCollectionBase
    {
        public bool ClientRefreshPending { get; set; } = false;
    }
}
