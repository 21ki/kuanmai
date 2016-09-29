using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.BL.Charge;
using log4net;
namespace KMBit.BL
{
    public enum RequestType
    {
        POST,
        GET
    }

    public class HttpService
    {
        public ILog Logger { get; set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Response { get; private set; }

        private string apiUrl = "";
        public string ApiUrl {
            get { return this.ServerUri.AbsoluteUri; }
            protected set { this.apiUrl = value; this.ServerUri = new Uri(this.apiUrl); }
        }
        public Uri ServerUri { get; set; }
        public HttpService(string svrUrl)
        {
            this.ServerUri = new Uri(svrUrl);
            this.apiUrl = svrUrl;
        }
        public HttpService()
        {
        }
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }
        public void SendRequest(List<WebRequestParameters> paras, bool postByByte, out bool succeed, RequestType requestType= RequestType.POST)
        {
            if(string.IsNullOrEmpty(this.ApiUrl))
            {
                throw new Exception("URI 不能为空");
            }
            string returnRes = string.Empty;
            string postData = string.Empty;

            if (paras != null && paras.Count > 0)
            {
                foreach (WebRequestParameters d in paras)
                {
                    string pdata = "";

                    if (!string.IsNullOrEmpty(d.Value))
                    {
                        pdata = d.Value;
                        if (d.URLEncodeParameter)
                        {
                            pdata = System.Web.HttpUtility.UrlEncode(pdata);
                        }
                    }

                    if (string.IsNullOrEmpty(pdata))
                    {
                        pdata = "";
                    }

                    if (postData == string.Empty)
                    {

                        postData = string.Format("{0}={1}", d.Name, pdata);
                    }
                    else
                    {
                        postData += "&" + string.Format("{0}={1}", d.Name, pdata);
                    }
                }
            }
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            succeed = false;
            try
            {
                if(requestType== RequestType.GET)
                {
                    this.ServerUri = new Uri(this.ApiUrl+"?"+ postData);
                }else
                {
                    if(this.ServerUri==null)
                    {
                        this.ServerUri = new Uri(this.ApiUrl);
                    }
                }

                request = (HttpWebRequest)WebRequest.Create(this.ServerUri);
                //request.PreAuthenticate = true;
                //request.Credentials = new NetworkCredential(this.UserName, this.Password);
                //request.AllowAutoRedirect = false;
                request.Accept = "*/*";
                request.KeepAlive = true;
                request.Timeout = 10000 * 6 * 15;
                request.Method = requestType.ToString();
                request.ContentType = "application/x-www-form-urlencoded";
                
                //string cookieheader = string.Empty;
                //CookieContainer cookieCon = new CookieContainer();
                //request.CookieContainer = cookieCon;
                //request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

                //if (cookieheader.Equals(string.Empty))
                //{
                //    cookieheader = request.CookieContainer.GetCookieHeader(this.ServerUri);
                //}
                //else
                //{
                //    request.CookieContainer.SetCookies(this.ServerUri, cookieheader);
                //}

                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                //Post string to the server
                if (!postByByte)
                {
                    if(requestType== RequestType.POST)
                    {
                        request.ContentLength = Encoding.UTF8.GetByteCount(postData);
                        System.IO.StreamWriter sw = new System.IO.StreamWriter(request.GetRequestStream(), new UTF8Encoding(false));
                        sw.Write(postData);
                        sw.Close();
                    }
                                    
                }
                else
                {
                    //Post byte to server
                    Encoding encoding = Encoding.GetEncoding("utf-8");
                    Byte[] bytePost = encoding.GetBytes(postData);
                    request.ContentLength = bytePost.Length;
                    System.IO.Stream stream = request.GetRequestStream();
                    stream.Write(bytePost, 0, bytePost.Length);
                    stream.Close();
                }

                response = (HttpWebResponse)request.GetResponse();
                if (response != null)
                {
                    this.StatusCode = response.StatusCode;
                    Encoding res_encoding = Encoding.GetEncoding(response.CharacterSet);
                    if (response.StatusCode == HttpStatusCode.OK && response.GetResponseStream()!=null)
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                        Response = sr.ReadToEnd();
                        succeed = true;
                        sr.Close();
                    }
                }
                else
                {
                    succeed = true;
                }
            }
            catch (WebException wex)
            {
                HttpStatusCode status = ((HttpWebResponse)wex.Response).StatusCode;
                this.StatusCode = status;
                if (wex.Response != null)
                {
                    response = (HttpWebResponse)wex.Response;
                    if(response!=null && response.GetResponseStream()!=null)
                    {
                        System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                        Response = sr.ReadToEnd();
                        succeed = true;
                        sr.Close();
                    }                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {

            }
        }
    }
}
