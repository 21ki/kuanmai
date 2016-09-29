using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
namespace KMBit.BL
{
    public class KMLogger
    {
        static ILog Logger = null;
        static KMLogger()
        {
            log4net.Config.XmlConfigurator.Configure();
            Logger = log4net.LogManager.GetLogger("KMLogger.");
        }

        public static ILog GetLogger()
        {
            return Logger;
        }
    }
}
