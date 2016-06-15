using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core.Lists
{
    public class ListTable
    {
        public int Count { get; }
        public List<object[]> Items { get; }
        public object[] ZeroItem { get; }

        public ListTable(int count, IEnumerable<dynamic> rows, string allDescription = "All")
        {
            //string[] names = null;

            Items = new List<object[]>(count);
            Count = count;

            foreach (dynamic item in rows)
            {
                //if (names == null)
                //{
                //    names = item.Keys.ToArray();
                //}

                //Items.Add(item.Values.ToArray());
            }

            ZeroItem = new object[] { 0, allDescription };
        }

        public object[] First
        {
            get
            {
                return Items[0];
            }
        }
    }
}
