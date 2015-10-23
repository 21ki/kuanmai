using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using KMBit.Beans;
namespace KMBit.BL.PayAPI.AliPay
{
    public class AlipayConfig
    {
        public string Partner { get; private set; }              //合作身份者ID
        public string Key { get; private set; }                  //商户的私钥
        public string Input_charset { get; private set; }         //编码格式
        public string Sign_Type { get; private set; }          //签名方式
        public string Gate_Url { get; private set; }
        public string Verify_Url { get; private set; }
        public string Notify_Url { get; private set; }
        public string Return_Url { get; private set; }
        public string Email { get; private set; }
        public AlipayConfig(string configFilePath)
        {
            Initialize(configFilePath);
        }

        private void Initialize(string configFilePath)
        {
            if(string.IsNullOrEmpty(configFilePath))
            {
                throw new KMBitException("Alipay config file must be provided");
            }

            if(!File.Exists(configFilePath))
            {
                throw new KMBitException(string.Format("Alipay config file:{0} doesn't exit", configFilePath));
            }
           
            XmlDocument dom = null;
            try
            {
                dom = new XmlDocument();
                dom.Load(configFilePath);
                XmlNode partner = dom.SelectSingleNode("/AliPayConfig/Partner");
                if(partner!=null && partner.InnerText!=null)
                {
                    this.Partner = partner.InnerText;
                }
                XmlNode Key = dom.SelectSingleNode("/AliPayConfig/Key");
                if (Key != null && Key.InnerText != null)
                {
                    this.Key= Key.InnerText;
                }
                XmlNode input_charset = dom.SelectSingleNode("/AliPayConfig/Input_charset");
                if (input_charset != null && input_charset.InnerText != null)
                {
                    this.Input_charset = input_charset.InnerText;
                }
                XmlNode sign_type = dom.SelectSingleNode("/AliPayConfig/Sign_type");
                if (sign_type != null && sign_type.InnerText != null)
                {
                    this.Sign_Type = sign_type.InnerText;
                }
                XmlNode gateUrl = dom.SelectSingleNode("/AliPayConfig/GateUrl");
                if (gateUrl != null && gateUrl.InnerText != null)
                {
                    this.Gate_Url = gateUrl.InnerText;
                }
                XmlNode verify_url = dom.SelectSingleNode("/AliPayConfig/VerifyUrl");
                if (verify_url != null && verify_url.InnerText != null)
                {
                    this.Verify_Url = verify_url.InnerText;
                }

                XmlNode notifyUrl = dom.SelectSingleNode("/AliPayConfig/NotifyUrl");
                if (notifyUrl != null && notifyUrl.InnerText != null)
                {
                    this.Notify_Url = notifyUrl.InnerText;
                }
                XmlNode returnUrl = dom.SelectSingleNode("/AliPayConfig/ReturnUrl");
                if (returnUrl != null && returnUrl.InnerText != null)
                {
                    this.Return_Url = returnUrl.InnerText;
                }
                XmlNode email = dom.SelectSingleNode("/AliPayConfig/Email");
                if (email != null && email.InnerText != null)
                {
                    this.Email = email.InnerText;
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                if(dom!=null)
                {
                    dom = null;
                }
            }
        }
    }
}
