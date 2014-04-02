using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class SystemManager
    {
        public SystemManager()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Open_Key> GetOpenKeys()
        {
            List<Open_Key> keys = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                keys = (from key in db.Open_Key select key).ToList<Open_Key>();
            }

            return keys;
        }
    }
}
