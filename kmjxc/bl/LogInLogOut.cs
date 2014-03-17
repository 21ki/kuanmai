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
    public class LogInLogOut
    {

        UserManager userMgr = null;

        public LogInLogOut()
        {
            userMgr = new UserManager();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="mall_type_id"></param>
        /// <returns></returns>
        public Access_Token AuthorizationCallBack(string code, int mall_type_id)
        {
            Access_Token request_token = null;
            IAccessToken tokenUtil = new TaoBaoAccessToken((long)mall_type_id);
            request_token = tokenUtil.RequestAccessToken(code);
            User requester = new User();
            if (request_token != null)
            {
                requester.Mall_Type = mall_type_id;
                requester.Mall_ID = request_token.Mall_User_ID;
                requester.Mall_Name = request_token.Mall_User_Name;               
            }
            KuanMaiEntities db = new KuanMaiEntities();
            var db_user = from u in db.User where u.Mall_ID == requester.Mall_ID && u.Mall_Name == requester.Mall_Name && u.Mall_Type == requester.Mall_Type select u;

            List<User> users = db_user.ToList<User>();
            if (users.Count == 0)
            {
                requester.Name = requester.Mall_Name;
                requester.Password = Guid.NewGuid().ToString();
                requester=userMgr.CreateNewUser(requester);
            }
            //Access_Token local_token = GetLocalToken();
            return request_token;
        }

        public Access_Token GetLocalToken(int user_id, int mall_type_id)
        {
            Access_Token token = null;

            KuanMaiEntities db = new KuanMaiEntities();

            var etoken = from p in db.Access_Token where p.Mall_Type_ID == mall_type_id && p.User_ID == user_id select p;

            if (etoken != null)
            {
                token = etoken.ToList<Access_Token>()[0];
            }

            return token;
        }
    }     
}
