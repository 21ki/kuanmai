using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
using KM.JXC.Common.Util;
namespace KM.JXC.BL
{
    public class AccessTokenManager
    {
        public IAccessToken tokenUtil{get;set;}
        UserManager userMgr = null;
        public int Mall_Type_ID { get; set; }
        
        public AccessTokenManager(int mall_type_id)
        {
            userMgr = new UserManager();
            this.Mall_Type_ID = mall_type_id;
            tokenUtil = new TaoBaoAccessToken((long)this.Mall_Type_ID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="mall_type_id"></param>
        /// <returns></returns>
        public Access_Token AuthorizationCallBack(string code)
        {
            Access_Token request_token = null;            
            request_token = tokenUtil.RequestAccessToken(code);
            User requester = new User();
            if (request_token != null)
            {
                requester.Mall_Type = this.Mall_Type_ID;
                requester.Mall_ID = request_token.Mall_User_ID;
                requester.Mall_Name = request_token.Mall_User_Name;               
            }
            KuanMaiEntities db = new KuanMaiEntities();
            var db_user = from u in db.User where u.Mall_ID == requester.Mall_ID && u.Mall_Name == requester.Mall_Name && u.Mall_Type == requester.Mall_Type select u;

            List<User> users = db_user.ToList<User>();
            //Create user in local db with mall owner id
            if (users.Count == 0)
            {
                requester.Name = requester.Mall_Name;
                requester.Password = Guid.NewGuid().ToString();
                requester = userMgr.CreateNewUser(requester);
                //Store access token for new user
                request_token.User_ID = requester.User_ID;
                db.Access_Token.Add(request_token);
                db.SaveChanges();
            }
            else
            {                
                //Verify if local db has non expried accesstoken
                requester = users[0];
                request_token.User_ID = requester.User_ID;
                Access_Token local_token = GetLocalToken(requester.User_ID, this.Mall_Type_ID);
                if (local_token != null)
                {
                    int timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);

                    //last access token is expried
                    if (timeNow >= local_token.Expirse_In + local_token.Request_Time)
                    {
                        UpdateLocalAccessToken(request_token);
                    }
                }
            }
            
            return request_token;
        }

        /// <summary>
        /// Get access token from local db
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="mall_type_id"></param>
        /// <returns></returns>
        private Access_Token GetLocalToken(long user_id, int mall_type_id)
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

        /// <summary>
        /// Update access token in local db
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool UpdateLocalAccessToken(Access_Token token)
        {
            bool result = false;

            Access_Token local_token = null;

            KuanMaiEntities db = new KuanMaiEntities();

            var etoken = from p in db.Access_Token where p.Mall_Type_ID == token.Mall_Type_ID && p.User_ID == token.User_ID select p;

            if (etoken != null)
            {
                local_token = etoken.ToList<Access_Token>()[0];
            }

            if (local_token != null)
            {
                local_token.Access_Token1 = token.Access_Token1;
                local_token.Expirse_In = token.Expirse_In;
                local_token.Request_Time = token.Request_Time;
                local_token.RExpirse_In = token.RExpirse_In;

                db.Access_Token.Attach(local_token);
                db.SaveChanges();
                result = true;
            }

            return result;
        }
    }     
}
