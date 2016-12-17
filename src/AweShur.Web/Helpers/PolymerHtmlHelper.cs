using AweShur.Core;
using AweShur.Core.DataViews;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AweShur.Web.Helpers
{
    public static class PolymerHtmlHelper
    {
        public static readonly string CRUDElementsTAG = "crudelements";

        //public static bool ViewExists(this HtmlHelper htmlHelper, string name)
        //{
        //    ViewEngineResult result = Microsoft.AspNetCore.Mvc.ViewEngines.Engines.FindView(htmlHelper.ViewContext.Controller.ControllerContext, name, null);

        //    return (result.View != null);
        //}

        public static void AddLinkImport(RazorPage page, string url)
        {
            object list;

            if (!page.Context.Items.ContainsKey("limps"))
            {
                list = new List<string>();

                page.Context.Items["limps"] = list;
                page.Context.Items["limpssb"] = new StringBuilder();

                page.DefineSection(CRUDElementsTAG, new RenderAsyncDelegate(writer =>
                        writer.WriteAsync(page.Context.Items["limpssb"].ToString())
                    ));
            }
            else
            {
                list = page.Context.Items["limps"];
            }

            if (!((List<string>) list).Contains(url))
            {
                ((List<string>)list).Add(url);

                //<link rel="import" href="/lib/polymer/polymer.html">

                ((StringBuilder)page.Context.Items["limpssb"])
                    .AppendLine(@"<link rel=""import"" href=""" + url + "\">");
            }
        }

        public static IHtmlContent PolEditor(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, Dictionary<string, string> other = null)
        {
            return PolEditor(htmlHelper, baseDefinition, propertyName, "", false, false, other);
        }
        public static IHtmlContent PolEditor(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, bool NoLabel, Dictionary<string, string> other = null)
        {
            return PolEditor(htmlHelper, baseDefinition, propertyName, "", NoLabel, false, other);
        }
        public static IHtmlContent PolEditor(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, string Colection, Dictionary<string, string> other = null)
        {
            return PolEditor(htmlHelper, baseDefinition, propertyName, Colection, false, false, other);
        }
        public static IHtmlContent PolEditor(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, string Colection, bool NoLabel, Dictionary<string, string> other = null)
        {
            return PolEditor(htmlHelper, baseDefinition, propertyName, Colection, NoLabel, false, other);
        }
        public static IHtmlContent PolEditor(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, string Colection, bool NoLabel, bool NoRequired, Dictionary<string, string> other = null)
        {
            //<label>
            //    Nombre <small>required</small>
            //    <input type="text" />
            //</label>
            PropertyDefinition def;
            TagBuilder input;
            bool forceReadOnly = htmlHelper.ViewData.ContainsKey("forceReadOnly");

            RazorPage page = (RazorPage)((RazorView)htmlHelper.ViewContext.View).RazorPage;

            def = baseDefinition.Properties[propertyName];
            if (def == null)
            {
                throw new Exception("Property " + propertyName + " not found in " + baseDefinition.Singular);
            }

            if (def.Type == PropertyInputType.select)
            {
                //<awsl-selector data-field="idCustomerType" label="Type" list-name="CustomerType"></awsl-selector>
                input = new TagBuilder("awsl-selector-vaadin");

                AddLinkImport(page, "/AWSLib/awsl-selector-vaadin");

                input.Attributes["prevent-invalid-input"] = "";

                if (def.ListObjectName != "")
                {
                    input.Attributes["object-name"] = def.ListObjectName;
                    input.Attributes["list-name"] = def.ListName;
                }
            }
            else if (def.Type == PropertyInputType.checkbox)
            {
                input = new TagBuilder("paper-checkbox");

                if (def.Label != "")
                {
                    if (def.LabelIsFieldName)
                    {
                        input.InnerHtml.SetContent("[[item.data." + def.Label + "]]");
                    }
                    else
                    {
                        input.InnerHtml.SetContent(def.Label);
                    }
                }

                AddLinkImport(page, "/lib/paper-checkbox/paper-checkbox.html");
            }
            else if (def.Type == PropertyInputType.radio)
            {
                input = new TagBuilder("input");
                input.Attributes["type"] = def.Type.ToString();

//                AddLinkImport(page, "");
            }
            else if ((def.Type == PropertyInputType.date || def.Type == PropertyInputType.datetimeHHmm || def.Type == PropertyInputType.datetimeHHmmss)
                && !def.IsReadOnly)
            {
                //<awsl-date data-field="nextCall" label="Next call"  noclear></awsl-date>
                input = new TagBuilder("awsl-date");

                AddLinkImport(page, "/AWSLib/awsl-date");
                if (!def.IsNullable)
                {
                    input.Attributes["noclear"] = "";
                }
            }
            else
            {
                //text,
                //date,
                //email,
                //textarea,
                //number,
                //time,
                //datetime,
                //tel,
                //range,
                if (def.Type == PropertyInputType.textarea)
                {
                    input = new TagBuilder("paper-textarea");
                    AddLinkImport(page, "/lib/paper-input/paper-textarea.html");

                    if (def.Rows > 0)
                    {
                        input.Attributes["rows"] = def.Rows.ToString();
                    }
                }
                else if (def.Type == PropertyInputType.email)
                {
                    input = new TagBuilder("gold-email-input");
                    AddLinkImport(page, "/lib/gold-email-input/gold-email-input.html");
                }
                else
                {
                    input = new TagBuilder("paper-input");
                    AddLinkImport(page, "/lib/paper-input/paper-input.html");

                    if (def.Type == PropertyInputType.password)
                    {
                        input.Attributes["type"] = "password";
                    }
                }

                if (def.MaxLength > 0)
                {
                    input.Attributes["maxlength"] = def.MaxLength.ToString();
                }

                //if (def.Required && !def.NoLabelRequired && !NoRequired)
                //{
                //    input.Attributes["required"] = "";

                //    //<small class="error">Nombre de usuario required.</small>
                //    TagBuilder sError = new TagBuilder("small");

                //    sError.AddCssClass("error");
                //    sError.InnerHtml.SetContent(def.RequiredErrorMessage);
                //    errorMessage = sError.ToString();

                //    if (!NoLabel)
                //    {
                //        //TagBuilder sReq = new TagBuilder("small");
                //        //sReq.SetInnerText("*");
                //        //required = " " + sReq.ToString();
                //        required = " *";
                //    }
                //    if (def.Pattern != "")
                //    {
                //        input.Attributes["pattern"] = def.Pattern;
                //    }
                //}
            }

            input.Attributes["data-field"] = def.FieldName;
            if (def.LabelIsFieldName)
            {
                input.Attributes["data-field"] += "," + def.Label;
            }

            //Friki Polymer :)
            //input.Attributes["value"] = "{{item.data." + def.FieldName + "}}";

            if (def.IsOnlyOnNew)
            {
                input.Attributes["onlynew"] = "";
            }

            if (def.IsReadOnly || forceReadOnly)
            {
                input.Attributes["readonly"] = "";
            }

            if (other != null)
            {
                foreach (string key in other.Keys)
                {
                    input.Attributes[key] = other[key];
                }
            }

            if (def.Label != "" && !NoLabel && def.Type != PropertyInputType.checkbox)
            {
                if (def.AlwaysFloatLabel)
                {
                    input.Attributes["always-float-label"] = "";
                }
                if (def.LabelIsFieldName)
                {
                    input.Attributes["label"] = "[[" + def.Label + "]]";
                }
                else
                {
                    input.Attributes["label"] = def.Label;
                }
            }
            else
            {
                input.Attributes["no-label-float"] = "";
            }

            //htmlHelper.ViewContext.TempData[""] = "";

            return input;
        }

        public static IHtmlContent PolSearch(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, Dictionary<string, string> other = null)
        {
            return PolSearch(htmlHelper, baseDefinition, propertyName, false, other);
        }

        public static IHtmlContent PolSearch(this IHtmlHelper htmlHelper, BusinessBaseDecorator baseDefinition, string propertyName, bool NoLabel, Dictionary<string, string> other = null)
        {
            PropertyDefinition def;
            TagBuilder input;
            RazorPage page = (RazorPage)((RazorView)htmlHelper.ViewContext.View).RazorPage;

            def = baseDefinition.Properties[propertyName];
            if (def == null)
            {
                throw new Exception("Property " + propertyName + " not found in " + baseDefinition.Singular);
            }

            if (def.Type == PropertyInputType.select)
            {
                input = new TagBuilder("awsl-selector-vaadin");

                AddLinkImport(page, "/AWSLib/awsl-selector-vaadin");

                input.Attributes["prevent-invalid-input"] = "";
                input.Attributes["is-search"] = "";

                if (def.ListObjectName != "")
                {
                    input.Attributes["object-name"] = def.ListObjectName;
                    input.Attributes["list-name"] = def.ListName;
                }

                if (other == null)
                {
                    other = new Dictionary<string, string>(1) { { "value", "0" } };
                }
                else
                {
                    if (!other.ContainsKey("value"))
                    {
                        other.Add("value", "0");
                    }
                }
            }
            else if (def.Type == PropertyInputType.checkbox)
            {
                input = new TagBuilder("paper-checkbox");

                if (def.Label != "")
                {
                    if (def.LabelIsFieldName)
                    {
                        input.InnerHtml.SetContent("[[item.data." + def.Label + "]]");
                    }
                    else
                    {
                        input.InnerHtml.SetContent(def.Label);
                    }
                }

                AddLinkImport(page, "/lib/paper-checkbox/paper-checkbox.html");
            }
            else if (def.Type == PropertyInputType.radio)
            {
                input = new TagBuilder("input");
                input.Attributes["type"] = def.Type.ToString();
            }
            else if ((def.Type == PropertyInputType.date || def.Type == PropertyInputType.datetimeHHmm || def.Type == PropertyInputType.datetimeHHmmss)
                && !def.IsReadOnly)
            {
                input = new TagBuilder("awsl-date");

                AddLinkImport(page, "/AWSLib/awsl-date");
            }
            else
            {
                if (def.Type == PropertyInputType.textarea)
                {
                    input = new TagBuilder("paper-textarea");
                    AddLinkImport(page, "/lib/paper-input/paper-textarea.html");
                }
                else if (def.Type == PropertyInputType.email)
                {
                    input = new TagBuilder("gold-email-input");
                    AddLinkImport(page, "/lib/gold-email-input/gold-email-input.html");
                }
                else
                {
                    input = new TagBuilder("paper-input");
                    AddLinkImport(page, "/lib/paper-input/paper-input.html");

                    if (def.Type == PropertyInputType.password)
                    {
                        input.Attributes["type"] = "password";
                    }
                }

                if (def.MaxLength > 0)
                {
                    input.Attributes["maxlength"] = def.MaxLength.ToString();
                }
            }

            input.Attributes["data-field"] = def.FieldName;

            if (other != null)
            {
                foreach (string key in other.Keys)
                {
                    input.Attributes[key] = other[key];
                }
            }

            if (def.Label != "" && !NoLabel && def.Type != PropertyInputType.checkbox)
            {
                if (def.AlwaysFloatLabel)
                {
                    input.Attributes["always-float-label"] = "";
                }
                if (def.LabelIsFieldName)
                {
                    input.Attributes["label"] = "[[" + def.Label + "]]";
                }
                else
                {
                    input.Attributes["label"] = def.Label;
                }
            }
            else
            {
                input.Attributes["no-label-float"] = "";
            }

            return input;
        }
        public static IHtmlContent JavascriptVaadinGridColumns(this IHtmlHelper htmlHelper, string gridID, DataView dataView)
        {
            List<DataViewColumn> columns = dataView.VisibleColumns;
            StringBuilder script = new StringBuilder(20 * columns.Count);
            int firstOrder = -1;
            int index = 0;
            bool firstIsHidden = false;
            
            foreach (DataViewColumn col in columns)
            {
                script.AppendLine(gridID + ".addColumn({");
                script.AppendLine("name: '" + col.Label + "',");
                if (col.Hidden)
                {
                    if (index == 0)
                    {
                        firstIsHidden = true;
                    }
                    else
                    {
                        script.AppendLine("hidden: true,");
                    }
                }
                if (col.Hidable)
                {
                    script.AppendLine("hidable: true,");
                }
                if (col.MinWidth != 0)
                {
                    script.AppendLine("minWidth: " + col.MinWidth + ",");
                }
                if (col.MaxWidth != 0)
                {
                    script.AppendLine("maxWidth: " + col.MaxWidth + ",");
                }
                if (col.Width != 0)
                {
                    script.AppendLine("width: " + col.Width + ",");
                }
                if (col.Flex != 0)
                {
                    script.AppendLine("flex: " + col.Flex + ",");
                }

                if (col.OrderBy != "")
                {
                    script.AppendLine("sortable: true,");
                    if (!col.Hidden)
                    {
                        if (firstOrder == -1)
                        {
                            script.AppendLine("sortDirection:'asc',");
                            firstOrder = index;
                        }
                    }
                }

                if (col.CustomRenderer == "")
                {
                    string alignRenderer = "";
                    string dataRenderer = "";

                    if (col.Align != HorizontalAlign.Left && col.Align != HorizontalAlign.NotSet)
                    {
                        alignRenderer = @"""<span style='width:100%; text-align:" + col.Align.ToString().ToLower() + @"; overflow: hidden; text-overflow: ellipsis;'>"" + ";
                    }

                    if (col.BasicType == BasicType.Number || col.BasicType == BasicType.DateTime || col.Money)
                    {
                        if (col.BasicType == BasicType.DateTime)
                        {
                            dataRenderer = "moment.Util.dateFormat.format(new Date(cell.data))";
                        }
                        else
                        {
                            dataRenderer = "moment.Util." + (col.Money ? "moneyFormat" : "numberFormat") + ".format(cell.data)";
                        }

                        dataRenderer = "(cell.data == null ? '' : " + dataRenderer + ")";
                    }

                    if (alignRenderer != "" || dataRenderer != "")
                    {
                        if (dataRenderer == "")
                        {
                            dataRenderer = "cell.data";
                        }

                        script.AppendLine("renderer:function(cell) {");
                        if (alignRenderer != "")
                        {
                            script.Append("cell.element.innerHTML = " + alignRenderer + dataRenderer + @" + ""</span>"";");
                        }
                        else
                        {
                            script.Append("cell.element.innerHTML = " + dataRenderer + ";");
                        }
                        script.AppendLine("},");
                    }
                }
                else
                {
                    script.AppendLine("renderer:function(cell) { " + col.CustomRenderer);
                    script.AppendLine("},");
                }

                script.AppendLine("});");

                index++;
            }

            script.AppendLine(gridID + ".sortOrder = [{column: " + firstOrder + ", direction: 'asc'}];");
            if (firstIsHidden)
            {
                script.AppendLine(gridID + ".columns[0].hidden = true;");
            }

            return new HtmlString(script.ToString());
        }
    }
}
