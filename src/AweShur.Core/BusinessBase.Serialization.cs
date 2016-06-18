using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        public virtual byte[] Serialize()
        {
            return dataItem.Serialize();
        }

        public void Deserialize(byte[] data)
        {
            dataItem.Deserialize(data);
        }
    }
}
