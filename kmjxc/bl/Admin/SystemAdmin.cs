using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.Common.Util;
using KM.JXC.BL.Admin;
using KM.JXC.DBA;
using KM.JXC.Common.KMException;

namespace KM.JXC.BL.Admin
{
    public class SystemAdmin
    {
        public void Login(int user_id,string password)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                User user=(from u in db.User where u.IsSystemUser==true && u.User_ID==user_id select u).FirstOrDefault<User>();
                if (user == null)
                {
                    throw new KMJXCException("用户名不存在");
                }

                string md5Password = Encrypt.MD5(password);

                if (md5Password != user.Password)
                {
                    throw new KMJXCException("密码不正确");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void SetCorpInfo(Corp_Info info)
        {

        }
    }
}
