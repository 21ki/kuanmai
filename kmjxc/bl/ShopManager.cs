using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;
namespace KM.JXC.BL
{
    public class ShopManager:BBaseManager
    {
        public int Mall_Type { get; private set; }
      
        public UserManager UserManager{get;private set;}
        public ShopManager(BUser user,int shop_id, int mall_type, Permission permission)
            : base(user, shop_id,permission)
        {
            this.Mall_Type = mall_type;
            UserManager = new UserManager(user,permission);
        }

        public ShopManager(BUser user, Shop shop, Permission permission,UserManager userMgr)
            : base(user, shop, permission)
        {
            this.Mall_Type=shop.Mall_Type_ID;
            this.UserManager=userMgr;
        }

        /// <summary>
        /// Get local shop detail
        /// </summary>
        /// <param name="mall_shop_id">Mall Shop ID</param>
        /// <param name="mall_type_id">Local Mall Type ID</param>
        /// <returns></returns>
        public Shop GetShopDetail(string mall_shop_id, int mall_type_id)
        {
            Shop shop = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var s = from sp in db.Shop where sp.Mall_Shop_ID == mall_shop_id && sp.Mall_Type_ID == mall_type_id select sp;
                if (s.ToList<Shop>().Count > 0)
                {
                    shop = s.ToList<Shop>()[0];
                }
            }
            
            return shop;
        }

        /// <summary>
        /// Get local shop detail
        /// </summary>
        /// <param name="shop_Id">Local Shop ID</param>
        /// <returns></returns>
        public Shop GetShopDetail(int shop_Id)
        {
            Shop shop = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var sps = from s in db.Shop where s.User_ID == shop_Id select s;
                List<Shop> shops = new List<Shop>();
                if (sps != null)
                {
                    shops = sps.ToList<Shop>();
                }

                if (shops.Count == 1)
                {
                    shop = shops[0];
                }
            }

            return shop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop"></param>
        /// <returns></returns>
        public void CreateNewShop(Shop shop)
        {   
            if (string.IsNullOrEmpty(shop.Mall_Shop_ID))
            {
                throw new KMJXCException("商城店铺ID丢失，无法创建本地店铺信息");
            }

            if (string.IsNullOrEmpty(shop.Name))
            {
                throw new KMJXCException("商城店铺名丢失，无法创建本地店铺信息");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Shop.Add(shop);
                db.SaveChanges();
            }
        }
       
        /// <summary>
        /// Add child shop
        /// </summary>
        /// <param name="parent_shop"></param>
        /// <param name="shop"></param>
        /// <returns></returns>
        public bool AddChildShop(Shop parent_shop,Shop shop)
        {
            bool result = false;           

            if (shop == null)
            {
                throw new KMJXCException("淘宝或者天猫没有此名称的店铺");
            }

            this.CreateNewShop(shop);
            if (shop.Shop_ID <= 0)
            {
                throw new KMJXCException("本地创建店铺失败");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Shop_Child_Request scr = new Shop_Child_Request();
                scr.Shop_ID = (int)parent_shop.Shop_ID;
                scr.Request_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                scr.Child_Shop_ID = (int)shop.Shop_ID;
                scr.User_ID = (int)this.CurrentUser.ID;
                scr.Status = "0";
                scr.Approve_User_ID = 0;
                scr.Approve_Time = 0;
                db.Shop_Child_Request.Add(scr);
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Approved to be child shop
        /// </summary>
        /// <param name="scr"></param>
        /// <returns></returns>
        public bool ApproveChildShopRequest(Shop_Child_Request scr)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                scr.Status = "1";
                scr.Approve_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                scr.Approve_User_ID = (int)this.CurrentUser.ID;
                db.Shop_Child_Request.Attach(scr);

                var sps = from sp in db.Shop where sp.Shop_ID == scr.Child_Shop_ID select sp;
                Shop childShop = sps.ToList<Shop>()[0];
                childShop.Parent_Shop_ID = scr.Shop_ID;
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BCustomer> SearchCustomers(int page,int pageSize, out long totalRecords)
        {
            totalRecords = 0;
            List<BCustomer> customers = new List<BCustomer>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] csp_ids=(from c in this.ChildShops select c.Shop_ID).ToArray<int>();

                var tmp = from cus in db.Customer
                          join mtype in db.Mall_Type on cus.Mall_Type_ID equals mtype.Mall_Type_ID
                          join shop_cus in db.Customer_Shop on cus.Customer_ID equals shop_cus.Customer_ID
                          join shop in db.Shop on shop_cus.Shop_ID equals shop.Shop_ID
                          where shop_cus.Shop_ID == this.Shop.Shop_ID || shop_cus.Shop_ID == this.Main_Shop.Shop_ID || csp_ids.Contains(shop_cus.Shop_ID)

                          select new BCustomer
                          {
                              ID=cus.Customer_ID,
                              Address = cus.Address,
                              Mall_ID = cus.Mall_ID,
                              Mall_Name = cus.Mall_Name,
                              Type = mtype,
                              Email = cus.Email,
                              Phone = cus.Phone,
                              Name=cus.Name,
                              Shop = new BShop
                              {
                                  Title = shop.Name,
                                  ID = shop.Shop_ID,
                                  Mall_ID = shop.Mall_Shop_ID,
                                  Type = mtype,
                              }
                          };
                tmp = tmp.OrderBy(c => c.ID);
                totalRecords = tmp.Count();
                if (totalRecords > 0)
                {
                    customers = tmp.Skip((page-1)*pageSize).Take(pageSize).ToList<BCustomer>();
                }
                        
            }
            return customers;
        }
    }
}
