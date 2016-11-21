using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WeChat.Adapter;
namespace WeChat.Adapter.Requests
{
    public enum RequestType
    {
        GET,
        POST
    }
    public class HttpSercice
    {
        public static string getXMLInput(NameValueCollection col,Encoding coder)
        {
            if(col==null || col.Count==0)
            {
                return "<xml></xml>";
            }
            StringBuilder cBuilder = new StringBuilder();
            cBuilder.Append("<xml>");
            foreach (String s in col.AllKeys)
            {
                cBuilder.Append("<" + s + ">");
                cBuilder.Append(col[s]);
                cBuilder.Append("</" + s + ">");
            }
            cBuilder.Append("</xml>");
            string content = cBuilder.ToString();
            byte[] byteArray = Encoding.UTF8.GetBytes(content);
            string return_string = coder.GetString(byteArray);
            return return_string;
        }
        public static string PostXmlToUrl(string url, NameValueCollection col, RequestType type)
        {
            string returnmsg = "";
            string inputXML = getXMLInput(col, Encoding.UTF8);
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                returnmsg = wc.UploadString(url, type.ToString(), inputXML);
            }
            return returnmsg;
        }
        public static string PostHttpRequest(string url, NameValueCollection col, RequestType type, string contentType)
        {
            WeChatLogger.GetLogger().Info("PostHttpRequest.........");
            string output = null;
            StreamReader rs = null;
            try
            {
                string json_str = string.Empty;

                var client = new HttpClient();
                if (!string.IsNullOrEmpty(contentType))
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                }

                if (type == RequestType.POST)
                {
                    HttpContent content = null;
                    var postData = new List<KeyValuePair<string, string>>();
                    if (col != null && col.Count > 0)
                    {
                        IEnumerator myEnumerator = col.GetEnumerator();                       
                        if (contentType != null && contentType.ToLower() == "text/xml")
                        {
                            WeChatLogger.GetLogger().Info("post xml data...");
                            StringBuilder cBuilder = new StringBuilder();
                            cBuilder.Append("<xml>");
                            foreach (String s in col.AllKeys)
                            {
                                cBuilder.Append("<" + s + ">");
                                cBuilder.Append(col[s]);
                                cBuilder.Append("</" + s + ">");
                            }
                            cBuilder.Append("</xml>");
                            WeChatLogger.GetLogger().Info(cBuilder.ToString());
                            content = new StringContent(cBuilder.ToString());
                           
                        }else
                        {
                            foreach (String s in col.AllKeys)
                            {
                                if(s!=null && col[s]!=null)
                                {
                                    postData.Add(new KeyValuePair<string, string>(s, col[s]));
                                }                                
                            }
                            
                            content = new FormUrlEncodedContent(postData);
                        }                        
                    }
                    
                    var response = client.PostAsync(url, content).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    Stream res = response.Content.ReadAsStreamAsync().Result;
                    //res = response.Content.ReadAsStreamAsync().Result;
                    if (contentType != "multipart/form-data")
                    {
                        rs = new StreamReader(res);
                        output = rs.ReadToEnd();
                    }
                }
                else if (type == RequestType.GET)
                {
                    StringBuilder urlParms = new StringBuilder();
                    if (col != null)
                    {
                        IEnumerator myEnumerator = col.GetEnumerator();
                        int count = 1;
                        foreach (String s in col.AllKeys)
                        {
                            urlParms.Append(s);
                            urlParms.Append("=");
                            urlParms.Append(col[s]);
                            if (count < (col.Count))
                            {
                                urlParms.Append("&");
                            }

                            count++;
                        }
                    }
                    string getUrl = url;
                    if (!string.IsNullOrEmpty(urlParms.ToString()))
                    {
                        getUrl += "?" + urlParms.ToString();
                    }
                    var response = client.GetAsync(getUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Stream res = response.Content.ReadAsStreamAsync().Result;
                        rs = new StreamReader(res);
                        output = rs.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                WeChatLogger.GetLogger().Error(ex);
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
            WeChatLogger.GetLogger().Info("Post request done.");
            return output;
        }
    }
}
