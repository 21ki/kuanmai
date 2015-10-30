using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace KMBit.Beans
{
    public class AppSettings
    {
        public string WebURL { get; set; }
        public string RootDirectory { get; set; }
        public string QRFolder { get; set; }

        private static AppSettings settings = null;
        private AppSettings()
        {

        }

        public static AppSettings GetAppSettings()
        {
            if(settings==null)
            {
                InitializeSettings();
            }
            return settings;
        }

        private static void InitializeSettings()
        {
            settings = new AppSettings();
            settings.WebURL = ConfigurationSettings.AppSettings["wwwurl"];
            settings.RootDirectory = ConfigurationSettings.AppSettings["wwwroot"];
            settings.QRFolder = ConfigurationSettings.AppSettings["qrfolder"];
        }
    }
}
