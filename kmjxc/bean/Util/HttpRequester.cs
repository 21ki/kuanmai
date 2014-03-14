using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections;
using System.Collections.Specialized;
using System.IO;

namespace KM.JXC.Common.Util
{  
    public class HttpRequester
    {  

        public HttpRequester()
        {
            
        }

        public static string PostHttpRequest(string url, NameValueCollection col)
        {
            string output = null;
            StreamReader rs = null;
           
            try
            {
                string json_str = string.Empty;

                var client = new HttpClient();

                var postData = new List<KeyValuePair<string, string>>();
                if (col != null && col.Count > 0)
                {
                    IEnumerator myEnumerator = col.GetEnumerator();
                    foreach (String s in col.AllKeys)
                    {
                        postData.Add(new KeyValuePair<string, string>(s, col[s]));
                    }
                }

                HttpContent content = new FormUrlEncodedContent(postData);

                var response = client.PostAsync(url, content).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                Stream res = response.Content.ReadAsStreamAsync().Result;
                rs = new StreamReader(res);
                output = rs.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            finally
            {
                if (rs != null)
                {
                    rs.Close();
                }
            }

            return output;
        }
    }
}
