using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
namespace WeChat.Adapter
{
    public class WeChatLogger
    {
        static ILog Logger = null;
        static WeChatLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger = log4net.LogManager.GetLogger("WeChat.Adapter");
        }

        public static ILog GetLogger()
        {
            return Logger;
        }
    }
}
