using AweShur.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AweShur.Core.DataViews;

namespace AweShur.Web.Demo.Models.Project
{
    public class ProjectTask : BusinessBase
    {
        public override bool Validate()
        {
            bool valid = base.Validate();

            if (valid && this["hours"].NoNullInt() > 8 && this["overtimeNotes"].NoNullString() == "")
            {
                valid = false;
                LastErrorMessage = "You must write a note if hours > 8";
                LastErrorProperty = "hours";
            }

            return valid;
        }
    }

    public class ProjectTaskDecorator : BusinessBaseDecorator
    {
        protected override void SetCustomProperties()
        {
            base.SetCustomProperties();

            PropertyDefinition prop = Properties["idProject"];

            prop.Type = PropertyInputType.select;
            prop.DefaultValue = BusinessBaseProvider.ListProvider.GetList("Project").First[0];
            prop.ListObjectName = "Project";
            prop.Label = "Project";

            prop = Properties["idAppUser"];

            prop.Type = PropertyInputType.select;
            prop.DefaultValue = BusinessBaseProvider.ListProvider.GetList("AppUser").First[0];
            prop.ListObjectName = "AppUser";
            prop.Label = "AppUser";

            Properties["taskStart"].Label = "Start";
            Properties["overtimeNotes"].Label = "Notes (overtime)";
        }

        public override FilterBase GetFilter(string filterName)
        {
            return new ProjectTaskFilter(this);
        }
    }

    public class ProjectTaskFilter : FilterBase
    {
        public ProjectTaskFilter(BusinessBaseDecorator decorator) : base(decorator)
        {
        }

        public override void SetDataView(DataView dataView)
        {
            DataViewColumn col;

            base.SetDataView(dataView);

            dataView.FromClause = @"ProjectTask INNER JOIN
AppUser ON ProjectTask.idAppUser = AppUser.idAppUser INNER JOIN
Project ON ProjectTask.idProject = Project.idProject";

            dataView.Columns.Clear();  // No default columns.

            col = new DataViewColumn("ProjectTask.idProjectTask", "Id");
            col.IsID = true;
            col.Hidden = true;
            dataView.Columns.Add(col);

            dataView.Columns.Add(new DataViewColumn("Project.description", "Project"));
            dataView.Columns.Add(new DataViewColumn("AppUser.name + ' ' + AppUser.surname", "User"));
            dataView.Columns.Add(new DataViewColumn("ProjectTask.description", "Description"));

            col = new DataViewColumn("ProjectTask.taskStart", "Start");
            col.BasicType = BasicType.DateTime;
            dataView.Columns.Add(col);

            dataView.Columns.Add(new DataViewColumn("ProjectTask.hours", "Hours"));
        }
    }
}
