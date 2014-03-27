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
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    /// <summary>
    /// Call back access manager
    /// </summary>
    public class AccessManager
    {
        public IAccessToken TokenManager{get;set;}
        public IOShopManager ShopManager { get; private set; }
        public IOUserManager MallUserManager { get; private set; }
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
            this.MallUserManager = new TaoBaoUserManager(token,this.Mall_Type_ID);
        }

        /// <summary>
        /// Calback from Mall Open API Authorization, it will verify if current login user has access to the system
        /// </summary>
        /// <param name="code">returns by Mall Open API Authorization</param>
        /// <returns></returns>
        public Access_Token AuthorizationCallBack(string code)
        {
            Access_Token request_token = null;            
            request_token = TokenManager.RequestAccessToken(code);
            if (request_token == null)
            {
                throw new KMJXCException("没有获取到Access token",ExceptionLevel.SYSTEM);
            }

            BUser requester = new BUser();
            requester.Type = new Mall_Type(){ Mall_Type_ID= this.Mall_Type_ID};
            requester.Mall_ID = request_token.Mall_User_ID;
            requester.Mall_Name = request_token.Mall_User_Name;
            requester.Parent_ID = 0;
            requester.Parent = null;

            this.InitializeMallManagers(request_token);

            if (this.ShopManager == null)
            {
                throw new KMJXCException("IShopManager 实例为null", ExceptionLevel.SYSTEM);
            }

            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                var db_user = from u in db.User
                              where u.Mall_ID == requester.Mall_ID && u.Mall_Name == requester.Mall_Name && u.Mall_Type == this.Mall_Type_ID
                              select new BUser {
                                  ID=u.User_ID,
                                  Name = u.Name,
                                  Mall_Name = u.Mall_Name,
                                  Mall_ID = u.Mall_ID,
                                  Password = u.Password,
                                  Parent_ID = (int)u.Parent_User_ID,
                                  Type = new Mall_Type { Mall_Type_ID = u.Mall_Type }
                              };
                List<BUser> users = db_user.ToList<BUser>();
                //Create user in local db with mall owner id
                if (users.Count == 0)
                {
                    //check if current user's shop is ready in system
                    Shop shop = this.ShopManager.GetShop(requester);
                    if (shop == null)
                    {
                        BUser subUser = this.MallUserManager.GetSubUser(requester.Mall_ID, requester.Mall_Name);
                        if (subUser == null)
                        {
                            throw new KMJXCException("用户:" + requester.Mall_Name + " 没有对应的" + ((KM.JXC.BL.Open.OBaseManager)this.ShopManager).MallType.Description + ",并且不属于任何店铺的子账户", ExceptionLevel.ERROR);
                        }
                        else
                        {
                            //
                            if (subUser.Parent==null || string.IsNullOrEmpty(subUser.Parent.Mall_Name))
                            {
                                throw new KMJXCException("用户:" + requester.Mall_Name + " 没有对应的" + ((KM.JXC.BL.Open.OBaseManager)this.ShopManager).MallType.Description + ",并且不属于任何店铺的子账户", ExceptionLevel.ERROR);
                            }

                            BUser mainUser = null;

                            var u = from us in db.User
                                    where us.Mall_ID == subUser.Parent.Mall_ID && us.Mall_Type == requester.Type.Mall_Type_ID && us.Mall_Name == subUser.Parent.Mall_Name
                                    select new BUser
                                    {
                                        ID = us.User_ID,
                                        Name = us.Name,
                                        Mall_Name = us.Mall_Name,
                                        Mall_ID = us.Mall_ID,
                                        Password = us.Password,
                                        Parent_ID = (int)us.Parent_User_ID,
                                        Type = new Mall_Type { Mall_Type_ID=us.Mall_Type }
                                    };
                            if (u.ToList<BUser>().Count() == 1)
                            {
                                mainUser = u.ToList<BUser>()[0];
                            }

                            if (mainUser == null)
                            {
                                throw new KMJXCException("主账户:" + subUser.Parent.Mall_Name + " 还没有初始化店铺信息，所有子账户无法登录系统", ExceptionLevel.ERROR);
                            }

                            requester.Parent_ID = mainUser.ID;
                            requester.Parent = mainUser;
                            requester.EmployeeInfo=subUser.EmployeeInfo;
                           
                        }
                    }                   

                    //create user in local db
                    requester.Name = requester.Mall_Name;
                    requester.Password = Guid.NewGuid().ToString();

                    User dbUser = new User();
                    dbUser.User_ID = requester.ID;
                    dbUser.Mall_ID = requester.Mall_ID;
                    dbUser.Mall_Name = requester.Mall_Name;
                    dbUser.Name = requester.Name;
                    dbUser.Mall_Type = requester.Type.Mall_Type_ID;
                    if (requester.Parent != null)
                    {
                        dbUser.Parent_Mall_ID = requester.Parent.Mall_ID;
                        dbUser.Parent_Mall_Name = requester.Parent.Mall_Name;
                        dbUser.Parent_User_ID = requester.Parent.ID;
                    }

                    db.User.Add(dbUser);
                    db.SaveChanges();

                    //create access token for the new user
                    request_token.User_ID = dbUser.User_ID;
                    requester.ID = dbUser.User_ID;
                    db.Access_Token.Add(request_token);

                    //save employee
                    if (requester.Parent_ID > 0 && requester.EmployeeInfo != null)
                    {
                        requester.EmployeeInfo.User_ID = requester.ID;
                        db.Employee.Add(requester.EmployeeInfo);
                    }

                    Shop_User shop_User = new Shop_User();
                    shop_User.User_ID = requester.ID;
                    shop_User.Shop_ID = shop.Shop_ID;
                    db.Shop_User.Add(shop_User);
                    db.SaveChanges();

                    //create local shop information for the new main user
                    if (shop != null && requester.Parent_ID==0)
                    {
                        shop.User_ID = requester.ID;
                        shop.Parent_Shop_ID = 0;
                        db.Shop.Add(shop);
                        db.SaveChanges();

                        //sync mall sub users to system
                        List<BUser> subUsers = this.MallUserManager.GetSubUsers(requester);
                        if (subUsers != null && subUsers.Count > 0 && shop.Shop_ID>0)
                        {
                            foreach (BUser user in subUsers)
                            {
                                User db1User = new User();
                                db1User.Parent_Mall_ID = requester.Mall_ID;
                                db1User.Parent_Mall_Name = requester.Mall_Name;
                                db1User.Parent_User_ID = (int)requester.ID;
                                db1User.Mall_Name = user.Mall_Name;
                                db1User.Mall_ID = user.Mall_ID;
                                db1User.Mall_Type = user.Type.Mall_Type_ID;
                                db1User.Name = user.Name;
                                db1User.Password = "";
                                db.User.Add(db1User);

                                db.SaveChanges();

                                if (db1User.User_ID > 0)
                                {
                                    //add shop user
                                    Shop_User shop_User1 = new Shop_User();
                                    shop_User1.User_ID = requester.ID;
                                    shop_User1.Shop_ID = shop.Shop_ID;
                                    db.Shop_User.Add(shop_User1);

                                    if (user.EmployeeInfo != null)
                                    {
                                        user.EmployeeInfo.User_ID = db1User.User_ID;
                                        db.Employee.Add(user.EmployeeInfo);
                                        //db.SaveChanges();
                                    }
                                }
                            }

                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    //Verify if local db has non expried accesstoken
                    requester = users[0];
                    request_token.User_ID = requester.ID;
                    Access_Token local_token = GetLocalToken(requester.ID, this.Mall_Type_ID);
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
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public bool ShopUserVerification(BUser user, int shop_id)
        {
            bool result = false;
            
            return result;
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
