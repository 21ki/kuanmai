using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using KMBit.Beans;
using log4net;
namespace KMBit.Util
{
    public class WeiChatMessageUtil
    {
        public static WeiChatReceivedContentMessage ParseWeichatXML(string xml,ILog logger=null)
        {
            WeiChatReceivedContentMessage message = new WeiChatReceivedContentMessage();
            try
            {
                logger.Info("ParseWeichatXML");
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);
                XmlNode contentNode = xmlDoc.SelectSingleNode("/xml/Content");
                if(contentNode!=null && !string.IsNullOrEmpty(contentNode.InnerText))
                {
                    message.Content = contentNode.InnerText.Trim();
                }
                XmlNode toUserNode = xmlDoc.SelectSingleNode("/xml/ToUserName");
                if (toUserNode != null && !string.IsNullOrEmpty(toUserNode.InnerText))
                {
                    message.ToUserName = toUserNode.InnerText.Trim();                    
                }

                XmlNode msgType = xmlDoc.SelectSingleNode("/xml/MsgType");
                if (msgType != null && !string.IsNullOrEmpty(msgType.InnerText))
                {
                    message.MsgType = msgType.InnerText.Trim();
                }              

                XmlNode fromUserNode = xmlDoc.SelectSingleNode("/xml/FromUserName");
                if (fromUserNode != null && !string.IsNullOrEmpty(fromUserNode.InnerText))
                {
                    message.FromUserName = fromUserNode.InnerText.Trim();
                }
                XmlNode msgIdNode = xmlDoc.SelectSingleNode("/xml/MsgId");
                if (msgIdNode != null && !string.IsNullOrEmpty(msgIdNode.InnerText))
                {
                    message.MsgId = msgIdNode.InnerText.Trim();
                }
                XmlNode createTimeNode = xmlDoc.SelectSingleNode("/xml/CreateTime");
                if (createTimeNode != null && !string.IsNullOrEmpty(createTimeNode.InnerText))
                {
                    long time = 0;
                    long.TryParse( createTimeNode.InnerText, out time);
                    message.CreateTime = time;
                }
                xmlDoc = null;                
            }
            catch(Exception ex)
            {
                if(logger!=null)
                {
                    logger.Error(ex);
                }
            }finally
            {
                logger.Info("Finished ParseWeichatXML");
            }
            return message;
        }

        public static string PrepareWeiChatNewsXml(WeiChatNewsMessage message)
        {
            StringBuilder xml = new StringBuilder("<xml>");
            if(message!=null)
            {
                xml.Append("<ToUserName>");
                xml.Append(message.ToUserName!=null? message.ToUserName:"");
                xml.Append("</ToUserName>");

                xml.Append("<FromUserName>");
                xml.Append(message.FromUserName!=null? message.FromUserName:"");
                xml.Append("</FromUserName>");

                xml.Append("<CreateTime>");
                xml.Append(DateTimeUtil.ConvertDateTimeToInt(DateTime.Now));
                xml.Append("</CreateTime>");

                xml.Append("<MsgType>");
                xml.Append(message.MsgType);
                xml.Append("</MsgType>");
                                
                if(message.Articles!=null && message.Articles.Count>0)
                {
                    xml.Append("<ArticleCount>");
                    xml.Append(message.Articles != null ? message.Articles.Count : 0);
                    xml.Append("</ArticleCount>");
                    xml.Append("<Articles>");
                    foreach (WeiChatArticle article in message.Articles)
                    {
                        xml.Append("<item>");
                        xml.Append("<Title>");
                        xml.Append(article.Title!=null?article.Title:"");
                        xml.Append("</Title>");
                        xml.Append("<Description>");
                        xml.Append(article.Description != null ? article.Description : "");
                        xml.Append("</Description>");
                        xml.Append("<PicUrl>");
                        xml.Append(article.PicUrl != null ? article.PicUrl : "");
                        xml.Append("</PicUrl>");
                        xml.Append("<Url>");
                        xml.Append(article.Url != null ? article.Url : "");
                        xml.Append("</Url>");
                        xml.Append("</item>");
                    }
                    xml.Append("</Articles>");
                }
            }
            xml.Append("</xml>");
            return xml.ToString();
        }

        public static string PrepareWeiChatXml(WeiChatContentMessage message)
        {
            StringBuilder xml = new StringBuilder("<xml>");
            if (message != null)
            {
                xml.Append("<ToUserName>");
                xml.Append(message.ToUserName != null ? message.ToUserName : "");
                xml.Append("</ToUserName>");

                xml.Append("<FromUserName>");
                xml.Append(message.FromUserName != null ? message.FromUserName : "");
                xml.Append("</FromUserName>");

                xml.Append("<CreateTime>");
                xml.Append(DateTimeUtil.ConvertDateTimeToInt(DateTime.Now));
                xml.Append("</CreateTime>");

                xml.Append("<MsgType>");
                xml.Append(message.MsgType);
                xml.Append("</MsgType>");

                xml.Append("<Content>");
                xml.Append(message.Content!=null?message.Content:"");
                xml.Append("</Content>");

            }
            xml.Append("</xml>");
            return xml.ToString();
        }
    }
}
