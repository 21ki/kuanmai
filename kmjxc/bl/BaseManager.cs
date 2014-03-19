using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
namespace KM.JXC.BL
{
    public class BaseManager
    {
        protected User currentUser = null;
        protected Permission currentUserPermission = new Permission();
        protected PermissionManager permissionManager = new PermissionManager();
        public BaseManager()
        {
            
        }

        public BaseManager(User user)
        {
            if (user == null)
            {
                throw new KMJXCException("");
            }
            this.currentUser = user;

            this.GetUserPermission();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetUserPermission()
        {
            currentUserPermission = this.permissionManager.GetUserPermission(this.currentUser);
        }        
    }
}
