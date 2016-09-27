using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL;
using KMBit.Util;
using log4net;
namespace KMBit.Controllers.api
{
    public class SaoMaController : BaseApiController
    {
        ILog logger = null;
        private string weixinToken = "kuanmaicode2015";
        public SaoMaController()
        {
            logger = log4net.LogManager.GetLogger(this.GetType());
        }
        public ApiMessage GetCodeIndirect()
        {
            ApiMessage message = new ApiMessage();
            try
            {
                this.IniRequest();
                ActivityManagement activityMgr = new ActivityManagement(0);
                string spName = string.Empty;
                string openId = string.Empty;
                int agentId = 0;
                int customerId = 0;
                int activityId = 0;
                spName = request["sp"];
                string url = activityMgr.GetOneRandomMarketOrderQrCodeUrl(spName, openId, agentId, customerId, activityId);    
            }catch(KMBitException kex)
            {
                message.Message = kex.Message;
                message.Status = "ERROR";
            }
            return message;
        }
        //just for weichat public account
        [AcceptVerbs("post","get")]
        public HttpResponseMessage GetCodeDirect()
        {
            this.IniRequest();
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            ApiMessage message = new ApiMessage();
            string openId = string.Empty;
            string openPublic = string.Empty;
            string spName = string.Empty;
            string signature = request["signature"];
            string once= request["nonce"];
            string timestamp = request["timestamp"];
            string echostr = request["echostr"];
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(once) || string.IsNullOrEmpty(timestamp))
            {
                logger.Info("The request was not sent from weixin");
                resp.Content = new StringContent("false", System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            logger.Info(string.Format("signature:{0},nonce:{1},timestamp:{2},echostr:{3}",signature,once,timestamp,echostr));
            SortedSet<string> paras = new SortedSet<string>();
            paras.Add(weixinToken);
            paras.Add(once);
            paras.Add(timestamp);
            string strKey = "";
            logger.Info(string.Format("signature posted by weixin is {0}", signature));
            foreach (string v in paras)
            {
                strKey += v;
            }
            string sign = KMEncoder.SHA1_Hash(strKey);
            logger.Info(string.Format("signature calculated in platform is {0}",sign));
            if(signature!=sign)
            {
                logger.Info("two signatures are different, The request was not sent from weixin");
                resp.Content = new StringContent("false", System.Text.Encoding.UTF8, "text/plain");
                return resp;               
            }
            //For weixin URL verification
            if(!string.IsNullOrEmpty(echostr))
            {
                resp.Content = new StringContent(echostr, System.Text.Encoding.UTF8, "text/plain");
                return resp;
            }
            try
            {               
                ActivityManagement activityMgr = new ActivityManagement(0);
                int agentId = 0;
                int customerId = 0;
                int activityId = 0;
                int.TryParse(request["agentId"], out agentId);
                int.TryParse(request["customerId"], out customerId);
                int.TryParse(request["activityId"], out activityId);
                System.IO.Stream sream = context.Request.InputStream;
                StreamReader sr = new StreamReader(sream);
                string strXML = sr.ReadToEnd();
                sream.Close();
                //strXML = "<xml><ToUserName><![CDATA[gh_3f1c1268428a]]></ToUserName><FromUserName><![CDATA[oNEHRsogX4seVvYK5v3S-veFUkEk]]></FromUserName><CreateTime>1446455044</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[+联通]]></Content><MsgId>6212477109514836427</MsgId></xml>";
                logger.Info(string.Format("Message pushed by Weichat server:{0}", strXML));
                WeiChatReceivedContentMessage weiChatMessage = WeiChatMessageUtil.ParseWeichatXML(strXML,logger);
                if (weiChatMessage != null)
                {
                    if(weiChatMessage.Content!= null)
                    {
                        spName = weiChatMessage.Content.Trim();
                    }                    
                    if(weiChatMessage.FromUserName!=null)
                    {
                        openId = weiChatMessage.FromUserName;
                    }                    
                    if(weiChatMessage.ToUserName!=null)
                    {
                        openPublic = weiChatMessage.ToUserName;
                    }                    
                }
                if(string.IsNullOrEmpty(spName))
                {
                    resp.Content = new StringContent("success", System.Text.Encoding.UTF8, "text/plain");
                    return resp;
                }
                if (!spName.Contains("+联通") && !spName.Contains("+移动") && !spName.Contains("+电信"))
                {
                    resp.Content = new StringContent("success", System.Text.Encoding.UTF8, "text/plain");
                    return resp;
                }
                string url = activityMgr.GetOneRandomMarketOrderQrCodeUrl(spName, openId, agentId, customerId, activityId);
                message.Item = url;
                message.Status = "OK";
                message.Message = "成功获取到二维码";
                logger.Info(url);
            }
            catch (KMBitException kex)
            {
                logger.Warn(kex);
                message.Message = kex.Message;
                message.Status = "ERROR";
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                message.Message = "未知错误";
                message.Status = "ERROR";
            }

            logger.Info(string.Format("GetOneRandomMarketOrderQrCodeUrl Status:{0}, Message:{1}",message.Status,message.Message));
            string returnXml = "";
            if (message.Status == "OK")
            {
                logger.Info("二维码获取成功，准备推送二维码到微信");
                WeiChatNewsMessage news = new WeiChatNewsMessage();
                news.ToUserName = openId;
                news.FromUserName = openPublic;
                news.MsgType = "news";
                List<WeiChatArticle> articles = new List<WeiChatArticle>();
                news.Articles = articles;
                WeiChatArticle article = new WeiChatArticle()
                {
                    Description = "",
                    Title = "将二维码保存到相册,微信->发现->扫一扫->相册，选择刚刚保存的二维码",
                    PicUrl = message.Item.ToString(),
                    Url = message.Item.ToString()
                };
                articles.Add(article);
                returnXml = WeiChatMessageUtil.PrepareWeiChatNewsXml(news);
            }
            else
            {
                logger.Info("二维码获取失败");
                WeiChatContentMessage msg = new WeiChatContentMessage()
                {
                    Content = message.Message,
                    CreateTime = 0,
                    FromUserName = openPublic,
                    MsgType = "text",
                    ToUserName = openId
                };
                returnXml = WeiChatMessageUtil.PrepareWeiChatXml(msg);
            }
            //logger.Info(returnXml);
            resp.Content = new StringContent(returnXml, System.Text.Encoding.UTF8, "text/plain");
            return resp;
        }
    }
}
