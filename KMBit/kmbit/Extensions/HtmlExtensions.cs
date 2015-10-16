using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace KMBit.Extensions
{
    public static class HtmlExtnsions
    {
        public static MvcHtmlString CheckBoxListTaocanFor<TModel, TProperty>(
            this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> expression,
            IList<KMBit.Beans.BResourceTaocan> selectList,
            object htmlAttributes
        )
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            string fullHtmlFieldName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            var values = GetModelStateValue(htmlHelper.ViewData, fullHtmlFieldName, typeof(string[]));
            if (values == null)
            {
                values = htmlHelper.ViewData.Eval(fullHtmlFieldName);
            }
            List<string> selctedValues = null;
            if (values != null)
            {
                var collection =
                    from object value in values as System.Collections.IEnumerable
                    select Convert.ToString(value, System.Globalization.CultureInfo.CurrentCulture);
                var hashSet = new HashSet<string>(collection, StringComparer.OrdinalIgnoreCase);
                selctedValues = hashSet.ToList<string>();
            }

            var sb = new System.Text.StringBuilder("<ul style=\"padding-left:0px;\">");
            if(selectList!=null && selectList.Count>0)
            {
                sb.Append("<li style=\"overflow:hidden;display:block;\">");
                sb.Append("<div style=\"float:left;width:5%;\"><input type=\"checkbox\" value=\"\" id=\"chkAll"+ fullHtmlFieldName + "\" name=\"chkAll" + fullHtmlFieldName + "\"/></div>");
                sb.Append("<div style=\"float:left;width:20%;\">套餐名称</div>");
                sb.Append("<div style=\"float:left;width:10%;\">运营商</div>");
                sb.Append("<div style=\"float:left;width:10%;\">归属地</div>");
                sb.Append("<div style=\"float:left;width:10%;\">流量大小</div>");
                sb.Append("<div style=\"float:left;width:10%;\">零售价</div>");
                sb.Append("</li>");
                foreach (var item in selectList)
                {
                    var li = new System.Text.StringBuilder("<li style=\"overflow:hidden;display:block;\">");
                    var checkbox = new TagBuilder("input");
                    checkbox.Attributes["type"] = "checkbox";
                    checkbox.Attributes["name"] = fullHtmlFieldName;
                    checkbox.Attributes["value"] = item.Taocan.Id.ToString();
                    checkbox.GenerateId(fullHtmlFieldName);
                    if(selctedValues != null)
                    {
                        string chk = (from s in selctedValues where s == item.Taocan.Id.ToString() select s).FirstOrDefault<string>();
                        if(chk!=null)
                        {
                            checkbox.Attributes["checked"] = "checked";
                        }
                    }

                    li.Append("<div style=\"float:left;width:5%;\">" + checkbox.ToString(TagRenderMode.SelfClosing) + "</div>");
                    li.Append("<div style=\"float:left;width:20%;\">" + item.Taocan2.Name + "</div>");
                    li.Append("<div style=\"float:left;width:10%;\">" + item.SP!=null?item.SP.Name:"全网" + "</div>");
                    li.Append("<div style=\"float:left;width:10%;\">" + item.Province!=null?item.Province.Name:"全国" + "</div>");
                    li.Append("<div style=\"float:left;width:10%\">" + item.Taocan.Quantity + "M</div>");
                    li.Append("<div style=\"float:left;width:10%\">" + item.Taocan.Sale_price + "元</div>");
                    li.Append("</li>");
                    sb.Append(li.ToString());
                }
            }
            
            sb.Append("</ul>");

            return new MvcHtmlString(sb.ToString());
        }

        private static object GetModelStateValue(System.Web.Mvc.ViewDataDictionary viewData, string key, Type destinationType)
        {
            ModelState modelState;
            if (viewData.ModelState.TryGetValue(key, out modelState) && modelState.Value != null)
            {
                return modelState.Value.ConvertTo(destinationType, null);
            }
            return null;
        }
    }
}