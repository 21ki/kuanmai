using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class WeiChatBaseMessage
    {
        public string Content { get; set; }        
        public long CreateTime { get; set; }
        public string MsgType { get; set; }
    }

    public class WeiChatContentMessage:WeiChatBaseMessage
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public WeiChatContentMessage()
        {
            MsgType = "text";
        }
    }

    public class WeiChatReceivedContentMessage : WeiChatBaseMessage
    {
        public string MsgId { get; set; }
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public WeiChatReceivedContentMessage()
        {
            MsgType = "text";
        }
    }

    public class WeiChatNewsMessage : WeiChatBaseMessage
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public WeiChatNewsMessage()
        {
            MsgType = "news";
        }

        public int ArticleCount { get; set; }
        public List<WeiChatArticle> Articles { get; set; }
    }

    public class WeiChatArticle
    {
        public string PicUrl { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }
}
