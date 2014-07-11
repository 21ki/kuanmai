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
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Open;
namespace KM.JXC.BL
{
    public class ProductManager : BBaseManager
    {
        private StockManager stockManager = null;

        public ProductManager(BUser user, int shop_id, Permission permission)
            : base(user,shop_id,permission)
        {

            stockManager = new StockManager(user,shop_id,permission);
        }

        public ProductManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            stockManager = new StockManager(user, shop, permission);
        }

        /// <summary>
        /// Input product is Parent product which doesn't have any chind products with Properties combination
        /// </summary>
        /// <param name="product"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public bool CreateProperties(BProduct product,List<BProductProperty> props)
        {
            return this.CreateProperties(this.GetProduct(product.ID), props);
        }

        /// <summary>
        /// Input product is Parent product which doesn't have any chind products with Properties combination
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public bool CreateProperties(int product_id, List<BProductProperty> props)
        {
            return this.CreateProperties(this.GetProduct(product_id), props);
        }

        /// <summary>
        /// Get single product in db Product struct
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product object</returns>
        private Product GetProduct(int id)
        {
            if (id == 0)
            {
                throw new KMJXCException("产品不存在");
            }
            Product product = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                product=(from pdt in db.Product where pdt.Product_ID==id select pdt).FirstOrDefault<Product>();
            }
            return product;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        private bool CreateProperties(Product product, List<BProductProperty> props)
        {
            if (this.CurrentUserPermission.UPDATE_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限更新产品");
            }

            if (product == null || product.Product_ID <= 0)
            {
                throw new KMJXCException("产品不存在");
            }
            bool result = false;

            Product newProduct = new Product();
            newProduct.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            newProduct.Code = product.Code;
            newProduct.Name = product.Name;
            newProduct.Parent_ID = product.Parent_ID;
            newProduct.Price = product.Price;
            newProduct.Product_Class_ID = product.Product_Class_ID;
            newProduct.Product_ID = 0;
            newProduct.Product_Unit_ID = product.Product_Unit_ID;
            newProduct.Shop_ID = product.Shop_ID;
            newProduct.User_ID = this.CurrentUser.ID;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Product.Add(newProduct);
                db.SaveChanges();
                if (newProduct.Product_ID <= 0)
                {
                    throw new KMJXCException("创建属性失败");
                }

                Stock_Pile stockPile = new Stock_Pile();
                stockPile.LastLeave_Time = 0;
                stockPile.Price = 0;
                stockPile.Product_ID = newProduct.Product_ID;
                stockPile.Quantity = 0;
                stockPile.Shop_ID = this.Shop.Shop_ID;
                stockPile.StockHouse_ID = 0;
                stockPile.StockPile_ID = 0;
                this.stockManager.CreateDefaultStockPile(stockPile);

                if (props != null)
                {
                    foreach (BProductProperty p in props)
                    {
                        Product_Specifications ps = new Product_Specifications();
                        ps.User_ID = this.CurrentUser.ID;
                        ps.Created = (int)newProduct.Create_Time;
                        ps.Product_ID = newProduct.Product_ID;
                        ps.Product_Spec_ID = p.PID;
                        ps.Product_Spec_Value_ID = p.PVID;
                        db.Product_Specifications.Add(ps);
                    }

                    db.SaveChanges();
                }

                result = true;
            }
            return result;
        }

        /// <summary>
        /// Create single parent or child product which can contains Properties combination
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public void CreateProduct(BProduct product)
        {
            if (this.CurrentUserPermission.ADD_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限创建产品");
            }
         
            Product dbProduct = new Product();
            dbProduct.Code = product.Code;
            dbProduct.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            dbProduct.Name = product.Title;
            dbProduct.Quantity = 0;
            dbProduct.Description = product.Description;
            if (product.Category != null)
            {
                dbProduct.Product_Class_ID = product.Category.ID;
            }
            dbProduct.Product_ID = 0; 
            dbProduct.Price = product.Price;
            if (product.Unit != null)
            {
                dbProduct.Product_Unit_ID = product.Unit.Product_Unit_ID;
            }
            dbProduct.Shop_ID = this.Shop.Shop_ID;
            dbProduct.User_ID = this.MainUser.ID;

            if (product.Parent != null && product.Parent.ID > 0)
            {
                dbProduct.Parent_ID = product.Parent.ID;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Product.Add(dbProduct);
                db.SaveChanges();
                if (dbProduct.Product_ID <= 0)
                {
                    throw new KMJXCException("产品创建失败");
                }
                product.ID = dbProduct.Product_ID;
                //Update product images
                if (product.Images != null && product.Images.Count > 0)
                {
                    List<int> img_ids = new List<int>();
                    foreach (Image img in product.Images)
                    {
                        img_ids.Add(img.ID);
                    }

                    List<Image> dbImages=(from img in db.Image where img_ids.Contains(img.ID) select img).ToList<Image>();
                    foreach (Image image in dbImages)
                    {
                        image.ProductID = product.ID;
                    }

                    db.SaveChanges();
                }

                Store_House defaultStoreHouse = null;
                List<Store_House> storeHouses = (from h in db.Store_House where (h.Shop_ID == dbProduct.Shop_ID || h.Shop_ID==this.Main_Shop.Shop_ID) select h).ToList<Store_House>();

                if (storeHouses.Count == 0)
                {
                    defaultStoreHouse = new Store_House();
                    defaultStoreHouse.Shop_ID = dbProduct.Shop_ID;
                    defaultStoreHouse.Title = "默认仓库";
                    defaultStoreHouse.User_ID = this.CurrentUser.ID;
                    defaultStoreHouse.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    defaultStoreHouse.Default = true;
                    defaultStoreHouse.Guard = 0;
                    db.Store_House.Add(defaultStoreHouse);
                    db.SaveChanges();
                }
                else
                {
                    defaultStoreHouse = (from h in storeHouses where h.Default == true select h).FirstOrDefault<Store_House>();
                    if (defaultStoreHouse == null)
                    {
                        defaultStoreHouse = storeHouses[0];
                        defaultStoreHouse.Default = true;
                    }
                }

                Stock_Pile stockPile = new Stock_Pile();
                stockPile.LastLeave_Time = 0;
                stockPile.Price = 0;
                stockPile.Product_ID = product.ID;
                stockPile.Quantity = 0;
                stockPile.Shop_ID = dbProduct.Shop_ID;
                stockPile.StockHouse_ID = defaultStoreHouse.StoreHouse_ID;
                stockPile.StockPile_ID = 0;

                db.Stock_Pile.Add(stockPile);

                if (product.Properties != null && product.Properties.Count > 0)
                {                    
                    foreach (BProductProperty pro in product.Properties)
                    {
                        Product_Specifications ps = new Product_Specifications();
                        ps.Product_ID = dbProduct.Product_ID;
                        ps.Product_Spec_ID = pro.PID;
                        ps.Product_Spec_Value_ID = pro.PVID;
                        ps.User_ID = this.CurrentUser.ID;
                        ps.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        db.Product_Specifications.Add(ps);
                    }

                    //db.SaveChanges();
                }

                if (product.Suppliers != null)
                {
                    foreach (Supplier su in product.Suppliers)
                    {
                        Product_Supplier ps = new Product_Supplier();
                        ps.Product_ID = product.ID;
                        ps.Supplier_ID = su.Supplier_ID;
                        db.Product_Supplier.Add(ps);
                    }

                    //db.SaveChanges();
                }

                if (product.Children != null)
                {
                    foreach (BProduct p in product.Children)
                    {
                        if (p.Parent == null)
                        {
                            p.Parent = new BProduct() { ID = product.ID };
                        }
                        p.Children = null;
                        this.CreateProduct(p);
                    }
                }
                else 
                {
                    base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = dbProduct.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.CREATE_PRODUCT }, Description = "" });
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Bath modify products category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="products"></param>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public bool BatchUpdateCategory(int category,int[] products,int shop_id=0)
        {
            bool result=false;
            if (this.CurrentUserPermission.UPDATE_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限执行编辑产品操作");
            }
            if (category <= 0)
            {
                throw new KMJXCException("请选择类目进行批量修改");
            }

            if (products == null || products.Length <= 0)
            {
                throw new KMJXCException("请选择产品，再批量编辑类目");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Product> dbProducts = null;
                var tmpProducts = from p in db.Product
                                  where products.Contains(p.Product_ID)
                                  select p;

                if (shop_id > 0)
                {
                    tmpProducts = tmpProducts.Where(p => p.Shop_ID == shop_id);
                }
                else
                {
                    if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        int[] child_shop_ids = (from c in this.ChildShops select c.ID).ToArray<int>();
                        tmpProducts = tmpProducts.Where(p => (p.Shop_ID == this.Shop.Shop_ID || child_shop_ids.Contains(p.Shop_ID)));
                    }
                    else
                    {                        
                        tmpProducts = tmpProducts.Where(p => (p.Shop_ID==this.Shop.Shop_ID || p.Shop_ID==this.Main_Shop.Shop_ID));
                    }
                }

                dbProducts = tmpProducts.ToList<Product>();
                int[] parent_ids=(from p in dbProducts select p.Product_ID).ToArray<int>();
                List<Product> children=(from p in db.Product where parent_ids.Contains(p.Parent_ID) select p).ToList<Product>();
                foreach (Product pdt in dbProducts)
                {
                    //cannot edit main shop products
                    if (this.Shop.Shop_ID != this.Main_Shop.Shop_ID && pdt.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能修改主店铺产品信息");
                    }

                    List<Product> tchildren=(from p in children where p.Parent_ID==pdt.Product_ID select p).ToList<Product>();
                    foreach (Product child in tchildren)
                    {
                        child.Product_Class_ID = category;
                    }

                    pdt.Product_Class_ID = category;                   
                }

                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Update product information
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public bool UpdateProduct(ref BProduct bproduct)
        {
            BProduct product = bproduct;
            if (this.CurrentUserPermission.UPDATE_PRODUCT == 0)
            {
                throw new KMJXCException("没有权限更新产品");
            }

            bool result = false;
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                Product dbProduct=(from pdt in db.Product where pdt.Product_ID==product.ID select pdt).FirstOrDefault<Product>();
                if (dbProduct == null)
                {
                    throw new KMJXCException("您要修改的产品信息不存在");
                }

                if (this.Shop.Shop_ID != this.Main_Shop.Shop_ID)
                {
                    if (dbProduct.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能修改主店铺产品");
                    }

                    if (dbProduct.Shop_ID != this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能其他主店铺产品");
                    }
                }
                else
                {
                    int[] child_shops=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                    if (dbProduct.Shop_ID != this.Main_Shop.Shop_ID && !child_shops.Contains(dbProduct.Shop_ID))
                    {
                        throw new KMJXCException("您无法修改其他店铺的产品，只能修改主店铺或者子店铺产品");
                    }
                }

                dbProduct.Name = product.Title;
                dbProduct.Description = product.Description;
                if (product.Category != null)
                {
                    dbProduct.Product_Class_ID = product.Category.ID;
                }
                dbProduct.Update_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                dbProduct.Update_User_ID = this.CurrentUser.ID;

                //update images
                if (product.Images!=null && product.Images.Count > 0)
                {
                    List<Image> existedImages=(from img in db.Image where img.ProductID==product.ID select img).ToList<Image>();
                    //Update new uploaded images
                    foreach (Image newimg in product.Images)
                    {
                        Image tmp = (from eted in existedImages where eted.ID == newimg.ID select eted).FirstOrDefault<Image>();
                        if (tmp == null)
                        {
                            Image newone = (from ni in db.Image where ni.ID == newimg.ID select ni).FirstOrDefault<Image>();
                            newone.ProductID = product.ID;
                            db.Image.Add(newone);
                        }
                    }

                    //Remove deleted images
                    string rootPath = product.FileRootPath;  
                    foreach (Image oldImg in existedImages)
                    {

                        Image tmp = (from eted in product.Images where eted.ID == oldImg.ID select eted).FirstOrDefault<Image>();
                        if (tmp == null)
                        {
                            db.Image.Remove(oldImg);
                            if (rootPath != null && System.IO.File.Exists(rootPath + oldImg.Path))
                            {
                                System.IO.File.Delete(rootPath + oldImg.Path);
                            }
                        }
                    }
                }                

                //update suppliers
                if (product.Suppliers != null) 
                {
                    foreach (Supplier s in product.Suppliers)
                    {
                        Product_Supplier ps = new Product_Supplier() { Product_ID = product.ID, Supplier_ID = s.Supplier_ID, Enabled=true,Created=DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),Created_By=this.CurrentUser.ID };
                        db.Product_Supplier.Add(ps);
                    }
                }

                //update children
                List<Product> children=(from p in db.Product where p.Parent_ID==dbProduct.Product_ID select p).ToList<Product>();
                foreach (Product child in children)
                {
                    child.Name = dbProduct.Name;
                    child.Product_Class_ID = dbProduct.Product_Class_ID;
                    child.Description = dbProduct.Description;
                }

                db.SaveChanges();

                if (product.Children != null && product.Children.Count > 0)
                {
                    foreach (BProduct child in product.Children)
                    {
                        if (child.ID == 0)
                        {
                            //create new child product with properties
                            child.Parent = product;
                            child.Children = null;
                            this.CreateProduct(child);
                        }
                        else
                        {
                            //Update properties
                            if (child.Properties != null && child.Properties.Count > 0)
                            {
                                List<Product_Specifications> properties = (from prop in db.Product_Specifications
                                                                           where prop.Product_ID == child.ID
                                                                           select prop).ToList<Product_Specifications>();

                                List<Product_Specifications> newProps = new List<Product_Specifications>();
                                if (properties.Count > 0)
                                {
                                    //current just support edit existed property's value, doesn't support deleting property
                                    foreach (BProductProperty p in child.Properties)
                                    {
                                        Product_Specifications psprop = (from ep in properties where ep.Product_Spec_ID == p.PID  select ep).FirstOrDefault<Product_Specifications>();
                                        if (psprop == null)
                                        {
                                            //cretae new property for existed product
                                            psprop = new Product_Specifications() { Product_ID=child.ID, Product_Spec_ID=p.PID, Product_Spec_Value_ID=p.PVID, Created=DateTimeUtil.ConvertDateTimeToInt(DateTime.Now), User_ID=this.CurrentUser.ID };
                                            db.Product_Specifications.Add(psprop);
                                        }
                                        else 
                                        {
                                            //update existed property's value
                                            if (psprop.Product_Spec_Value_ID != p.PVID)
                                            {
                                                psprop.Product_Spec_Value_ID = p.PVID;
                                            }
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                }


                bproduct = this.GetProductFullInfo(product.ID);

                base.CreateActionLog(new BUserActionLog() { Shop = new BShop { ID = bproduct.Shop.Shop_ID }, Action = new BUserAction() { Action_ID = UserLogAction.UPDATE_PRODUCT }, Description = "商品编号:" + bproduct.ID+ "\n商品名称:" + bproduct.Title });
                result = true;

            }
            catch (KMJXCException kex)
            {
                throw kex;
            }
            catch
            {
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }

        /// <summary>
        /// Remove properties for parent product or child product
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public bool RemoveProductProperties(int product_id, List<BProductProperty> props)
        {
            Product dbProduct = this.GetProduct(product_id);
            if (dbProduct == null)
            {
                throw new KMJXCException("产品不存在");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if(props!=null && props.Count>0)
                {
                    List<Product_Specifications> specs = (from ps in db.Product_Specifications where ps.Product_ID == dbProduct.Product_ID select ps).ToList<Product_Specifications>();
                    foreach (BProductProperty prop in props)
                    {
                        Product_Specifications ps = (from sp in specs where sp.Product_Spec_ID == prop.PID && sp.Product_Spec_Value_ID == prop.PVID && sp.Product_ID==product_id select sp).FirstOrDefault<Product_Specifications>();
                        if (ps != null)
                        {
                            db.Product_Specifications.Remove(ps);
                        }
                    }
                    db.SaveChanges();
                }
            }

            return true;
        }
        
        /// <summary>
        /// Add properties for parent product or child product
        /// </summary>
        /// <param name="product_id"></param>
        /// <param name="props"></param>
        /// <returns></returns>
        public bool AddProductProperties(int product_id, List<BProductProperty> props)
        {
            Product dbProduct = this.GetProduct(product_id);
            if (dbProduct == null)
            {
                throw new KMJXCException("产品不存在");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                if (props != null && props.Count > 0)
                {
                    List<Product_Specifications> specs = (from ps in db.Product_Specifications where ps.Product_ID == dbProduct.Product_ID select ps).ToList<Product_Specifications>();
                    foreach (BProductProperty prop in props)
                    {
                        Product_Specifications ps = (from sp in specs where sp.Product_Spec_ID == prop.PID && sp.Product_Spec_Value_ID == prop.PVID && sp.Product_ID==product_id select sp).FirstOrDefault<Product_Specifications>();
                        if (ps == null)
                        {
                            ps = new Product_Specifications();
                            ps.Product_Spec_Value_ID = prop.PVID;
                            ps.Product_Spec_ID = prop.PID;
                            ps.Product_ID = product_id;
                            db.Product_Specifications.Add(ps);
                        }
                    }
                    db.SaveChanges();
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<BProduct> SearchProducts(int[] product_ids,int[] suppliers,string title, string description, int startTime, int endTime, int? category_id, int pageIndex, int pageSize, out int total,bool includeProps=false,bool paging=true,bool includeSupplier=false)
        {
            total = 0;
            List<BProduct> products = new List<BProduct>();
            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {

                int[] childshop_ids = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                var dbps = from product in db.Product
                           //join employee in db.Employee on product.User_ID equals employee.User_ID                           
                           where product.Parent_ID == 0 
                           //&& (product.Shop_ID == this.Shop.Shop_ID || product.Shop_ID == this.Main_Shop.Shop_ID || childshop_ids.Contains(product.Shop_ID))
                           select new
                           {
                               Pdt=product,
                               //Emp=employee,                              
                           };

                if (product_ids != null && product_ids.Length > 0)
                {
                    dbps = dbps.Where(a => product_ids.Contains(a.Pdt.Product_ID));
                }

                if (childshop_ids != null && childshop_ids.Length > 0)
                {
                    dbps = dbps.Where(a => a.Pdt.Shop_ID == this.Shop.Shop_ID || a.Pdt.Shop_ID == this.Main_Shop.Shop_ID || childshop_ids.Contains(a.Pdt.Shop_ID));
                }
                else 
                {
                    dbps = dbps.Where(a => a.Pdt.Shop_ID == this.Shop.Shop_ID || a.Pdt.Shop_ID == this.Main_Shop.Shop_ID);
                }
                if (!string.IsNullOrEmpty(title))
                {
                    dbps = dbps.Where(a=>a.Pdt.Name.Contains(title.Trim()));
                }

                if (!string.IsNullOrEmpty(description))
                {
                    dbps = dbps.Where(a => a.Pdt.Description.Contains(description));
                }

                if (startTime > 0)
                {
                    dbps = dbps.Where(a => a.Pdt.Create_Time >= startTime);
                }

                if (endTime > 0)
                {
                    dbps = dbps.Where(a => a.Pdt.Create_Time <= endTime);
                }

                if (suppliers != null && suppliers.Length > 0)
                {
                    //int[] pdtIds=(from ps in db.Product_Supplier where suppliers.Contains(ps.Supplier_ID) && ps.Enabled==true select ps.Product_ID).ToArray<int>();
                    var pdtIds = from ps in db.Product_Supplier where suppliers.Contains(ps.Supplier_ID) && ps.Enabled == true orderby ps.Created descending select ps.Product_ID;
                    dbps = dbps.Where(a => pdtIds.Contains(a.Pdt.Product_ID));
                }

                if (category_id !=null)
                {
                    Product_Class cate = (from ca in db.Product_Class where ca.Product_Class_ID == category_id select ca).FirstOrDefault<Product_Class>();
                    if (cate != null)
                    {
                        if (cate.Parent_ID == 0)
                        {
                            int[] ccids = (from c in db.Product_Class where c.Parent_ID == category_id select c.Product_Class_ID).ToArray<int>();
                            if (ccids != null && ccids.Length > 0)
                            {
                                dbps = dbps.Where(a => (ccids.Contains(a.Pdt.Product_Class_ID) || a.Pdt.Product_Class_ID==category_id));
                            }
                            else
                            {
                                dbps = dbps.Where(a => a.Pdt.Product_Class_ID == category_id);  
                            }
                        }
                        else 
                        {
                            dbps = dbps.Where(a => a.Pdt.Product_Class_ID == category_id);                            
                        }
                    }
                }
                dbps = dbps.OrderBy(a=>a.Pdt.Shop_ID).OrderBy(b=>b.Pdt.Create_Time);
                total = dbps.Count();
                if (total > 0)
                {
                    var bps = from bpss in dbps                             
                              select new BProduct
                              {
                                  Description = bpss.Pdt.Description,
                                  Shop=(from sp in db.Shop where sp.Shop_ID==bpss.Pdt.Shop_ID select sp).FirstOrDefault<Shop>(),
                                  Price = bpss.Pdt.Price,
                                  ID = bpss.Pdt.Product_ID,
                                  Title = bpss.Pdt.Name,
                                  CreateTime = bpss.Pdt.Create_Time,
                                  Code = bpss.Pdt.Code,                                
                                  Unit = (from u in db.Product_Unit where u.Product_Unit_ID == bpss.Pdt.Product_Unit_ID select u).FirstOrDefault<Product_Unit>(),
                                  Category = (from c in db.Product_Class
                                              where bpss.Pdt.Product_Class_ID == c.Product_Class_ID
                                              select new BCategory
                                              {
                                                  Name = c.Name,
                                                  ID = c.Product_Class_ID,                                                 
                                              }).FirstOrDefault<BCategory>(),
                                  User = (from u in db.User
                                          where u.User_ID == bpss.Pdt.User_ID
                                          select new BUser
                                          {
                                              ID = u.User_ID,
                                              Mall_Name = u.Mall_Name,
                                              Mall_ID = u.Mall_ID,                                             
                                          }).FirstOrDefault<BUser>()
                              };

                    if (paging)
                    {
                        products = bps.OrderBy(a => a.ID).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList<BProduct>();
                    }
                    else
                    {
                        products = bps.OrderBy(a => a.ID).ToList<BProduct>();
                    }
                }

                List<BProduct> childProducts = null;
                List<BProductProperty> properties = null;
                if (includeProps)
                {
                    int[] parent_ids = (from p in products select p.ID).ToArray<int>();
                    var tmpchildProducts = from p in db.Product
                                           join stock in db.Stock_Pile on p.Product_ID equals stock.Product_ID into LStock
                                           from l_stock in LStock.DefaultIfEmpty()
                                           where parent_ids.Contains(p.Parent_ID)
                                           select new BProduct
                                           {
                                               ID = p.Product_ID,
                                               Title = p.Name,
                                               Quantity = l_stock != null ? l_stock.Quantity : 0,
                                               ParentID=p.Parent_ID
                                           };

                    childProducts = tmpchildProducts.ToList<BProduct>();
                    int[] child_product_ids = (from c in childProducts select c.ID).ToArray<int>();
                    properties = (from pv in db.Product_Specifications
                                                         join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                                                         from l_prop in LProp.DefaultIfEmpty()
                                                         join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                                                         from l_propv in LPropv.DefaultIfEmpty()
                                                         where child_product_ids.Contains(pv.Product_ID)
                                                         select new BProductProperty
                                                         {
                                                             PID = pv.Product_Spec_ID,
                                                             PName = l_prop.Name,
                                                             ProductID = pv.Product_ID,
                                                             PValue = l_propv.Name,
                                                             PVID = pv.Product_Spec_Value_ID
                                                         }).ToList<BProductProperty>();
                }

                List<BProductSupplier> bsuppliers = new List<BProductSupplier>();

                if (includeSupplier)
                {
                    int[] parent_ids = (from p in products select p.ID).ToArray<int>();
                    bsuppliers = (from s in db.Product_Supplier
                                  join supplier in db.Supplier on s.Supplier_ID equals supplier.Supplier_ID into LSupplier
                                  from l_supplier in LSupplier.DefaultIfEmpty()
                                  where parent_ids.Contains(s.Product_ID)
                                  select new BProductSupplier
                                  {
                                      Supplier = new BSupplier { ID = s.Supplier_ID, Name = l_supplier.Name },
                                      Product = new BProduct { ID=s.Product_ID }
                                  }).ToList<BProductSupplier>();
                }

                foreach (BProduct product in products)
                {
                    if (product.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        product.FromMainShop = true;
                    }else if (childshop_ids != null && childshop_ids.Length > 0 && childshop_ids.Contains(product.Shop.Shop_ID))
                    {
                        product.FromChildShop = true;
                    }

                    if (includeProps)
                    {
                        List<BProduct> children = (from c in childProducts where c.ParentID == product.ID select c).ToList<BProduct>();
                        if (children.Count > 0)
                        {
                            product.Children = new List<BProduct>();
                            foreach (BProduct child in children)
                            {
                                child.Properties = (from prop in properties where prop.ProductID == child.ID select prop).ToList<BProductProperty>();
                                product.Children.Add(child);
                            }
                        }
                    }

                    if (includeSupplier)
                    {
                        product.Suppliers = (from ps in bsuppliers where ps.Product.ID == product.ID select new Supplier { Supplier_ID=ps.Supplier.ID, Name=ps.Supplier.Name }).ToList<Supplier>();
                    }
                }
            }

            return products;
        }

        /// <summary>
        /// Gets full local product information
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public BProduct GetProductFullInfo(int productId,string mall_id=null)
        {
            
            if (productId == 0 && string.IsNullOrEmpty(mall_id))
            {
                throw new KMJXCException("获取产品详细信息时候，必须输入产品编号或者已关联的商城宝贝编号");
            }

            BProduct product = null;
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                if (productId == 0 && !string.IsNullOrEmpty(mall_id))
                {
                    productId=(from mp in db.Mall_Product where mp.Mall_ID==mall_id select mp.Outer_ID).FirstOrDefault<int>();
                }

                if (productId == 0)
                {
                    throw new KMJXCException("获取产品详细信息时候，必须输入产品编号或者已关联的商城宝贝编号");
                }

                product = (from pudt in db.Product
                           where pudt.Product_ID == productId
                           select new BProduct
                           {
                               Shop = (from sp in db.Shop where sp.Shop_ID == pudt.Shop_ID select sp).FirstOrDefault<Shop>(),
                               ID = pudt.Product_ID,
                               Description = pudt.Description,
                               Title = pudt.Name,
                               Code = pudt.Code,
                               CreateTime = pudt.Create_Time,                              
                               Category = (from c in db.Product_Class
                                           where pudt.Product_Class_ID == c.Product_Class_ID
                                           select new BCategory
                                           {
                                               Name = c.Name,
                                               ID = c.Product_Class_ID,
                                               ParentID=(int)c.Parent_ID
                                           }).FirstOrDefault<BCategory>(),
                               User = (from u in db.User
                                       where u.User_ID == pudt.User_ID
                                       select new BUser
                                       {
                                           ID = u.User_ID,
                                           Mall_Name = u.Mall_Name,
                                           Mall_ID = u.Mall_ID,
                                       }).FirstOrDefault<BUser>()
                             
                           }).FirstOrDefault<BProduct>();

                if (product != null)
                {
                    product.Images = (from img in db.Image where img.ProductID == product.ID select img).ToList<Image>();
                    product.Suppliers = (from sp in db.Supplier
                                         join ps in db.Product_Supplier on sp.Supplier_ID equals ps.Supplier_ID
                                         where ps.Product_ID == product.ID && ps.Enabled==true
                                         select sp
                                      ).ToList<Supplier>();
                   
                    //product.Properties = properties;
                    List<Product> children = (from p in db.Product where p.Parent_ID == product.ID select p).ToList<Product>();
                    int[] children_ids=(from c in children select c.Product_ID).ToArray<int>();
                    List<BProductProperty> properties = (from pp in db.Product_Specifications
                                                         where children_ids.Contains(pp.Product_ID)
                                                         select new BProductProperty
                                                         {
                                                             ProductID=pp.Product_ID,
                                                             PID = pp.Product_Spec_ID,
                                                             PName = (from prop in db.Product_Spec where prop.Product_Spec_ID == pp.Product_Spec_ID select prop.Name).FirstOrDefault<string>(),
                                                             PVID = pp.Product_Spec_Value_ID,
                                                             PValue = (from propv in db.Product_Spec_Value where propv.Product_Spec_Value_ID == pp.Product_Spec_Value_ID select propv.Name).FirstOrDefault<string>()
                                                         }).ToList<BProductProperty>();

                    if (children != null && children.Count > 0)
                    {
                        if (product.Children == null)
                        {
                            product.Children = new List<BProduct>();
                        }
                        foreach (Product pdt in children)
                        {
                            BProduct child = new BProduct() {  ID=pdt.Product_ID,Title=product.Title};
                            child.Properties=(from prop in properties where prop.ProductID==child.ID select prop).ToList<BProductProperty>();
                            product.Children.Add(child);
                        }
                    }
                }
            }
            catch (KMJXCException kex)
            {
                throw kex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                db.Dispose();
            }
            return product;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public List<BProduct> GetProductProperties(int productId)
        {
            List<BProduct> props = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Product product = (from p in db.Product where p.Product_ID == productId select p).FirstOrDefault<Product>();
                if (product == null)
                {
                    throw new KMJXCException("编号为:" + productId + " 的产品不存在");
                }

                var mall_id = from m in db.Mall_Product
                              where m.Outer_ID == productId
                              select m.Mall_ID;

                var children_id = from sku in db.Mall_Product_Sku
                                  where mall_id.Contains(sku.Mall_ID) && sku.Outer_ID>0
                                  select sku.Outer_ID;

                props=(from p in db.Product
                                  where p.Parent_ID == productId && !children_id.Contains(p.Product_ID)
                                  select new BProduct
                                  {
                                     ID=p.Product_ID,
                                     Title=p.Name                
                                  }).ToList<BProduct>();

                if (props == null || props.Count <= 0)
                {
                    throw new KMJXCException("编号为:" + productId + " 的产品不存在销售属性");
                }

                int[] tmpProducts = (from p in props select p.ID).ToArray<int>();

                List<BProductProperty> properties = new List<BProductProperty>();
                properties = (from pv in db.Product_Specifications
                         join prop in db.Product_Spec on pv.Product_Spec_ID equals prop.Product_Spec_ID into LProp
                         from l_prop in LProp.DefaultIfEmpty()
                         join propV in db.Product_Spec_Value on pv.Product_Spec_Value_ID equals propV.Product_Spec_Value_ID into LPropv
                         from l_propv in LPropv.DefaultIfEmpty()
                         where tmpProducts.Contains(pv.Product_ID)
                         select new BProductProperty
                         {                             
                             PID = pv.Product_Spec_ID,
                             PName = l_prop.Name,
                             ProductID = pv.Product_ID,
                             PValue = l_propv.Name,
                             PVID = pv.Product_Spec_Value_ID
                         }).ToList<BProductProperty>();

                foreach (BProduct prop in props)
                {
                    prop.Properties=(from prp in properties where prp.ProductID==prop.ID select prp).ToList<BProductProperty>();
                }
            }

            return props;
        }
    }
}
