using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Bean;
namespace KM.JXC.BL
{
    public class UserManager
    {
        public void Login(User user)
        { 
            
        }

        public bool UserExists(User user) {
            bool result = false;

            KuanMaiEntities dba = new KuanMaiEntities();

            var obj = from p in dba.User where p.Mall_Name == user.Mall_Name && p.Mall_Type == user.Mall_Type select p;

            if (obj != null && obj.ToList<User>().Count > 0)
            {
                result = true;
            }

            return result;
        }

        public bool CreateNewUser(User user) {
            bool result = false;

            if (user == null) {
                throw new UserException("用户实体不能为空引用");
            }

            if (string.IsNullOrEmpty(user.Name)) {
                throw new UserException("用户名不能为空");
            }

            try
            {
                KuanMaiEntities dba = new KuanMaiEntities();

                if(this.UserExists(user))
                throw new UserException("用户名已经存在");

                dba.User.Add(user);
                int row = dba.SaveChanges();
                if (row == 1)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
