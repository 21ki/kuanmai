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
        protected User CurrentUser{get;set;}
        protected int Shop_Id { get; set; }
        protected Permission currentUserPermission = new Permission();
        protected PermissionManager permissionManager = new PermissionManager();


        public BaseManager()
        {
            
        }

        public BaseManager(User user,int shop_id)
        {
            if (user == null)
            {
                throw new KMJXCException("");
            }
            this.CurrentUser = user;
            this.Shop_Id = shop_id;
            this.GetUserPermission();
        }

        public BaseManager(User user)
        {
            if (user == null)
            {
                throw new KMJXCException("");
            }
            this.CurrentUser = user;           
            this.GetUserPermission();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetUserPermission()
        {
            currentUserPermission = this.permissionManager.GetUserPermission(this.CurrentUser);
        }        
    }
}
