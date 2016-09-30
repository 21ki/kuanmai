using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.Beans;
using KMBit.BL.MobileLocator;
using KMBit.Util;
using WeChat.Adapter;
namespace KMBit.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            charge();
            //products();
        }
        static void products()
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            parameters["Token"] = "19291b66e20365ff00e85e4e960fe0d4";
            string sectoken = "a3d1ece2-bdf0-4889-92fc-db46ba9878d7";
            string query = "Token=19291b66e20365ff00e85e4e960fe0d4";
            string sign = UrlSignUtil.GetMD5(query + "&key=" + sectoken);
            string url = "http://localhost:8000/km/Account/";
            NameValueCollection col = new NameValueCollection();
            foreach (KeyValuePair<string, string> p in parameters)
            {
                col.Add(p.Key, p.Value);
            }
            col.Add("Sign", sign);
            string res = WeChat.Adapter.Requests.HttpSercice.PostHttpRequest(url + "Products", col, WeChat.Adapter.Requests.RequestType.POST, null);
            Console.WriteLine(res);
        }
        static void charge()
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            parameters["Token"] = "19291b66e20365ff00e85e4e960fe0d4";
            parameters["Id"] = "49";
            //parameters["Id"] = "48";
            parameters["Province"] = "河南";
            parameters["City"] = "许昌";
            parameters["Mobile"] = "15603888388";
            parameters["MobileSP"] = "中国联通";
            //parameters["Client_order_id"] = "102365";
            parameters["CallBackUrl"] = "http://localhost:8000/";
            string sectoken = "a3d1ece2-bdf0-4889-92fc-db46ba9878d7";

            StringBuilder query = new StringBuilder();
            foreach (KeyValuePair<string, string> p in parameters)
            {
                if (query.ToString() == "")
                {
                    query.Append(p.Key);
                    query.Append("=");
                    query.Append(p.Value);
                }
                else
                {
                    query.Append("&");
                    query.Append(p.Key);
                    query.Append("=");
                    query.Append(p.Value != null ? p.Value : "");
                }
            }

            string sign = UrlSignUtil.GetMD5(query.ToString() + "&key=" + sectoken);
            string url = "http://localhost:8000/km/Account/";
            NameValueCollection col = new NameValueCollection();
            foreach (KeyValuePair<string, string> p in parameters)
            {
                col.Add(p.Key, p.Value);
            }
            col.Add("Sign", sign);
            string res = WeChat.Adapter.Requests.HttpSercice.PostHttpRequest(url + "Charge", col, WeChat.Adapter.Requests.RequestType.POST, null);
        }
    }
}
