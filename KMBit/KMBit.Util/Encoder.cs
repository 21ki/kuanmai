using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace KMBit.Util
{
    public static class KMEncoder
    {
        public static string publicKey = "kuanmaifsd5234sadfasdf24567675675475653454356sdfsdfasdf235435436546754768789797898078908970890";  
        static KMEncoder()
        {
            publicKey = publicKey.Substring(0,8);
        }

        public static string SHA1_Hash(string str_sha1_in)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = UTF8Encoding.Default.GetBytes(str_sha1_in);
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            string str_sha1_out = BitConverter.ToString(bytes_sha1_out);
            str_sha1_out = str_sha1_out.Replace("-", "").ToLower();
            return str_sha1_out;
        }

        public static string Encode(string plaintext)
        {
            byte[] data = Encoding.UTF8.GetBytes(plaintext);
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                try
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(publicKey);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(publicKey);
                    ICryptoTransform desencrypt = des.CreateEncryptor();
                    byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
                    return BitConverter.ToString(result);
                }
                catch(Exception ex)
                { }
               
            }

            return null;
        }

        public static string Decode(string ciphertext)
        {
            string[] sInput = ciphertext.Split("-".ToCharArray());
            byte[] data = new byte[sInput.Length];
            for (int i = 0; i < sInput.Length; i++)
            {
                data[i] = byte.Parse(sInput[i], System.Globalization.NumberStyles.HexNumber);
            }
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                try
                {
                    des.Key = ASCIIEncoding.ASCII.GetBytes(publicKey);
                    des.IV = ASCIIEncoding.ASCII.GetBytes(publicKey);
                    ICryptoTransform desencrypt = des.CreateDecryptor();
                    byte[] result = desencrypt.TransformFinalBlock(data, 0, data.Length);
                    return Encoding.UTF8.GetString(result);
                }
                catch(Exception ex)
                { }                
            }

            return null;
        }
    }
}
