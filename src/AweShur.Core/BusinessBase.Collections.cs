using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class BusinessBase
    {
        protected Dictionary<string, BusinessCollectionBase> objetosSub 
            = new Dictionary<string, BusinessCollectionBase>();

        public BusinessCollectionBase Parent { get; set; } = null;

        public virtual bool MatchFilter(string filterName)
        {
            return true;
        }

        public virtual BusinessCollectionBase Collection(string collectionName)
        {
            BusinessCollectionBase col = NotEnsuredCollection(collectionName);

            col.EnsureList();

            return col;
        }

        public virtual BusinessCollectionBase NotEnsuredCollection(string collectionName)
        {
            BusinessCollectionBase col = null;

            try
            {
                col = objetosSub[collectionName];
                if (col == null)
                {
                    throw new Exception("Not possible.");
                }
            }
            catch
            {
                throw new ArgumentException("Collection " + collectionName + " not defined in " + this.ObjectName + ".");
            }

            return col;
        }

        public virtual bool IsCollection(string collectionName)
        {
            return objetosSub.ContainsKey(collectionName);
        }
    }
}
