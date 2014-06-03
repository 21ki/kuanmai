using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

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
        /// 
        /// </summary>
        /// <param name="reqId"></param>
        /// <param name="status"></param>
        public void HandleAddChildRequest(int reqId, int status)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Shop_Child_Request request=(from req in db.Shop_Child_Request where req.ID==reqId select req).FirstOrDefault<Shop_Child_Request>();
                if (request == null)
                {
                    throw new KMJXCException("请求已经被删除，不能操作");
                }

                int child_shop = request.Child_Shop_ID;

                Shop cshop=(from shop in db.Shop where shop.User_ID==this.CurrentUser.ID select shop).FirstOrDefault<Shop>();
                if (cshop == null)
                {
                    throw new KMJXCException("您没有权限处理请求，请用子店铺主账户登录来处理请求");
                }

                if (cshop.Shop_ID != child_shop)
                {
                    throw new KMJXCException("您没有权限操作此请求");
                }

                request.Status = status;
                request.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                request.Modified_By = this.CurrentUser.ID;
                if (status == 1)
                {
                    var sps = from sp in db.Shop where sp.Shop_ID == request.Child_Shop_ID select sp;
                    Shop childShop = sps.FirstOrDefault<Shop>();
                    if (childShop != null)
                    {
                        childShop.Parent_Shop_ID = request.Shop_ID;
                    }
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="child_shop_id"></param>
        /// <returns></returns>
        public bool AddChildShop(int mall_type,string child_shop_name)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Shop shop = (from sp in db.Shop where sp.Name == child_shop_name && sp.Mall_Type_ID==mall_type select sp).FirstOrDefault<Shop>();
                if (shop == null)
                {
                    throw new KMJXCException("您要添加的子店铺信息不存在，请先使用子店铺的主账户登录进销存，然后在执行添加子店铺操作");
                }

                result = this.AddChildShop(this.Shop, shop);
            }

            return result;
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
                throw new KMJXCException("您要添加的子店铺信息不存在，请先使用子店铺的主账户登录进销存，然后在执行添加子店铺操作");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Shop_Child_Request scr = (from sr in db.Shop_Child_Request where sr.Shop_ID == parent_shop.Shop_ID && sr.Child_Shop_ID == shop.Shop_ID && sr.Status == 0 select sr).FirstOrDefault<Shop_Child_Request>();
                if (scr == null)
                {
                    scr.Shop_ID = (int)parent_shop.Shop_ID;
                    scr.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    scr.Child_Shop_ID = (int)shop.Shop_ID;
                    scr.Created_By = (int)this.CurrentUser.ID;
                    scr.Status = 0;
                    scr.Modified_By = 0;
                    scr.Modified = 0;
                    db.Shop_Child_Request.Add(scr);
                    db.SaveChanges();
                    result = true;
                }
                else
                {
                    throw new KMJXCException("已经发送过添加子店铺请求，请不要重复发送");
                }
            }

            return result;
        }

        

        public List<BAddChildRequest> SearchReceivedAddChildRequests()
        {
            List<BAddChildRequest> requests = new List<BAddChildRequest>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from request in db.Shop_Child_Request where request.Child_Shop_ID == this.Shop.Shop_ID select request;

                var tmpRequests = from request in tmp
                                  join pshop in db.Shop on request.Shop_ID equals pshop.Shop_ID into lPshop
                                  from l_pshop in lPshop.DefaultIfEmpty()

                                  join cshop in db.Shop on request.Child_Shop_ID equals cshop.Shop_ID into lCshop
                                  from l_cshop in lCshop.DefaultIfEmpty()

                                  join luser in db.User on request.Created_By equals luser.User_ID into lUser
                                  from l_user in lUser.DefaultIfEmpty()

                                  join lmuser in db.User on request.Modified_By equals lmuser.User_ID into lMUser
                                  from l_muser in lMUser.DefaultIfEmpty()
                                  select new BAddChildRequest
                                  {
                                      ID=request.ID,
                                      Child = new BShop
                                      { 
                                          ID=l_cshop.Shop_ID,
                                          Title=l_cshop.Name
                                      },
                                      Parent=new BShop
                                      {
                                          ID = l_pshop.Shop_ID,
                                          Title = l_pshop.Name
                                      },
                                      Created_By = l_user != null ? new BUser
                                      {
                                          ID = l_user.User_ID,
                                          Mall_Name = l_user.Mall_Name,
                                          Mall_ID = l_user.Mall_ID
                                      } : new BUser
                                      {
                                          ID = 0,
                                          Mall_Name = "",
                                          Mall_ID = ""
                                      },
                                      Modified_By = l_muser != null ? new BUser()
                                      {
                                          ID = l_muser.User_ID,
                                          Mall_Name = l_muser.Mall_Name,
                                          Mall_ID = l_muser.Mall_ID
                                      } : new BUser
                                      {
                                          ID = 0,
                                          Mall_Name = "",
                                          Mall_ID = ""
                                      },
                                      Created = request.Created,
                                      Modified = (int)request.Modified
                                  };

                requests = tmpRequests.ToList<BAddChildRequest>();
            }
            return requests;
        }

        public List<BAddChildRequest> SearchSentAddChildRequests()
        {
            List<BAddChildRequest> requests = new List<BAddChildRequest>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from request in db.Shop_Child_Request where request.Shop_ID == this.Shop.Shop_ID select request;

                var tmpRequests = from request in tmp
                                  join pshop in db.Shop on request.Shop_ID equals pshop.Shop_ID into lPshop
                                  from l_pshop in lPshop.DefaultIfEmpty()

                                  join cshop in db.Shop on request.Child_Shop_ID equals cshop.Shop_ID into lCshop
                                  from l_cshop in lCshop.DefaultIfEmpty()

                                  join luser in db.User on request.Created_By equals luser.User_ID into lUser
                                  from l_user in lUser.DefaultIfEmpty()

                                  join lmuser in db.User on request.Modified_By equals lmuser.User_ID into lMUser
                                  from l_muser in lMUser.DefaultIfEmpty()
                                  select new BAddChildRequest
                                  {
                                      ID = request.ID,
                                      Child = new BShop
                                      {
                                          ID = l_cshop.Shop_ID,
                                          Title = l_cshop.Name
                                      },
                                      Parent = new BShop
                                      {
                                          ID = l_pshop.Shop_ID,
                                          Title = l_pshop.Name
                                      },
                                      Created_By = l_user != null ? new BUser
                                      {
                                          ID = l_user.User_ID,
                                          Mall_Name = l_user.Mall_Name,
                                          Mall_ID = l_user.Mall_ID
                                      } : new BUser {
                                          ID = 0,
                                          Mall_Name = "",
                                          Mall_ID = ""
                                      },
                                      Modified_By =l_muser!=null? new BUser()
                                      {
                                          ID = l_muser.User_ID,
                                          Mall_Name = l_muser.Mall_Name,
                                          Mall_ID = l_muser.Mall_ID
                                      }: new BUser {
                                          ID = 0,
                                          Mall_Name = "",
                                          Mall_ID = ""
                                      },
                                      Created = request.Created,
                                      Modified = (int)request.Modified
                                  };

                requests = tmpRequests.ToList<BAddChildRequest>();
            }
            return requests;
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

        /// <summary>
        /// 
        /// </summary>
        public void CreateShopExpress(BShopExpress express)
        {
            if (this.CurrentUserPermission.ADD_SHOP_EXPRESS == 0)
            {
                throw new KMJXCException("没有权限添加店铺快递信息");
            }

            if (express == null)
            {
                throw new KMJXCException("异常出错");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    Express_Shop es=(from exp in db.Express_Shop where exp.Express_ID==express.ID && exp.Shop_ID==this.Shop.Shop_ID select exp).FirstOrDefault<Express_Shop>();
                    if (es != null)
                    {
                        throw new KMJXCException("已经添加过此快递公司");
                    }

                    es = new Express_Shop();
                    es.Shop_ID = this.Shop.Shop_ID;
                    es.Express_ID = express.ID;
                    es.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    es.Modified = es.Created;
                    es.Created_By = this.CurrentUser.ID;
                    es.Modified_By = es.Created_By;
                    if (express.IsDefault)
                    {
                        es.IsDefault = 1;

                        //revert old default exp
                        Express_Shop defaultExp=(from exp in db.Express_Shop where exp.IsDefault==1 && exp.Shop_ID==es.Shop_ID select exp).FirstOrDefault<Express_Shop>();
                        if (defaultExp != null)
                        {
                            defaultExp.IsDefault = 0;
                        }
                    }
                    else
                    {
                        es.IsDefault = 0;
                    }
                    db.Express_Shop.Add(es);
                    db.SaveChanges();

                    if (express.Fees != null && express.Fees.Count > 0)
                    {
                        foreach (BExpressFee fee in express.Fees)
                        {
                            Express_Fee sFee = new Express_Fee();
                            if (fee.City != null)
                            {
                                sFee.City_ID = fee.City.ID;
                            }
                            sFee.Created = es.Created;
                            sFee.Created_By = es.Created_By;
                            sFee.Express_ID = es.Express_ID;
                            sFee.Fee = fee.Fee;
                            sFee.Modified = es.Modified;
                            sFee.Modified_By = es.Modified_By;
                            if (fee.Province != null)
                            {
                                sFee.Province_ID = fee.Province.ID;
                            }
                            if (fee.StoreHouse != null && fee.StoreHouse.ID > 0)
                            {
                                sFee.StoreHouse_ID = fee.StoreHouse.ID;
                            }
                            else
                            {
                                continue;
                            }

                            sFee.Shop_ID = es.Shop_ID;
                            db.Express_Fee.Add(sFee);
                        }
                    }

                    db.SaveChanges();

                    trans.Complete();
                }
            }
        }

        public void CreateExpressFees(BShopExpress express)
        {
            if (this.CurrentUserPermission.ADD_SHOP_EXPRESS == 0)
            {
                throw new KMJXCException("没有权限添加店铺快递信息");
            }

            if (express == null)
            {
                throw new KMJXCException("异常出错");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                using (TransactionScope trans = new TransactionScope())
                {
                    Express_Shop es = (from exp in db.Express_Shop where exp.Express_ID == express.ID && exp.Shop_ID == this.Shop.Shop_ID select exp).FirstOrDefault<Express_Shop>();
                    if (es == null)
                    {
                        throw new KMJXCException("店铺还没有添加此快递公司，请先添加快递公司，然后再添加快递费用");
                    }

                    List<Express_Fee> allFees=(from fee in db.Express_Fee where fee.Shop_ID==es.Shop_ID select fee).ToList<Express_Fee>();
                  

                    if (express.Fees != null && express.Fees.Count > 0)
                    {
                        foreach (BExpressFee fee in express.Fees)
                        {
                            if (fee.City == null && fee.Province == null)
                            {
                                continue;
                            }

                            if (fee.City == null)
                            {
                                fee.City = new BArea() { ID = 0 };
                            }

                            if (fee.Province == null)
                            {
                                fee.Province = new BArea() { ID = 0 };
                            }

                            Express_Fee sFee = (from efee in allFees where efee.Express_ID==es.Express_ID && efee.Province_ID == fee.Province.ID && efee.City_ID == fee.City.ID && efee.StoreHouse_ID == fee.StoreHouse.ID select efee).FirstOrDefault<Express_Fee>();
                            bool isNew = false;
                            if (sFee == null)
                            {
                                isNew = true;
                                sFee = new Express_Fee();
                            }                           
                           
                            sFee.Fee = fee.Fee;
                            sFee.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            sFee.Modified_By = this.CurrentUser.ID;
                           
                            if (fee.StoreHouse != null && fee.StoreHouse.ID > 0)
                            {
                                sFee.StoreHouse_ID = fee.StoreHouse.ID;
                            }
                            else
                            {
                                continue;
                            }
                            
                            if (isNew)
                            {
                                if (fee.Province != null)
                                {
                                    sFee.Province_ID = fee.Province.ID;
                                }

                                if (fee.City != null)
                                {
                                    sFee.City_ID = fee.City.ID;
                                }
                                sFee.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                                sFee.Created_By = this.CurrentUser.ID;
                                sFee.Express_ID = es.Express_ID;
                                sFee.Shop_ID = es.Shop_ID;
                                db.Express_Fee.Add(sFee);
                            }
                        }
                    }

                    db.SaveChanges();

                    trans.Complete();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Express> GetNonAddedExpresses()
        {
            List<Express> expresses = new List<Express>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {

                int[] express_ids=(from sp_exp in db.Express_Shop where sp_exp.Shop_ID==this.Shop.Shop_ID select sp_exp.Express_ID).Distinct().ToArray<int>();

                expresses = (from express in db.Express where !express_ids.Contains(express.Express_ID) orderby express.Express_ID ascending select express).ToList<Express>();
            }
            return expresses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BStoreHouse> GetNonExpressedHouses(int express_id,int shop_id=0)
        {
            if (express_id == 0)
            {
                throw new KMJXCException("必须选择快递公司来查询没有此快递公司费用的仓库");
            }
            List<BStoreHouse> houses = new List<BStoreHouse>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] child_shop=(from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                var tmpHouse = from house in db.Store_House
                               select house;

                if (shop_id == 0)
                {
                    tmpHouse = tmpHouse.Where(h => h.Shop_ID == this.Shop.Shop_ID || h.Shop_ID == this.Main_Shop.Shop_ID || child_shop.Contains(h.Shop_ID));
                }
                else
                {
                    tmpHouse = tmpHouse.Where(h => h.Shop_ID == shop_id);
                }

                var epf = from fee in db.Express_Fee
                          where fee.Express_ID == express_id
                          select fee.StoreHouse_ID;

                int[] house_ids = epf.Distinct().ToArray<int>();

                if (house_ids != null && house_ids.Length > 0)
                {
                    tmpHouse = tmpHouse.Where(h => !house_ids.Contains(h.StoreHouse_ID));
                }

                var tmp = from house in tmpHouse
                          select new BStoreHouse
                          {
                              Address = house.Address,
                              ID = house.StoreHouse_ID,
                              Name = house.Title
                          };

                houses = tmp.ToList<BStoreHouse>();
            }

            return houses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BShopExpress> SearchExpresses()
        {
            List<BShopExpress> expresses = new List<BShopExpress>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from shop_express in db.Express_Shop
                          join express in db.Express on shop_express.Express_ID equals express.Express_ID
                          join shop in db.Shop on shop_express.Shop_ID equals shop.Shop_ID
                          join created_by in db.User on shop_express.Created_By equals created_by.User_ID
                          join modified_by in db.User on shop_express.Modified_By equals modified_by.User_ID
                          select new BShopExpress
                          {
                              ID = shop_express.Express_ID,
                              Name = express.Name,
                              Created = shop_express.Created,
                              Modified = shop_express.Modified,
                              Shop = new BShop
                              {
                                  ID = shop.Shop_ID,
                                  Title = shop.Name
                              },
                              Created_By = new BUser
                              {
                                  ID = created_by.User_ID,
                                  Mall_ID = created_by.Mall_ID,
                                  Mall_Name = created_by.Mall_Name
                              },
                              Modified_By = new BUser
                              {
                                  ID = modified_by.User_ID,
                                  Mall_ID = modified_by.Mall_ID,
                                  Mall_Name = modified_by.Mall_Name
                              },
                              IsDefault = shop_express.IsDefault == 0 ? false : true
                          };

                expresses = tmp.OrderBy(a => a.ID).ToList<BShopExpress>();
            }
            return expresses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="express_id"></param>
        /// <param name="shop_id"></param>
        public void SetDefaultExpress(int express_id,int shop_id=0) 
        {
            if (express_id <= 0)
            {
                throw new KMJXCException("快递公司不能为空");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] child_shop = (from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                var tmpNew = from express in db.Express_Shop
                             where (express.IsDefault == 0 && express.Express_ID==express_id)
                             select express;
                var tmp = from express in db.Express_Shop
                          where express.IsDefault==1
                          select express;

                if (shop_id > 0)
                {
                    tmp = tmp.Where(t => t.Shop_ID == shop_id);
                    tmpNew = tmpNew.Where(t => t.Shop_ID == shop_id);
                }
                else
                {
                    tmp = tmp.Where(t => t.Shop_ID == this.Shop.Shop_ID);
                    tmpNew = tmpNew.Where(t => t.Shop_ID == this.Shop.Shop_ID);
                }

                Express_Shop old = tmp.FirstOrDefault<Express_Shop>();
                Express_Shop newDefault = tmpNew.FirstOrDefault<Express_Shop>();
                if (old != null)
                {
                    old.IsDefault = 0;
                    old.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    old.Modified_By = this.CurrentUser.ID;
                    db.Entry(old).State = System.Data.EntityState.Modified;
                    
                    //db.Express_Shop.SqlQuery("Update Express_Shop set IsDefault=0 where Express_ID="+old.Express_ID+" and Shop_ID="+old.Shop_ID);
                }                
                
                if (newDefault != null)
                {
                    newDefault.IsDefault = 1;
                    newDefault.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    newDefault.Modified_By = this.CurrentUser.ID;
                    db.Entry(newDefault).State = System.Data.EntityState.Modified;
                    
                    //db.Express_Shop.SqlQuery("Update Express_Shop set IsDefault=1 where Express_ID=" + newDefault.Express_ID + " and Shop_ID=" + newDefault.Shop_ID);
                }
                else
                {
                    throw new KMJXCException("设置默认快递公司前，请先确保快递公司已经添加到店铺");
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="express_fee_id"></param>
        /// <param name="fee"></param>
        public void UpdateExpressFee(int express_fee_id, double fee)
        {
            if (this.CurrentUserPermission.UPDATE_SHOP_EXPRESS == 0)
            {
                throw new KMJXCException("没有权限更新快递以及快递费用");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Express_Fee express_fee=(from expfee in db.Express_Fee where expfee.Express_Fee_ID==express_fee_id select expfee).FirstOrDefault<Express_Fee>();
                if (express_fee == null)
                {
                    throw new KMJXCException("要修改的快递费用不存在");
                }
                express_fee.Modified = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                express_fee.Modified_By = this.CurrentUser.ID;
                express_fee.Fee = fee;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="express_id"></param>
        /// <param name="province_id"></param>
        /// <param name="city_id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public List<BExpressFee> SearchExpressFee(int express_id,int province_id,int city_id,int page, int pageSize, out long totalRecords)
        {
            totalRecords = 0;
            List<BExpressFee> express_fees = new List<BExpressFee>();
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] child_shop=(from c in this.ChildShops select c.Shop_ID).ToArray<int>();
                var tmpFee = from express_fee in db.Express_Fee
                          where express_fee.Shop_ID==this.Shop.Shop_ID || express_fee.Shop_ID==this.Main_Shop.Shop_ID || child_shop.Contains(express_fee.Shop_ID)
                          select express_fee;

                if (express_id > 0)
                {
                    tmpFee = tmpFee.Where(f=>f.Express_ID==express_id);
                }

                if (province_id > 0)
                {
                    tmpFee = tmpFee.Where(f => f.Province_ID == province_id);
                }

                if (city_id > 0)
                {
                    tmpFee = tmpFee.Where(f => f.City_ID == city_id);
                }

                var tmp = from fee in tmpFee
                          join express in db.Express on fee.Express_ID equals express.Express_ID into lExpress
                          from l_express in lExpress.DefaultIfEmpty()
                          join shop in db.Shop on fee.Shop_ID equals shop.Shop_ID into lShop
                          from l_shop in lShop.DefaultIfEmpty()
                          join created_by in db.User on fee.Created_By equals created_by.User_ID into lCreatedBy
                          from l_created_by in lCreatedBy.DefaultIfEmpty()
                          join modified_by in db.User on fee.Modified_By equals modified_by.User_ID into lModifiedBy
                          from l_modified_by in lModifiedBy.DefaultIfEmpty()
                          join house in db.Store_House on fee.StoreHouse_ID equals house.StoreHouse_ID into lHouse
                          from l_house in lHouse.DefaultIfEmpty()
                          join province in db.Common_District on fee.Province_ID equals province.id into lProvince
                          from l_province in lProvince.DefaultIfEmpty()
                          join city in db.Common_District on fee.City_ID equals city.id into lCity
                          from l_city in lCity.DefaultIfEmpty()
                          select new BExpressFee
                          {
                              ID = fee.Express_Fee_ID,
                              Created = fee.Created,
                              Modified = fee.Modified,
                              Fee = fee.Fee,
                              City = l_city != null ? new BArea
                              {
                                  ID = l_city.id,
                                  Name = l_city.name
                              } : new BArea
                              {
                                  ID = 0,
                                  Name = ""
                              },
                              Province = new BArea
                              {
                                  ID = l_province.id,
                                  Name = l_province.name
                              },
                              Created_By = new BUser
                              {
                                  ID = l_created_by.User_ID,
                                  Mall_Name = l_created_by.Mall_Name,
                                  Mall_ID = l_created_by.Mall_ID
                              },
                              Modified_By = new BUser
                              {
                                  ID = l_modified_by.User_ID,
                                  Mall_Name = l_modified_by.Mall_Name,
                                  Mall_ID = l_modified_by.Mall_ID
                              },
                              Express = new BExpress
                              {
                                  ID = l_express.Express_ID,
                                  Name = l_express.Name
                              },
                              Shop = new BShop
                              {
                                  ID = l_shop.Shop_ID,
                                  Title = l_shop.Name
                              },
                              StoreHouse = new BStoreHouse
                              {
                                  ID = l_house.StoreHouse_ID,
                                  Name = l_house.Title,
                                  Address = l_house.Address
                              }
                          };

                totalRecords = tmp.Count();
                if (totalRecords > 0)
                {
                    express_fees = tmp.OrderBy(f => f.ID).Skip((page-1)*pageSize).Take(pageSize).ToList<BExpressFee>();
                }
            }
            return express_fees;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainShop"></param>
        /// <returns></returns>
        public List<BShop> SearchChildShops(int mainShop=0)
        {
            List<BShop> shops = new List<BShop>();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int main = this.Shop.Shop_ID;
                if (mainShop > 0)
                {
                    main = mainShop;
                }
                var tmp = from shop in db.Shop where shop.Parent_Shop_ID == main select shop;

                var tmpShops = from shop in tmp
                               join user in db.User on shop.User_ID equals user.User_ID into lUser
                               from l_user in lUser.DefaultIfEmpty()
                               join mtype in db.Mall_Type on shop.Mall_Type_ID equals mtype.Mall_Type_ID into lMtype
                               from l_mtype in lMtype.DefaultIfEmpty()
                               select new BShop
                               {
                                   Created = (int)shop.Created,
                                   Description = shop.Description,
                                   ID = shop.Shop_ID,
                                   Mall_ID = shop.Mall_Shop_ID,
                                   Synced = (int)shop.Synced,
                                   Title = shop.Name,
                                   Type = l_mtype,
                                   Created_By = new BUser
                                   {
                                       ID = l_user.User_ID,
                                       Mall_ID = l_user.Mall_ID,
                                       Mall_Name = l_user.Mall_Name
                                   }
                               };

                shops = tmpShops.ToList<BShop>();
            }
            
            return shops;
        }
    }
}
