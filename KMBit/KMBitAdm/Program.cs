using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBitAdm
{
    class Program
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            logger.Info("INFO");
            logger.Error("ERROR");
        }
    }
}
