using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Specialized;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
namespace KM.JXC.Common.Util
{
    public class HttpWebRequester
    {
        private static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            
            return true;
        }

        public static string PostHttpRequest(string url, NameValueCollection parameters)
        {
            string response = null;

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url cannot be null");
            }


            System.Net.HttpWebRequest request = null;

            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;  
            }

            request.Method = "POST";
            request.UserAgent = DefaultUserAgent;

            if (parameters != null && parameters.Count > 0)
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                foreach (string key in parameters.AllKeys)
                {
                    if (i > 0)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);   
                    }

                    i++;
                }

                
                byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }

            HttpWebResponse res = null;
            System.IO.StreamReader sr = null;
            try
            {
                res = request.GetResponse() as HttpWebResponse;
                sr = new System.IO.StreamReader(res.GetResponseStream(), Encoding.UTF8);
                
                sr.Close();
            }
            catch (WebException wex)
            {
                res = (HttpWebResponse)wex.Response;
                sr = new System.IO.StreamReader(res.GetResponseStream(), Encoding.UTF8);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                response = sr.ReadToEnd();

                if (sr != null)
                {
                    sr.Close();
                }

                if (res != null)
                {
                    res.Close();
                }
            }

            return response;
        }
    }
}
