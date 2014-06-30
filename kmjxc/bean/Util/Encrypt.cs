using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
namespace KM.JXC.Common.Util
{
    public class Encrypt
    {
        public static string MD5(string resource)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string a = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(resource)));
            a = a.Replace("-", "");
            return a;
        }
    }
}
