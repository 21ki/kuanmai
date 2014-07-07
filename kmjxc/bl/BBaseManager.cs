using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using System.Data.Entity;
using KM.JXC.BL.Models;
using KM.JXC.Common.Util;

namespace KM.JXC.BL
{
    public enum SaleDetailStatus
    {
        INITIALIZED=0,
        LEAVED_STOCK=1,
        NO_ENOUGH_STOCK=2,
        NOT_CONNECTED=3,
        REFOUND_BEFORE_SEND=4,
        BACK_STOCK=5,
        REFOUNDED_WAIT_HANDLE=6,
        REFOUND_HANDLED=7
    }

    public class UserActionAttribute : System.Attribute
    {
        public string Description { get; set; }        
    }

    public class UserLogAction
    {
        [UserActionAttribute(Description = "创建采购订单")]
        public static readonly int CREATE_BUY_ORDER = 1;

        [UserActionAttribute(Description = "生成验货单")]
        public static readonly int CREATE_BUY = 2;

        [UserActionAttribute(Description = "创建采购询价单")]
        public static readonly int CREATE_BUY_PRICE = 3;

        [UserActionAttribute(Description = "更新库存")]
        public static readonly int UPDATE_STOCK = 4;

        [UserActionAttribute(Description = "创建退货单")]
        public static readonly int CREATE_BACK_SALE = 5;

        [UserActionAttribute(Description = "创建退库单")]
        public static readonly int CREATE_BACK_STOCK = 6;

        [UserActionAttribute(Description = "创建出库单")]
        public static readonly int CREATE_LEAVE_STOCK = 7;

        [UserActionAttribute(Description = "创建入库单")]
        public static readonly int CREATE_ENTER_STOCK = 8;

        [UserActionAttribute(Description = "创建商品")]
        public static readonly int CREATE_PRODUCT = 9;

        [UserActionAttribute(Description = "修改商品")]
        public static readonly int UPDATE_PRODUCT = 10;

        [UserActionAttribute(Description = "更新采购订单")]
        public static readonly int UPDATE_BUY_ORDER = 11;

        [UserActionAttribute(Description = "处理退货单")]
        public static readonly int HANDLE_BACK_SALE = 12;

        [UserActionAttribute(Description = "处理退库单")]
        public static readonly int HANDLE_BACK_STOCK = 13;

        [UserActionAttribute(Description = "处理入库单")]
        public static readonly int HANDLE_ENTER_STOCK = 14;

        [UserActionAttribute(Description = "用户登录")]
        public static readonly int USER_LOGIN = 15;

        [UserActionAttribute(Description = "创建商品类目")]
        public static readonly int CREATE_PRODUCT_CATEGORY = 16;

        [UserActionAttribute(Description = "创建商品销售属性")]
        public static readonly int CREATE_PRODUCT_PROPERTY = 17;

        [UserActionAttribute(Description = "添加快递")]
        public static readonly int CREATE_SHOP_EXPRESS = 18;

        [UserActionAttribute(Description = "添加快递费用")]
        public static readonly int CREATE_SHOP_EXPRESS_FEE = 19;

        [UserActionAttribute(Description = "添加仓库")]
        public static readonly int CREATE_STOREHOUSE = 20;

        [UserActionAttribute(Description = "更新仓库")]
        public static readonly int UPDATE_STOREHOUSE = 21;

        [UserActionAttribute(Description = "更新快递费用")]
        public static readonly int UPDATE_SHOP_EXPRESS_FEE = 22;

        [UserActionAttribute(Description = "同步在售宝贝")]
        public static readonly int SYNC_SHOP_ONSALE_PRODUCT = 23;

        [UserActionAttribute(Description = "同步订单")]
        public static readonly int SYNC_SHOP_TRADE = 24;
    }

    public class BBaseManager:CommonManager
    {
        public static List<Common_District> Areas = null;

        public Shop Shop { get; private set; }
        public Shop Main_Shop { get; private set; }
        protected List<Shop> DBChildShops { get; set; }
        public BUser CurrentUser { get; private set; }
        public BUser MainUser { get; private set; }
        public int Shop_Id { get; private set; }
        public int Main_Shop_Id { get; private set; }
        public Permission CurrentUserPermission {get;private set;}
        private PermissionManager permissionManager;
        public Access_Token AccessToken { get; private set; }
        
        public List<BShop> ChildShops
        {
            get{
                List<BShop> ss = new List<BShop>();

                ss = (from shop in this.DBChildShops
                      select new BShop
                      {
                          ID = shop.Shop_ID,
                          Title = shop.Name,
                          Type = new Mall_Type() { Mall_Type_ID=shop.Mall_Type_ID}
                      }).ToList<BShop>();

                return ss;
            }
        }

