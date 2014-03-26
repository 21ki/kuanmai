using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using KM.JXC.DBA;

namespace KM.JXC.BL.Open
{
    public class BaseAccessToken:OBaseManager
    {   
        public BaseAccessToken(int mall_type_id)
            :base(mall_type_id)
        {
           
        }      

        public Access_Token GetLocAccessToken(long user_id) 
        {
            Access_Token token = null;

            KuanMaiEntities db = new KuanMaiEntities();

            var etoken = from p in db.Access_Token where p.Mall_Type_ID == this.Mall_Type_ID && p.User_ID == user_id select p;

            if (etoken != null)
            {
                token = etoken.ToList<Access_Token>()[0];
            }

            return token;
        }
    }
}
