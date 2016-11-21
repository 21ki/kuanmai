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
        static string url = "http://yfll.eruifo.com/km/Account/";
        static void Main(string[] args)
        {
            charge();
            //products();
            //string x = "CallBackUrl=http://113.31.21.238:8200/flux/callbackservice/yifengcb.ws&City=商丘&Client_order_id=68919_0&Id=20&Mobile=15649939049&Province=河南&Token=0ce9445505baa521ed1a2b0a34853164&key=0a483117-4e4d-4d97-aad4-d6576c2ffdec";
            //string sign = Md5(x);
        }

        static string Md5(string s)
        {
            return UrlSignUtil.GetMD5(s);
        }

        static void products()
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            parameters["Token"] = "0ce9445505baa521ed1a2b0a34853164";
            string sectoken = "0a483117-4e4d-4d97-aad4-d6576c2ffdec";
            string query = "Token=0ce9445505baa521ed1a2b0a34853164";
            string sign = UrlSignUtil.GetMD5(query + "&key=" + sectoken);
            
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
            parameters["Token"] = "0ce9445505baa521ed1a2b0a34853164";
            parameters["Id"] = "57";
            //parameters["Id"] = "48";
            parameters["Province"] = "河南";
            parameters["City"] = "商丘";
            parameters["Mobile"] = "15649939049";
            parameters["MobileSP"] = "中国联通";
            //parameters["Client_order_id"] = "102365";
            parameters["CallBackUrl"] = "http://113.31.21.238:8200/flux/callbackservice/yifengcb.ws";
            string sectoken = "0a483117-4e4d-4d97-aad4-d6576c2ffdec";

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
            string urlStr = query.ToString() + "&key=" + sectoken;
            string sign = UrlSignUtil.GetMD5(urlStr);
           
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
