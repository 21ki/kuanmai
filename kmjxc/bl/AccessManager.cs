using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.Common.Util;
namespace KM.JXC.BL
{
    public class AccessManager
    {
        public IAccessToken TokenManager{get;set;}
        public IShopManager ShopManager { get; private set; }
        public IUserManager UserManager { get; private set; }
        public UserManager LocUserManager { get; private set; }
        public int Mall_Type_ID { get; set; }
        
        public AccessManager(int mall_type_id)
        {            
            this.Mall_Type_ID = mall_type_id;
            TokenManager = new TaoBaoAccessToken(this.Mall_Type_ID);            
        }

        /// <summary>
        /// Initialize IManager instances
        /// </summary>
        /// <param name="token">Access token got from IAccessToken</param>
        private void InitializeMallManagers(Access_Token token)
        {
            this.ShopManager = new TaoBaoShopManager(token, this.Mall_Type_ID);
            this.UserManager = new TaoBaoUserManager(token,this.Mall_Type_ID);
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
            request_token = TokenManager.RequestAccessToken(code);
            if (request_token == null)
            {
                throw new KMJXCException("没有获取到Access token",ExceptionLevel.SYSTEM);
            }

            User requester = new User();
            requester.Mall_Type = this.Mall_Type_ID;
            requester.Mall_ID = request_token.Mall_User_ID;
            requester.Mall_Name = request_token.Mall_User_Name; 

            this.InitializeMallManagers(request_token);

            if (this.ShopManager == null)
            {
                throw new KMJXCException("IShopManager 实例为null", ExceptionLevel.SYSTEM);
            }

            KuanMaiEntities db = new KuanMaiEntities();
            try
            { 
                var db_user = from u in db.User where u.Mall_ID == requester.Mall_ID && u.Mall_Name == requester.Mall_Name && u.Mall_Type == requester.Mall_Type select u;
                List<User> users = db_user.ToList<User>();
                //Create user in local db with mall owner id
                if (users.Count == 0)
                {
                    //check if current user's shop is ready in system
                    Shop shop = this.ShopManager.GetShop(requester.Mall_ID, requester.Mall_Name);
                    if (shop == null)
                    {
                        User subUser = this.UserManager.GetSubUser(requester.Mall_ID, requester.Mall_Name);
                        if (subUser == null)
                        {
                            throw new KMJXCException("用户:" + requester.Mall_Name + " 没有对应的" + this.ShopManager.MallType.Description + ",并且不属于任何店铺的子账户", ExceptionLevel.ERROR);
                        }
                        else
                        {
                            //not any user's sub user
                            if (!string.IsNullOrEmpty(subUser.Parent_Mall_ID))
                            {
                                throw new KMJXCException("用户:" + requester.Mall_Name + " 没有对应的" + this.ShopManager.MallType.Description + ",并且不属于任何店铺的子账户", ExceptionLevel.ERROR);
                            }

                            User mainUser = null;

                            var u = from us in db.User where us.Mall_ID == subUser.Parent_Mall_ID && us.Mall_Type == requester.Mall_Type && us.Parent_Mall_Name == subUser.Parent_Mall_Name select us;
                            if (u.ToList<User>().Count() == 1)
                            {
                                mainUser = u.ToList<User>()[0];
                            }

                            if (mainUser == null)
                            {
                                throw new KMJXCException("主账户:" + subUser.Parent_Mall_Name + " 还没有初始化店铺信息，所有子账户无法登录系统", ExceptionLevel.ERROR);
                            }

                            requester.Parent_Mall_ID = subUser.Parent_Mall_ID;
                            requester.Parent_Mall_Name = subUser.Parent_Mall_Name;
                            requester.Parent_User_ID = (int)mainUser.User_ID;
                        }
                    }
                    else
                    {
                        //create main user and shop
                    }

                    //create sub user in local db
                    requester.Name = requester.Mall_Name;
                    requester.Password = Guid.NewGuid().ToString();

                    //create the new user
                    db.User.Add(requester);
                    db.SaveChanges();
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
            }
            catch (Exception ex)
            {
                throw new KMJXCException(ex.Message,ExceptionLevel.SYSTEM);
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
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
