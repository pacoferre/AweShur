using AweShur.Core.REST;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AweShur.Core
{
    public partial class FilterBase
    {
        public virtual ListModelToClient ProcessRequestAndCreateResponse(HttpContext context, ListModelFromClient request)
        {
            ListModelToClient resp = new ListModelToClient();

            this.FastSearchActivated = request.dofastsearch;
            this.FastSearch = request.fastsearch;
            if (!request.first)
            {
                this.Filter = request.data;
                this.topRecords = request.topRecords;
            }

            if (this.Filter == null)
            {
                this.Filter = request.data;
                this.Clear();
            }

            if (request.sortDir != "asc" && request.sortDir != "desc")
            {
                request.sortDir = "asc";
            }

            resp.plural = this.Decorator.Plural;
            resp.data = this.Filter;
            resp.result = Dapper.SqlMapper.ToList(this.Get(request.sortIndex,
                (request.sortDir == "asc" ? SortDirection.Ascending : SortDirection.Descending),
                0, 0));
            resp.fastsearch = this.FastSearch;
            resp.sortIndex = request.sortIndex;
            resp.sortDir = request.sortDir;
            resp.topRecords = this.topRecords;

            this.SetExtraToClientResponse(resp);

            return resp;
        }
        protected virtual void SetExtraToClientResponse(ListModelToClient model)
        {

        }
    }
}
