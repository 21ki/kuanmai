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
    public class CommonManager
    {
        public CommonManager()
        {
        }

        public List<Mall_Type> GetMallTypes()
        {
            List<Mall_Type> types = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                types = (from mtype in db.Mall_Type select mtype).ToList<Mall_Type>();
            }

            return types;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent_id"></param>
        /// <returns></returns>
        public List<Common_District> GetAreas(int parent_id)
        {
            List<Common_District> areas = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var a=(from ass in db.Common_District where ass.upid==parent_id select ass);
              
                areas = a.ToList<Common_District>();
            }
            return areas;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Express> GetExpresses()
        {
            List<Express> expresses = new List<Express>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                expresses=(from express in db.Express orderby express.Express_ID ascending select express).ToList<Express>();
            }
            return expresses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Corp_Info GetCorpInfo()
        {
            Corp_Info info = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                info=(from ci in db.Corp_Info where ci.IsCurrent==true select ci).FirstOrDefault<Corp_Info>();
            }
            return info;
        }
    }
}
