using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.Beans;
using KMBit.BL.MobileLocator;
using KMBit.Util;

namespace KMBit.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SortedDictionary<string, string> parameters = new SortedDictionary<string, string>();
            parameters["Token"] = "d89d32ab-f96d-4b29-be6f-8fb3f7a3cd6c";
            parameters["Id"] = "44";
            parameters["Province"] = "上海";
            parameters["City"] = "上海";
            parameters["Mobile"] = "13636651285";
            parameters["CallBackUrl"] = "http//:www.kxcall.com";
            string sectoken = "d89d32ab-f96d-4b29-be6f-8fb3f7a3cd6c";

            StringBuilder query = new StringBuilder();
            foreach(KeyValuePair<string,string> p in parameters)
            {
                if (query.ToString() == "") {
                    query.Append(p.Key);
                    query.Append("=");
                    query.Append(p.Value);
                }
                else
                {
                    query.Append("&");
                    query.Append(p.Key);
                    query.Append("=");
                    query.Append(p.Value);
                }
            }

            string sign = UrlSignUtil.GetMD5(query.ToString()+"&key=" + sectoken);
            
            
            
        }
    }
}
