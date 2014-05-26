using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KM.JXC.Web.Utilities
{
    public class JXCPager
    {
        public static String Pager(KM.JXC.BL.Models.BPageData data)
        {
            System.Text.StringBuilder pageHtml = new System.Text.StringBuilder();

            long total = data.TotalRecords;
            long totalPage = 0;
            int curPage = data.Page;
            int pageSize = data.PageSize;
            string url=data.URL;

            if(url.IndexOf("page=")>-1)
            {
                string s = url.Substring(0, url.IndexOf("page="));
                if (s.EndsWith("&"))
                {
                    s = s.Substring(0,s.Length-1);
                }
                string e = url.Substring(url.IndexOf("page=")+6);
                url = s;
                if (e.IndexOf("&") > -1)
                {
                    url += e.Substring(e.IndexOf("&"));
                }
            }

            if (url.IndexOf('?') < 0)
            {
                url += "?";
            }
            else 
            {
                if (!url.EndsWith("?"))
                {
                    url += "&";
                }
            }
            //No need to page

            if (total <= pageSize)
            {
                return null;
            }

            if (total % pageSize == 0)
            {
                totalPage = total / pageSize;
            }
            else
            {
                totalPage = (total / pageSize)+1;
            }

            pageHtml.Append("<a class=\"pn\" href=\""+url+"page=1\">首页</a>");

            if (curPage > 1)
            {
                pageHtml.Append("<a class=\"pn\" href=\"" + url + "page=" + (curPage - 1) + "\">上一页</a>");
            }

            for (long i = 1; i <= totalPage; i++)
            {
              

                if (curPage != i)
                {
                    pageHtml.Append("<a class=\"pn\" href=\"" + url + "page=" + i.ToString() + "\">" + i.ToString() + "</a>");
                }
                else
                {
                    pageHtml.Append("<span class=\"spn\">" + i.ToString() + "</span>");
                }               
            }
            if (curPage < totalPage)
            {
                pageHtml.Append("<a class=\"pn\" href=\"" + url + "page=" + (curPage + 1) + "\">下一页</a>");
            }
            pageHtml.Append("<a class=\"pn\" href=\"" + url + "page=" + totalPage.ToString() + "\">末页</a>");
            pageHtml.Append("<span class=\"spn\">总记录:</span>"+total);
            return pageHtml.ToString();
        }
    }
}