        static BBaseManager()
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Areas = (from area in db.Common_District select area).ToList<Common_District>();
            }
        }

        public BBaseManager(BUser user,int shop_id,Permission permission)
        {
            this.CurrentUser = user;
            GetUserById(user.ID);
            this.Shop_Id = shop_id;
            if (this.Shop_Id == 0)
            {
                this.GetShops();
            }
            permissionManager = new PermissionManager(shop_id);
            this.CurrentUserPermission = permission;
            this.GetUserPermission();
        }

        public BBaseManager(BUser user, Shop shop, Permission permission)
        {
            this.CurrentUser = user;
            GetUserById(user.ID);
            this.Shop = shop;
            this.GetShops();
            permissionManager = new PermissionManager(this.Shop.Shop_ID);
            this.CurrentUserPermission = permission;
            this.GetUserPermission();
        }

        public BBaseManager(int user_id, int shop_id, Permission permission)
        {
            GetUserById(user_id);
            this.Shop_Id = shop_id;
            if (this.Shop_Id == 0)
            {
                this.GetShops();
            }
            permissionManager = new PermissionManager(shop_id);
            this.CurrentUserPermission = permission;
            this.GetUserPermission();
        }

        public BBaseManager(int user_id, Permission permission)
        {
            GetUserById(user_id);
            this.GetShops();
            permissionManager = new PermissionManager();
            this.CurrentUserPermission = permission;
            this.GetUserPermission();            
        }

        public BBaseManager(BUser user, Permission permission)
        {
            GetUserById(user.ID);
            permissionManager = new PermissionManager();
            this.GetShops();
            this.CurrentUserPermission = permission;
            this.CurrentUser = user;
            this.GetUserPermission();           
        }        

        private void GetUserById(int user_id)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var cu = from us in db.User
                         join mtype in db.Mall_Type on us.Mall_Type equals mtype.Mall_Type_ID into lMtype
                         from l_mtype in lMtype.DefaultIfEmpty()
                         join shop in db.Shop on us.Shop_ID equals shop.Shop_ID into LShop
                         from l_shop in LShop.DefaultIfEmpty()
                         where us.User_ID == user_id
                         select new BUser
                         {
                             //EmployeeInfo = (from employee in db.Employee
                             //                where employee.User_ID == us.User_ID
                             //                select new BEmployee
                             //                {
                             //                    ID = employee.Employee_ID,
                             //                    Name = employee.Name
                             //                }).FirstOrDefault<BEmployee>(),
                             ID = us.User_ID,
                             Mall_ID = us.Mall_ID,
                             Mall_Name = us.Mall_Name,
                             Name = us.Name,
                             Parent_ID = (int)us.Parent_User_ID,
                             Password = us.Password,
                             Type = new BMallType
                             {
                                 ID = l_mtype.Mall_Type_ID,
                                 Name = l_mtype.Name
                             },
                             Shop = l_shop != null ?
                             new BShop { 
                                ID=l_shop.Shop_ID,
                                Title=l_shop.Name
                             }
                             : new BShop {
                                 ID = 0,
                                 Title = ""
                             },
                             IsSystemUser=us.IsSystemUser
                         };
               this.CurrentUser = cu.ToList<BUser>()[0];
                if (this.CurrentUser != null && this.CurrentUser.Parent_ID > 0 && !string.IsNullOrEmpty(this.CurrentUser.Parent.Mall_ID))
                {
                    this.MainUser = (from us in db.User
                                     where us.User_ID == this.CurrentUser.Parent_ID
                                     select new BUser
                                     {
                                         EmployeeInfo = (from employee in db.Employee
                                                         where employee.User_ID == us.User_ID
                                                         select new BEmployee
                                                         {
                                                             ID = employee.Employee_ID,
                                                             Name = employee.Name
                                                         }).FirstOrDefault<BEmployee>(),
                                         ID = us.User_ID,
                                         Mall_ID = us.Mall_ID,
                                         Mall_Name = us.Mall_Name,
                                         Name = us.Name,
                                         Parent = null,
                                         Parent_ID = (int)us.Parent_User_ID,
                                         Password = us.Password,
                                         Type = (from mtype in db.Mall_Type
                                                 where mtype.Mall_Type_ID == us.Mall_Type
                                                 select new BMallType
                                                 {
                                                     ID = mtype.Mall_Type_ID,
                                                     Name = mtype.Name,
                                                     Description=mtype.Description
                                                 }).FirstOrDefault<BMallType>(),
                                        IsSystemUser=us.IsSystemUser
                                     }).FirstOrDefault<BUser>();
                }
                else
                {
                    this.MainUser = this.CurrentUser;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void GetUserPermission()
        {
            if (this.CurrentUser == null || this.CurrentUser.ID <= 0)
            {
                return;
                //throw new KMJXCException("调用 " + this.GetType() + " 没有传入当前登录用户对象", ExceptionLevel.SYSTEM);
            }

            if (this.CurrentUserPermission == null)
            {
                CurrentUserPermission = this.permissionManager.GetUserPermission(this.CurrentUser);
            }

            if (this.CurrentUser.ID == this.Shop.User_ID)
            {
                //shop owner has full permissions
                Type permission = typeof(Permission);
                FieldInfo[] fields = permission.GetFields();
                foreach (FieldInfo field in fields)
                {
                    field.SetValue(CurrentUserPermission, 1);
                }
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                this.AccessToken = (from at in db.Access_Token where at.User_ID == this.CurrentUser.ID select at).FirstOrDefault<Access_Token>();
            }
        }

        /// <summary>
        /// Find shop for current login user
        /// </summary>
        private void GetShops()
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (this.Shop == null)
                {
                    Shop shop = (from s in db.Shop where s.User_ID == this.MainUser.ID select s).FirstOrDefault<Shop>();
                    if (shop == null)
                    {
                        shop = (from s in db.Shop
                                from sp in db.Shop_User
                                where s.Shop_ID == sp.Shop_ID && sp.User_ID == this.CurrentUser.ID
                                select s).FirstOrDefault<Shop>();

                        if (shop == null)
                        {
                            throw new KMJXCException("你不是店铺掌柜，也不是任何店铺的子账户");
                        }
                    }

                    this.Shop_Id = shop.Shop_ID;
                    this.Shop = shop;                    
                }

                if (this.Shop.Parent_Shop_ID > 0)
                {
                    this.Main_Shop = (from s in db.Shop where s.Shop_ID == this.Shop.Parent_Shop_ID select s).FirstOrDefault<Shop>();
                    this.DBChildShops = new List<Shop>();
                }
                else
                {
                    this.Main_Shop = this.Shop;
                    this.DBChildShops = (from s in db.Shop where s.Parent_Shop_ID == this.Main_Shop.Shop_ID select s).ToList();
                }
            }
        }

        protected void UpdateProperties(object oldObj,object newObj)
        {
            if (oldObj.GetType().ToString() != newObj.GetType().ToString())
            {
                throw new KMJXCException("更新model数据时两个对象的类型必须相同",ExceptionLevel.SYSTEM);
            }

            Type type = oldObj.GetType();

            
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                property.SetValue(oldObj, property.GetValue(newObj));
            }
        }

        protected string GetTradeStatusText(string status) 
        {
            string mallTradeStatus = "";

            switch (this.Shop.Mall_Type_ID)
            {
                case 1:
                    if (status == "1")
                    {
                        mallTradeStatus = "WAIT_BUYER_CONFIRM_GOODS";
                    }
                    else if (status == "2")
                    {
                        mallTradeStatus = "TRADE_CLOSED";
                    }
                    else if (status == "3")
                    {
                        mallTradeStatus = "TRADE_FINISHED";
                    }
                    else if (status == "4")
                    {
                        mallTradeStatus = "SELLER_CONSIGNED_PART";
                    }
                    break;
                default:
                    break;
            }

            return mallTradeStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsTokenExpired(Access_Token token)
        {
            bool result = false;
            long timeNow = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            if (timeNow >= token.Request_Time + token.Expirse_In)
            {
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void CreateActionLog(BUserActionLog action)
        {
            if (action == null)
            {
                return;
            }

            if (action.Shop == null)
            {
                return;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                User_Action_Log log = new User_Action_Log();

                log.Action = action.Action.Action_ID;

                log.Description = action.Description;
                if (action.User != null && action.User.ID > 0)
                {
                    log.User_ID = action.User.ID;
                }
                else
                {
                    log.User_ID = this.CurrentUser.ID;
                }

                log.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                if (string.IsNullOrEmpty(log.Description))
                {
                    log.Description = "";
                }
                if (action.Shop != null)
                {
                    log.Shop_ID = action.Shop.ID;
                }
                db.User_Action_Log.Add(log);
                db.SaveChanges();
            }
        }
    }
}
