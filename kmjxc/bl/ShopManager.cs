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
using KM.JXC.BL.Open.TaoBao;
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
                    throw new KMJXCException("您要添加的子店铺(" + child_shop_name + ")信息不存在，请先使用子店铺的主账户登录进销存，然后在执行添加子店铺操作");
                }

                if (shop.Parent_Shop_ID > 0) 
                {
                    Shop mainshop = (from sp in db.Shop where sp.Shop_ID==shop.Parent_Shop_ID select sp).FirstOrDefault<Shop>();
                    if (mainshop != null) 
                    {
                        throw new KMJXCException(child_shop_name+" 已经是 "+mainshop.Name+" 的子店铺，不能重复添加或者添加为别的店铺的子店铺");
                    }
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
                    scr = new Shop_Child_Request();
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
                                      Modified = (int)request.Modified,
                                      Status=(int)request.Status
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
                                      Modified = (int)request.Modified,
                                      Status=(int)request.Status
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
                int[] csp_ids=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                var tmp = from cus in db.Customer
                          join mtype in db.Mall_Type on cus.Mall_Type_ID equals mtype.Mall_Type_ID
                          join shop_cus in db.Customer_Shop on cus.Customer_ID equals shop_cus.Customer_ID
                          join shop in db.Shop on shop_cus.Shop_ID equals shop.Shop_ID
                          where shop_cus.Shop_ID == this.Shop.Shop_ID || csp_ids.Contains(shop_cus.Shop_ID)

                          select new BCustomer
                          {
                              ID = cus.Customer_ID,
                              Address = cus.Address,
                              Mall_ID = cus.Mall_ID,
                              Mall_Name = cus.Mall_Name,
                              Type = new BMallType {  ID=mtype.Mall_Type_ID,Name=mtype.Name,Description=mtype.Description},
                              Email = cus.Email,
                              Phone = cus.Phone,
                              Name = cus.Name,
                              Shop = new BShop
                              {
                                  Title = shop.Name,
                                  ID = shop.Shop_ID,
                                  Mall_ID = shop.Mall_Shop_ID,
                                  Type = mtype,
                              }
                          };
                tmp = tmp.OrderBy(c => c.Shop.ID);
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
                int[] child_shop=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
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

                int[] childs=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    tmp = tmp.Where(e => (e.Shop.ID == this.Shop.Shop_ID || childs.Contains(e.Shop.ID)));
                }
                else
                {
                    tmp = tmp.Where(e => (e.Shop.ID == this.Shop.Shop_ID));
                }

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
                int[] child_shop = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
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
                int[] child_shop=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
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
        /// Search child shops
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

        /// <summary>
        /// Gets shop sub accounts
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BUser> SearchShopUsers(int page,int pageSize,out long total,int shop_id=0)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            List<BUser> users = new List<BUser>();
            total = 0;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from shop_user in db.User
                          select shop_user;

                if (shop_id == 0)
                {
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        int[] childs = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                        tmp = tmp.Where(u => (u.Shop_ID == this.Shop.Shop_ID || childs.Contains(u.Shop_ID)));
                    }
                    else
                    {
                        tmp = tmp.Where(u => u.Shop_ID == this.Shop.Shop_ID);
                    }
                }
                else
                {
                    tmp = tmp.Where(u => u.Shop_ID == shop_id);
                }

                var tmpUser = from user in tmp
                              join shop in db.Shop on user.Shop_ID equals shop.Shop_ID into lShop
                              from l_shop in lShop.DefaultIfEmpty()
                              join mtype in db.Mall_Type on user.Mall_Type equals mtype.Mall_Type_ID into lType
                              from l_mtype in lType.DefaultIfEmpty()
                              join employee in db.Employee on user.User_ID equals employee.User_ID into lEmployee
                              from l_employee in lEmployee.DefaultIfEmpty()
                              join parent in db.User on user.Parent_User_ID equals parent.User_ID into lParent
                              from l_parent in lParent.DefaultIfEmpty()
                              select new BUser
                              {
                                  Created = (int)user.Created,
                                  Mall_ID = user.Mall_ID,
                                  Mall_Name = user.Mall_Name,
                                  ID = user.User_ID,
                                  Name = user.Name,
                                  Type = l_mtype != null ? new BMallType
                                  {
                                      ID = l_mtype.Mall_Type_ID,
                                      Name = l_mtype.Name,
                                      Description = l_mtype.Description
                                  } : new BMallType { ID = 0, Name = "", Description = "" },
                                  EmployeeInfo = l_employee != null ? new BEmployee
                                  {
                                      ID = l_employee.Employee_ID,
                                      Address = l_employee.Address,
                                      User_ID = user.User_ID,
                                      Phone = l_employee.Phone,
                                      MatureDate = (int)l_employee.MatureDate,
                                      IdentityCard = l_employee.IdentityCard,
                                      HireDate = (int)l_employee.HireDate,
                                      Gendar = l_employee.Gendar,
                                      Email = l_employee.Email,
                                      Duty = l_employee.Duty,
                                      Department = l_employee.Department,
                                      BirthDate = (int)l_employee.BirthDate,
                                      Name = l_employee.Name
                                  } : new BEmployee
                                  {
                                      ID = 0,
                                      Address = "",
                                      User_ID = 0,
                                      Phone = "",
                                      MatureDate = 0,
                                      IdentityCard = "",
                                      HireDate = 0,
                                      Gendar = "",
                                      Email = "",
                                      Duty = "",
                                      Department = "",
                                      BirthDate = 0,
                                      Name = ""
                                  },                                 
                                  Shop = l_shop != null ?
                                  new BShop
                                  {
                                      ID = l_shop.Shop_ID,
                                      Mall_ID = l_shop.Mall_Shop_ID,
                                      Title = l_shop.Name
                                  } :
                                  new BShop
                                  {
                                      ID = 0,
                                      Mall_ID = "",
                                      Title = ""
                                  }
                              };

                total = tmpUser.Count();
                if (total > 0)
                {
                    users = tmpUser.OrderBy(u => u.Shop.ID).OrderBy(u=>u.ID).Skip((page - 1) * pageSize).Take(pageSize).ToList<BUser>();
                }
            }

            return users;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shop_id"></param>
        /// <param name="syncType">0-sync products</param>
        /// <returns></returns>
        public BMallSync GetMallSync(int shop_id = 0,int syncType=0)
        {
            BMallSync sync = null;
            int shopID = this.Shop.Shop_ID;
            if (shop_id > 0)
            {
                shopID = shop_id;
            }
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                sync = (from s in db.SyncWithMall
                        join shop in db.Shop on s.Shop_ID equals shop.Shop_ID into LShop
                        from l_shop in LShop.DefaultIfEmpty()
                        join user in db.User on s.User_ID equals user.User_ID into LUser
                        from l_user in LUser.DefaultIfEmpty()
                        where s.SyncType == syncType && s.Shop_ID == shopID
                        orderby s.SyncTime descending
                        select new BMallSync
                        {
                            ID = s.ID,
                            SyncTime = s.SyncTime,
                            Shop = l_shop != null ?
                            new BShop
                            {
                                ID = l_shop.Shop_ID,
                                Title = l_shop.Name
                            } :
                            new BShop
                            {
                                ID = 0,
                                Title = ""
                            }
                            ,
                            User = l_user != null ? new BUser
                            {
                                ID=l_user.User_ID,
                                Mall_ID=l_user.Mall_ID,
                                Mall_Name=l_user.Mall_Name
                            } : new BUser {
                                ID = 0,
                                Mall_ID = "",
                                Mall_Name = ""
                            }
                        }).FirstOrDefault<BMallSync>();
            }
            return sync;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mallProduct_ids"></param>
        /// <param name="mapProduct"></param>
        public void CreateProductsByMallProducts(string[] mallProduct_ids, bool mapProduct = false)
        {
            if (mallProduct_ids==null || mallProduct_ids.Length<=0)
            {
                throw new KMJXCException("请选择在售宝贝来创建进销存产品");
            }
            List<BMallProduct> products = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var tmp = from item in db.Mall_Product
                          where mallProduct_ids.Contains(item.Mall_ID)
                          select item;

                string[] mall_ids=(from t in tmp select t.Mall_ID).ToArray<string>();

                var tmp1 = from sku in db.Mall_Product_Sku
                           where mall_ids.Contains(sku.Mall_ID)
                           select sku;

                List<Mall_Product_Sku> skus = tmp1.OrderBy(i => i.Mall_ID).ToList<Mall_Product_Sku>();

                var tmpPdts = from item in tmp
                              join product in db.Product on item.Outer_ID equals product.Product_ID into lProduct
                              from l_product in lProduct.DefaultIfEmpty()
                              join shop in db.Shop on item.Shop_ID equals shop.Shop_ID into lShop
                              from l_shop in lShop.DefaultIfEmpty()
                              join category in db.Product_Class on l_product.Product_Class_ID equals category.Product_Class_ID into lCategory
                              from l_category in lCategory.DefaultIfEmpty()
                              select new BMallProduct
                              {
                                  Created = (int)item.Created,
                                  Description = item.Description,
                                  ID = item.Mall_ID,
                                  Modified = (int)item.Modified,
                                  OuterID = item.Outer_ID,
                                  PicUrl = item.PicUrl,
                                  Price = (double)item.Price,
                                  Product = new BProduct
                                  {
                                      Title = l_product.Name,
                                      Category = l_category != null ? new BCategory { ID = l_category.Product_Class_ID, Name = l_category.Name } : new BCategory { ID = 0, Name = "" }
                                  },
                                  Quantity = (long)item.Quantity,
                                  Shop = new BShop { ID = l_shop.Shop_ID, Title = l_shop.Name },
                                  Title = item.Title,
                                  Synced = (long)item.Synced,
                                  FirstSync = (long)item.FirstSync,
                                  HasProductCreated = (bool)item.CreatedProduct
                              };

                products = tmpPdts.OrderByDescending(p => p.ID).ToList<BMallProduct>();
                foreach (BMallProduct product in products)
                {
                    product.Skus = (from sku in skus
                                    where sku.Mall_ID == product.ID
                                    select new BMallSku
                                    {
                                        MallProduct_ID = sku.Mall_ID,
                                        OuterID = sku.Outer_ID,
                                        SkuID = sku.SKU_ID,
                                        Price = (double)sku.Price,
                                        Quantity = (int)sku.Quantity,                                       
                                        Properities = sku.Properties,
                                        PropertiesName = sku.Properties_name
                                    }).ToList<BMallSku>();
                }

                if (products.Count > 0)
                {
                    this.CreateProductsByMallProducts(products, mapProduct);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="products"></param>
        /// <param name="mapProduct"></param>
        private void CreateProductsByMallProducts(List<BMallProduct> products,bool mapProduct=false)
        {
            if (products == null || products.Count == 0)
            {
                return;
            }

            List<BProperty> properties = new List<BProperty>();
            int shop_id = products[0].Shop.ID;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {                
                string[] dbMallPdtIds=(from p in products select p.ID).ToArray<string>();
                List<Mall_Product> dbMallProcuts = (from p in db.Mall_Product where dbMallPdtIds.Contains(p.Mall_ID) select p).ToList<Mall_Product>();
                List<Product> dbProducts = new List<Product>();
                List<Product_Spec> existedProperties = null;
                var tmpProperties = from p in db.Product_Spec
                                   select p;
                var tmpDbProducts = from p in db.Product
                                    where p.Parent_ID == 0
                                    select p;
                if (shop_id == this.Main_Shop.Shop_ID)
                {
                    int[] child_shop_ids = (from c in this.ChildShops select c.ID).ToArray<int>();
                    tmpProperties = tmpProperties.Where(p => (child_shop_ids.Contains(p.Shop_ID) || p.Shop_ID == shop_id));
                    tmpDbProducts = tmpDbProducts.Where(p => (child_shop_ids.Contains(p.Shop_ID) || p.Shop_ID == shop_id));
                }
                else
                {
                    tmpProperties = tmpProperties.Where(p =>(p.Shop_ID == shop_id || p.Shop_ID==this.Main_Shop.Shop_ID));
                    tmpDbProducts = tmpDbProducts.Where(p => (p.Shop_ID == shop_id || p.Shop_ID == this.Main_Shop.Shop_ID));
                }
                dbProducts = tmpDbProducts.ToList<Product>();
                existedProperties = tmpProperties.ToList<Product_Spec>();
                int[] prop_ids=(from prop in existedProperties select prop.Product_Spec_ID).ToArray<int>();
                List<Product_Spec_Value> existedPropValues=(from pv in db.Product_Spec_Value where prop_ids.Contains(pv.Product_Spec_ID) select pv).ToList<Product_Spec_Value>();

                Store_House defaultStoreHouse = null;
                List<Store_House> storeHouses=(from h in db.Store_House where h.Shop_ID==shop_id select h).ToList<Store_House>();

                if (storeHouses.Count == 0)
                {
                    defaultStoreHouse = new Store_House();
                    defaultStoreHouse.Shop_ID = shop_id;
                    defaultStoreHouse.Title = "默认仓库";
                    defaultStoreHouse.User_ID = this.CurrentUser.ID;
                    defaultStoreHouse.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    defaultStoreHouse.Default = true;
                    db.Store_House.Add(defaultStoreHouse);
                    db.SaveChanges();
                }
                else
                {
                    defaultStoreHouse= (from h in storeHouses where h.Default==true select h).FirstOrDefault<Store_House>();
                    if (defaultStoreHouse == null)
                    {
                        defaultStoreHouse = storeHouses[0];
                        defaultStoreHouse.Default = true;
                    }
                }

                foreach (BMallProduct product in products)
                {
                    Mall_Product dbMallProduct=(from p in dbMallProcuts where p.Mall_ID==product.ID select p).FirstOrDefault<Mall_Product>();
                    Product dbProduct = (from p in dbProducts where p.MallProduct == product.ID select p).FirstOrDefault<Product>();
                    if (dbProduct != null)
                    {
                        continue;
                    }
                    dbProduct = new Product();
                    dbProduct.Code = product.Code;
                    dbProduct.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbProduct.Description = dbProduct.Description;
                    dbProduct.MallProduct = product.ID;
                    dbProduct.Name = product.Title;
                    dbProduct.Parent_ID = 0;
                    dbProduct.Price = product.Price;
                    dbProduct.Product_Class_ID = 0;
                    //dbProduct.Quantity = (int)product.Quantity;
                    dbProduct.Shop_ID = product.Shop.ID;
                    dbProduct.Update_Time = dbProduct.Create_Time;
                    dbProduct.Update_User_ID = this.CurrentUser.ID;
                    dbProduct.User_ID = this.CurrentUser.ID;
                    dbProduct.Wastage = 0;
                    db.Product.Add(dbProduct);
                    db.SaveChanges();
                    if (dbProduct.Product_ID <= 0)
                    {
                        continue;
                    }

                    Stock_Pile stockPile = new Stock_Pile();
                    stockPile.LastLeave_Time = 0;
                    stockPile.Price = 0;
                    stockPile.Product_ID = dbProduct.Product_ID;
                    stockPile.Quantity = 0;
                    stockPile.Shop_ID = product.Shop.ID;
                    stockPile.StockHouse_ID = defaultStoreHouse.StoreHouse_ID;
                    stockPile.StockPile_ID = 0;

                    db.Stock_Pile.Add(stockPile);

                    if (product.Skus != null)
                    {
                        foreach (BMallSku sku in product.Skus)
                        {
                            Product dbChildProduct = new Product();
                            dbChildProduct.Code = product.Code;
                            dbChildProduct.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                            dbChildProduct.Description = dbProduct.Description;
                            dbChildProduct.MallProduct = sku.SkuID;
                            dbChildProduct.Name = product.Title;
                            dbChildProduct.Parent_ID = dbProduct.Product_ID;
                            dbChildProduct.Price = product.Price;
                            dbChildProduct.Product_Class_ID = 0;
                            //dbChildProduct.Quantity = (int)sku.Quantity;
                            dbChildProduct.Shop_ID = product.Shop.ID;
                            dbChildProduct.Update_Time = dbChildProduct.Create_Time;
                            dbChildProduct.Update_User_ID = this.CurrentUser.ID;
                            dbChildProduct.User_ID = this.CurrentUser.ID;
                            dbChildProduct.Wastage = 0;
                            db.Product.Add(dbChildProduct);   
                            db.SaveChanges();

                            if (dbChildProduct.Product_ID <= 0)
                            {
                                continue;
                            }

                            if (string.IsNullOrEmpty(sku.PropertiesName))
                            {
                                continue;
                            }

                            Stock_Pile skustockPile = new Stock_Pile();
                            skustockPile.LastLeave_Time = 0;
                            skustockPile.Price = 0;
                            skustockPile.Product_ID = dbChildProduct.Product_ID;
                            skustockPile.Quantity = 0;
                            skustockPile.Shop_ID = product.Shop.ID;
                            skustockPile.StockHouse_ID = defaultStoreHouse.StoreHouse_ID;
                            skustockPile.StockPile_ID = 0;

                            db.Stock_Pile.Add(skustockPile);

                            string[] props = sku.PropertiesName.Split(';');
                            foreach (string prop in props)
                            {
                                string[] values = prop.Split(':');
                                Product_Spec property = (from p in existedProperties where (p.Mall_PID == values[0] || p.Name==values[2]) select p).FirstOrDefault<Product_Spec>();
                                if (property == null)
                                {
                                    property=new Product_Spec();
                                    property.Name = values[2];
                                    property.Mall_PID = values[0];
                                    property.Shop_ID = shop_id;
                                    property.User_ID = this.CurrentUser.ID;
                                    property.Product_Class_ID = 0;                                    
                                    property.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                                    db.Product_Spec.Add(property);
                                    db.SaveChanges();
                                    if (property.Product_Spec_ID > 0)
                                    {
                                        existedProperties.Add(property);
                                    }
                                }

                                Product_Spec_Value propValue=(from v in existedPropValues where (v.Mall_PVID==values[2] || v.Name==values[3]) select v ).FirstOrDefault<Product_Spec_Value>();
                                if(propValue==null)
                                {
                                    propValue=new Product_Spec_Value(){ Product_Spec_ID=property.Product_Spec_ID, Mall_PVID=values[1], Name=values[3], Created=DateTimeUtil.ConvertDateTimeToInt(DateTime.Now), User_ID=this.CurrentUser.ID};
                                    db.Product_Spec_Value.Add(propValue);
                                    db.SaveChanges();
                                    if (propValue.Product_Spec_Value_ID > 0)
                                    {
                                        existedPropValues.Add(propValue);
                                    }
                                }

                                if (property.Product_Spec_ID > 0 && propValue.Product_Spec_Value_ID > 0)
                                {
                                    Product_Specifications ps = new Product_Specifications();
                                    ps.Product_ID = dbChildProduct.Product_ID;
                                    ps.Product_Spec_ID = property.Product_Spec_ID;
                                    ps.Product_Spec_Value_ID = propValue.Product_Spec_Value_ID;
                                    db.Product_Specifications.Add(ps);
                                }
                            }

                            //db.SaveChanges();
                        }
                    }

                    dbMallProduct.CreatedProduct = true;                   
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Sync onsale products to local database
        /// </summary>
        /// <returns></returns>
        public List<BMallProduct> SyncMallOnSaleProducts(int shop_id=0,bool create_product=false,bool mapProduct=false)
        {
            List<BMallProduct> newProducts = new List<BMallProduct>();
            List<BMallProduct> products = new List<BMallProduct>();
            IOProductManager productManager = new TaobaoProductManager(this.AccessToken,this.Shop.Mall_Type_ID);
            List<BMallProduct> newUnMappedProducts = new List<BMallProduct>();
            long total = 0;
            long page = 1;
            long pageSize = 40;
            List<BMallProduct> tmp = new List<BMallProduct>();
            Shop shop = this.Shop;
            if (shop_id > 0)
            {
                shop = new Shop() { Shop_ID=shop_id };
            }
            tmp = productManager.GetOnSaleProducts(this.CurrentUser, shop, page, pageSize, out total);

            if (tmp != null)
            {
                products = products.Concat(tmp).ToList<BMallProduct>();
            }

            long totalPage = 1;

            if (total > pageSize)
            {
                if (total % pageSize == 0)
                {
                    totalPage = total / pageSize;
                }
                else
                {
                    totalPage = total / pageSize + 1 ;                    
                }
            }

            if (totalPage > 1)
            {                
                page++;
                while (page <= totalPage)
                {
                    tmp = productManager.GetOnSaleProducts(this.CurrentUser, this.Shop, page, pageSize, out total);
                    products = products.Concat(tmp).ToList<BMallProduct>();
                    page++;
                }
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Product> locProducts=(from locPdt in db.Product where locPdt.Shop_ID==this.Shop.Shop_ID select locPdt).ToList<Product>();

                List<Mall_Product> dbMallProducts=(from pdt in db.Mall_Product
                                                   where pdt.Shop_ID==this.Shop.Shop_ID
                                                   select pdt).ToList<Mall_Product>();

                List<Mall_Product_Sku> dbSkus = (from sku in db.Mall_Product_Sku
                                                 where sku.Shop_ID == this.Shop.Shop_ID
                                                 select sku).ToList<Mall_Product_Sku>();

                foreach (BMallProduct product in products)
                {
                    Mall_Product dbProduct=(from dbPdt in dbMallProducts where dbPdt.Mall_ID==product.ID select dbPdt).FirstOrDefault<Mall_Product>();
                    bool isNew = false;
                    if (dbProduct == null)
                    {
                        isNew = true;
                        dbProduct = new Mall_Product();
                        dbProduct.CreatedProduct = false;
                        dbProduct.FirstSync = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    }

                    Product mappedLocProduct=(from p in locProducts where p.Product_ID==product.OuterID select p).FirstOrDefault<Product>();
                    if (mappedLocProduct == null)
                    {
                        product.OuterID = 0;
                    }

                    dbProduct.Created = product.Created;
                    dbProduct.Description = product.Description;
                    dbProduct.Mall_ID = product.ID;
                    dbProduct.Modified = product.Modified;
                    dbProduct.Outer_ID = product.OuterID;
                    dbProduct.PicUrl = product.PicUrl;
                    dbProduct.Price = product.Price;
                    dbProduct.Quantity = (int)product.Quantity;
                    dbProduct.Shop_ID = this.Shop.Shop_ID;
                    
                    if (product.Shop != null)
                    {
                        dbProduct.Shop_ID = product.Shop.ID;
                    }
                    else {
                        product.Shop = new BShop { ID=this.Shop.Shop_ID };
                    }

                    dbProduct.Synced = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    dbProduct.Title = product.Title;

                    if (isNew)
                    {
                        db.Mall_Product.Add(dbProduct);
                        newProducts.Add(product);
                        if (mappedLocProduct == null)
                        {
                            newUnMappedProducts.Add(product);
                        }
                    }

                    if (product.Skus!=null)
                    {
                        foreach (BMallSku mSku in product.Skus)
                        {
                            bool skuNew = false;
                            Mall_Product_Sku dbSku=(from dbs in dbSkus where mSku.SkuID== dbs.SKU_ID select dbs).FirstOrDefault<Mall_Product_Sku>();
                            if (dbSku == null)
                            {
                                skuNew = true;
                                dbSku = new Mall_Product_Sku();
                            }

                            Product childLocProduct=(from p in locProducts where p.Product_ID==mSku.OuterID select p).FirstOrDefault<Product>();
                            if (childLocProduct == null)
                            {
                                mSku.OuterID = 0;
                            }

                            dbSku.Outer_ID = mSku.OuterID;
                            dbSku.Mall_ID = mSku.MallProduct_ID;
                            dbSku.Price = mSku.Price;
                            dbSku.Properties = mSku.Properities;
                            dbSku.Properties_name = mSku.PropertiesName;
                            dbSku.Quantity = (int)mSku.Quantity;
                            dbSku.Shop_ID = product.Shop.ID;
                            dbSku.SKU_ID = mSku.SkuID;
                            if (skuNew)
                            {
                                db.Mall_Product_Sku.Add(dbSku);
                            }
                        }
                    }
                }

                SyncWithMall sync = new SyncWithMall();
                sync.Shop_ID = this.Shop.Shop_ID;
                sync.User_ID = this.CurrentUser.ID;
                sync.SyncType = 0;//宝贝同步
                sync.SyncTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                db.SyncWithMall.Add(sync);
                db.SaveChanges();

                if (create_product)
                {
                    this.CreateProductsByMallProducts(newUnMappedProducts,mapProduct);
                }
            }

            return newProducts;
        }

        /// <summary>
        /// Search on sale products
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <param name="connected"></param>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BMallProduct> SearchOnSaleMallProducts(string productName,int page, int pageSize, out int total,bool? connected=null, int shop_id = 0)
        {
            List<BMallProduct> products = new List<BMallProduct>();
            total = 0;
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
                var tmp = from item in db.Mall_Product
                          select item;

                var tmp1 = from sku in db.Mall_Product_Sku
                           select sku;

                if (shop_id > 0)
                {
                    tmp = tmp.Where(i => i.Shop_ID == shop_id);
                    tmp1 = tmp1.Where(i => i.Shop_ID == shop_id);
                }
                else
                {
                    int[] childs=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        tmp = tmp.Where(i => (i.Shop_ID == this.Shop.Shop_ID || childs.Contains(i.Shop_ID)));
                        tmp1 = tmp1.Where(i => (i.Shop_ID == this.Shop.Shop_ID || childs.Contains(i.Shop_ID)));
                    }
                    else
                    {
                        tmp = tmp.Where(i => i.Shop_ID == this.Shop.Shop_ID);
                        tmp1 = tmp1.Where(i => i.Shop_ID == this.Shop.Shop_ID);
                    }
                }

                if (connected != null)
                {
                    if (connected == true)
                    {
                        tmp = tmp.Where(i => i.Outer_ID > 0);
                    }
                    else
                    {
                        tmp = tmp.Where(i => i.Outer_ID == 0);
                    }
                }

                if (!string.IsNullOrEmpty(productName))
                {
                    tmp = tmp.Where(p=>p.Title.Contains(productName.Trim()));
                }
                List<Mall_Product_Sku> skus = tmp1.OrderBy(i=>i.Mall_ID).ToList<Mall_Product_Sku>();

                var tmpPdts = from item in tmp
                              join product in db.Product on item.Outer_ID equals product.Product_ID into lProduct
                              from l_product in lProduct.DefaultIfEmpty()
                              join shop in db.Shop on item.Shop_ID equals shop.Shop_ID into lShop
                              from l_shop in lShop.DefaultIfEmpty()
                              join category in db.Product_Class on l_product.Product_Class_ID equals category.Product_Class_ID into lCategory
                              from l_category in lCategory.DefaultIfEmpty()
                              select new BMallProduct
                              {
                                  Created = (int)item.Created,
                                  Description = item.Description,
                                  ID = item.Mall_ID,
                                  Modified = (int)item.Modified,
                                  OuterID = item.Outer_ID,
                                  PicUrl = item.PicUrl,
                                  Price = (double)item.Price,
                                  Product = new BProduct
                                  {
                                      Title = l_product.Name,
                                      Category = l_category != null ? new BCategory { ID=l_category.Product_Class_ID,Name=l_category.Name } : new BCategory {ID=0,Name="" }
                                  },
                                  Quantity = (long)item.Quantity,
                                  Shop = new BShop { ID = l_shop.Shop_ID, Title = l_shop.Name },
                                  Title = item.Title,
                                  Synced=(long)item.Synced,
                                  FirstSync=(long)item.FirstSync,
                                  HasProductCreated=(bool)item.CreatedProduct
                              };

                total = tmpPdts.Count();
                if (total > 0)
                {
                    products = tmpPdts.OrderByDescending(p => p.FirstSync).OrderBy(p => p.Shop.ID).Skip((page - 1) * pageSize).Take(pageSize).ToList<BMallProduct>();
                    foreach (BMallProduct product in products)
                    {
                        product.Skus = (from sku in skus
                                        where sku.Mall_ID == product.ID
                                        select new BMallSku
                                        {
                                            MallProduct_ID = sku.Mall_ID,
                                            OuterID = sku.Outer_ID,
                                            SkuID = sku.SKU_ID,
                                            Price = (double)sku.Price,
                                            Quantity = (int)sku.Quantity,
                                            //Product = new BProduct 
                                            //{

                                            //},
                                            Properities = sku.Properties,
                                            PropertiesName = sku.Properties_name
                                        }).ToList<BMallSku>();
                    }
                }
                            
            }
            return products;
        }

        /// <summary>
        /// Gets shop statistic data
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public BShopStatistic GetShopStatistic(int shop_id = 0,bool containChildShop=false)
        {
            BShopStatistic statistic = new BShopStatistic();
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                int[] child=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (containChildShop && this.Shop.Shop_ID==this.Main_Shop.Shop_ID)
                {
                    statistic.ChildShop = this.DBChildShops.Count;

                    statistic.Account = (from user in db.User where user.Shop_ID == this.Shop.Shop_ID || child.Contains(user.Shop_ID) select user.User_ID).Count();
                    statistic.BackSale = (from bs in db.Back_Sale where bs.Shop_ID == this.Shop.Shop_ID || child.Contains(bs.Shop_ID) select bs.Back_Sale_ID).Count();

                    var tmpbs = from bsd in db.Back_Sale_Detail
                                join bs in db.Back_Sale on bsd.Back_Sale_ID equals bs.Back_Sale_ID into lBs
                                from l_bs in lBs.DefaultIfEmpty()
                                where (l_bs.Shop_ID == this.Shop_Id || child.Contains(l_bs.Shop_ID)) && bsd.Status == 0
                                select bsd.Back_Sale_ID;
                    statistic.BackSaleUnhandled = tmpbs.Distinct().Count();

                    statistic.BackStock = (from bs in db.Back_Stock where bs.Shop_ID == this.Shop.Shop_ID || child.Contains(bs.Shop_ID) select bs.Back_Sock_ID).Count();
                    var tmpbs1 = from bsd in db.Back_Stock_Detail
                                 join bs in db.Back_Stock on bsd.Back_Stock_ID equals bs.Back_Sock_ID into lBs
                                 from l_bs in lBs.DefaultIfEmpty()
                                 where (l_bs.Shop_ID == this.Shop_Id || child.Contains(l_bs.Shop_ID)) && bsd.Status == 0
                                 select bsd.Back_Stock_ID;
                    statistic.BackStockUnhandled = tmpbs1.Distinct().Count();

                    statistic.Buy = (from buy in db.Buy where buy.Shop_ID == this.Shop.Shop_ID || child.Contains(buy.Shop_ID) select buy.Buy_ID).Count();
                    var tmpbuy = from bsd in db.Buy_Detail
                                 join bs in db.Buy on bsd.Buy_ID equals bs.Buy_ID into lBs
                                 from l_bs in lBs.DefaultIfEmpty()
                                 where (l_bs.Shop_ID == this.Shop_Id || child.Contains(l_bs.Shop_ID)) && l_bs.Status == 0
                                 select bsd.Buy_ID;
                    statistic.BuyUnhandled = tmpbuy.Distinct().Count();

                    statistic.BuyOrder = (from order in db.Buy_Order where order.Shop_ID == this.Shop.Shop_ID || child.Contains(order.Shop_ID) select order.Buy_Order_ID).Count();
                    var tmpbuyorder = from bsd in db.Buy_Order_Detail
                                      join bs in db.Buy_Order on bsd.Buy_Order_ID equals bs.Buy_Order_ID into lBs
                                      from l_bs in lBs.DefaultIfEmpty()
                                      where (l_bs.Shop_ID == this.Shop_Id || child.Contains(l_bs.Shop_ID)) && l_bs.Status == 0
                                      select bsd.Buy_Order_ID;
                    statistic.BuyOrderUnhandled = tmpbuyorder.Distinct().Count();

                    statistic.Product = (from pdt in db.Product where pdt.Parent_ID == 0 & (pdt.Shop_ID == this.Shop.Shop_ID || child.Contains(pdt.Shop_ID)) select pdt.Product_ID).Count();

                    statistic.OnSaleProduct = (from pdt in db.Mall_Product where pdt.Shop_ID == this.Shop.Shop_ID || child.Contains(pdt.Shop_ID) select pdt.Mall_ID).Count();

                    statistic.OnSaleProductNotConnected = (from pdt in db.Mall_Product where (pdt.Shop_ID == this.Shop.Shop_ID || child.Contains(pdt.Shop_ID)) && pdt.Outer_ID == 0 select pdt.Mall_ID).Count();
                    statistic.Trade = (from t in db.Sale where t.Shop_ID==this.Shop.Shop_ID || child.Contains(t.Shop_ID) select t).Count();
                }
                else
                {
                    int shop = this.Shop.Shop_ID;
                    if (shop_id > 0)
                    {
                        shop = shop_id;
                    }

                    statistic.Account = (from user in db.User where user.Shop_ID == shop select user.User_ID).Count();
                    statistic.BackSale = (from bs in db.Back_Sale where bs.Shop_ID == shop select bs.Back_Sale_ID).Count();

                    var tmpbs = from bsd in db.Back_Sale_Detail
                                join bs in db.Back_Sale on bsd.Back_Sale_ID equals bs.Back_Sale_ID into lBs
                                from l_bs in lBs.DefaultIfEmpty()
                                where (l_bs.Shop_ID == shop) && bsd.Status == 0
                                select bsd.Back_Sale_ID;
                    statistic.BackSaleUnhandled = tmpbs.Distinct().Count();

                    statistic.BackStock = (from bs in db.Back_Stock where bs.Shop_ID == shop select bs.Back_Sock_ID).Count();
                    var tmpbs1 = from bsd in db.Back_Stock_Detail
                                 join bs in db.Back_Stock on bsd.Back_Stock_ID equals bs.Back_Sock_ID into lBs
                                 from l_bs in lBs.DefaultIfEmpty()
                                 where (l_bs.Shop_ID == shop) && bsd.Status == 0
                                 select bsd.Back_Stock_ID;
                    statistic.BackStockUnhandled = tmpbs1.Distinct().Count();

                    statistic.Buy = (from buy in db.Buy where buy.Shop_ID ==shop select buy.Buy_ID).Count();
                    var tmpbuy = from bsd in db.Buy_Detail
                                 join bs in db.Buy on bsd.Buy_ID equals bs.Buy_ID into lBs
                                 from l_bs in lBs.DefaultIfEmpty()
                                 where (l_bs.Shop_ID == shop) && l_bs.Status == 0
                                 select bsd.Buy_ID;
                    statistic.BuyUnhandled = tmpbuy.Distinct().Count();

                    statistic.BuyOrder = (from order in db.Buy_Order where order.Shop_ID == shop select order.Buy_Order_ID).Count();
                    var tmpbuyorder = from bsd in db.Buy_Order_Detail
                                      join bs in db.Buy_Order on bsd.Buy_Order_ID equals bs.Buy_Order_ID into lBs
                                      from l_bs in lBs.DefaultIfEmpty()
                                      where (l_bs.Shop_ID == shop) && l_bs.Status == 0
                                      select bsd.Buy_Order_ID;
                    statistic.BuyOrderUnhandled = tmpbuyorder.Distinct().Count();

                    statistic.Product = (from pdt in db.Product where pdt.Parent_ID == 0 & (pdt.Shop_ID == shop) select pdt.Product_ID).Count();

                    statistic.OnSaleProduct = (from pdt in db.Mall_Product where pdt.Shop_ID == shop select pdt.Mall_ID).Count();

                    statistic.OnSaleProductNotConnected = (from pdt in db.Mall_Product where (pdt.Shop_ID == shop) && pdt.Outer_ID == 0 select pdt.Mall_ID).Count();

                    statistic.Trade=(from t in db.Sale where t.Shop_ID==shop select t).Count();
                }
            }
            return statistic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mall_id"></param>
        /// <param name="locProductId"></param>
        /// <returns></returns>
        public bool MapMallProduct(string mall_id, int locProductId)
        {
            bool result = false;
            if (string.IsNullOrEmpty(mall_id))
            {
                return result;
            }

            if (locProductId <= 0)
            {
                return result;
            }

            Product locProduct = null;
            IOProductManager productManager = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                locProduct = (from p in db.Product where p.Product_ID == locProductId && p.Parent_ID == 0 select p).FirstOrDefault<Product>();
                if (locProduct == null)
                {
                    throw new KMJXCException("本地产品不存在，无法关联到商城宝贝");
                }

                Mall_Product locMallProduct = (from mp in db.Mall_Product where mp.Mall_ID == mall_id select mp).FirstOrDefault<Mall_Product>();
                if (locMallProduct == null)
                {
                    throw new KMJXCException("宝贝信息不存在，请先同步出售中的宝贝");
                }

                Access_Token token = this.AccessToken;
                Shop desshop = this.Shop;
                //current user is main shop user
                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    int[] child_shop_ids = (from c in this.ChildShops select c.ID).ToArray<int>();
                    if (!child_shop_ids.Contains(locMallProduct.Shop_ID))
                    {
                        throw new KMJXCException("不能关联他人店铺的宝贝，请不要尝试此操作");
                    }
                    else
                    {
                        desshop = (from s in this.ChildShops
                                   where s.ID == locMallProduct.Shop_ID
                                   select new Shop
                                   {
                                       Shop_ID = s.ID,
                                       Name = s.Title,
                                       Mall_Type_ID = s.Type!=null?s.Type.Mall_Type_ID:0
                                   }).FirstOrDefault<Shop>();
                        User shopUser = (from user in db.User
                                         from shop in db.Shop
                                         where shop.Shop_ID == locMallProduct.Shop_ID && user.User_ID == shop.User_ID
                                         select user).FirstOrDefault<User>();

                        if (desshop == null || shopUser == null)
                        {
                            throw new KMJXCException("请退出系统，用子店铺账户从商城授权登录进销存");
                        }
                        if (desshop.Mall_Type_ID == 0)
                        {
                            throw new KMJXCException("请退出系统，重新登录");
                        }
                        token = (from t in db.Access_Token where t.User_ID==shopUser.User_ID select t).FirstOrDefault<Access_Token>();
                        if (token == null)
                        {
                            throw new KMJXCException("子账户授权已经过期，请退出系统，用子店铺账户从商城授权登录进销存");
                        }

                        if (IsTokenExpired(token))
                        {
                            throw new KMJXCException("子账户授权已经过期，请退出系统，用子店铺账户从商城授权登录进销存");
                        }
                    }
                }
                else 
                {
                    if (locMallProduct.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能关联主店铺宝贝");
                    }

                    if (locMallProduct.Shop_ID != this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能关联他人店铺的宝贝，请不要尝试此操作");
                    }
                }

                productManager = new TaobaoProductManager(token, desshop.Mall_Type_ID);

                result = productManager.MappingProduct(locProductId.ToString(), mall_id);
                if (result)
                {
                    locMallProduct.Outer_ID = locProductId;
                }

                db.SaveChanges();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skuId"></param>
        /// <param name="locProductId"></param>
        /// <returns></returns>
        public bool MapSku(string skuId,int locProductId)
        {
            bool result = false;
            if (string.IsNullOrEmpty(skuId))
            {
                return result;
            }

            if (locProductId <= 0)
            {
                return result;
            }
            Product locProduct = null;
            Access_Token token = this.AccessToken;
            Shop desshop = this.Shop;
            IOProductManager productManager = new TaobaoProductManager(this.AccessToken, this.Shop.Mall_Type_ID);
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                locProduct = (from p in db.Product where p.Product_ID == locProductId && p.Parent_ID > 0 select p).FirstOrDefault<Product>();
                if (locProduct == null)
                {
                    throw new KMJXCException("本地库存属性不存在，无法关联到商城宝贝的SKU");
                }

                Mall_Product_Sku msku = null;
                msku = (from sk in db.Mall_Product_Sku where sk.SKU_ID == skuId select sk).FirstOrDefault<Mall_Product_Sku>();
                if (msku == null)
                {
                    throw new KMJXCException("未找到SKU ID 对应的商城宝贝ID");
                }

                Mall_Product_Sku existed=(from esku in db.Mall_Product_Sku where esku.Mall_ID==msku.Mall_ID && esku.Outer_ID==locProductId select esku).FirstOrDefault<Mall_Product_Sku>();
                if (existed != null)
                {
                    throw new KMJXCException("所选库存属性已经被关联到此宝贝下其他的SKU，请选择其他的库存属性进行关联");
                }

                Mall_Product locMallProduct=(from p in db.Mall_Product where p.Mall_ID==msku.Mall_ID select p).FirstOrDefault<Mall_Product>();
                if (locMallProduct == null)
                {
                    throw new KMJXCException("此SKU所对应的宝贝快照不存在，请先同步在售宝贝");
                }
                //current user is main shop user
                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    int[] child_shop_ids = (from c in this.ChildShops select c.ID).ToArray<int>();
                    if (!child_shop_ids.Contains(locMallProduct.Shop_ID))
                    {
                        throw new KMJXCException("不能关联他人店铺的宝贝，请不要尝试此操作");
                    }
                    else
                    {
                        desshop = (from s in this.ChildShops
                                   where s.ID == locMallProduct.Shop_ID
                                   select new Shop
                                   {
                                       Shop_ID = s.ID,
                                       Name = s.Title,
                                       Mall_Type_ID=s.Type.Mall_Type_ID
                                   }).FirstOrDefault<Shop>();
                        User shopUser = (from user in db.User
                                         from shop in db.Shop
                                         where shop.Shop_ID == locMallProduct.Shop_ID && user.User_ID == shop.User_ID
                                         select user).FirstOrDefault<User>();

                        if (desshop == null || shopUser == null)
                        {
                            throw new KMJXCException("请退出系统，用子店铺账户从商城授权登录进销存");
                        }

                        token = (from t in db.Access_Token where t.User_ID == shopUser.User_ID select t).FirstOrDefault<Access_Token>();
                        if (token == null)
                        {
                            throw new KMJXCException("子账户授权已经过期，请退出系统，用子店铺账户从商城授权登录进销存");
                        }

                        if (IsTokenExpired(token))
                        {
                            throw new KMJXCException("子账户授权已经过期，请退出系统，用子店铺账户从商城授权登录进销存");
                        }
                    }
                }
                else
                {
                    if (locMallProduct.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能关联主店铺宝贝");
                    }

                    if (locMallProduct.Shop_ID != this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("不能关联他人店铺的宝贝，请不要尝试此操作");
                    }
                }

                productManager = new TaobaoProductManager(token, desshop.Mall_Type_ID);

                result = productManager.MappingSku(locProductId.ToString(), skuId, msku.Mall_ID, msku.Properties);
                if (result)
                {
                    msku.Outer_ID = locProductId;
                }

                db.SaveChanges();
            }
            return result;
        }
    }
}
