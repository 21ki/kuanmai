using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Models;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;

namespace KM.JXC.BL
{
    public class ShopCategoryManager:BBaseManager
    {
        IOShopManager mallShopManager = null;
        public ShopCategoryManager(BUser user, int shop_id, Permission permission)
            : base(user,shop_id,permission)
        {
            mallShopManager = new TaoBaoShopManager(this.AccessToken, this.Shop.Mall_Type_ID);
            
        }

        public ShopCategoryManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            mallShopManager = new TaoBaoShopManager(this.AccessToken, this.Shop.Mall_Type_ID);
        }

        /// <summary>
        /// Get single category information
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public BCategory GetCategory(int category_id)
        {
            BCategory cate = null;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                cate = this.GetCategory(category_id, db);
            }
            return cate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="category_id"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private BCategory GetCategory(int category_id,KuanMaiEntities db)
        {
            BCategory category = null;
            try
            {
                Product_Class ca = (from pc in db.Product_Class where pc.Product_Class_ID == category_id select pc).FirstOrDefault<Product_Class>();
                if (ca != null)
                {
                    category = new BCategory();
                    category.ID = ca.Product_Class_ID;
                    category.Mall_ID = ca.Mall_CID;
                    category.Mall_PID = ca.Mall_PCID;
                    category.Name = ca.Name;
                    category.Order = (int)ca.Order;
                    if (ca.Parent_ID <= 0)
                    {
                        category.Parent = null;
                    }
                    category.Enabled = ca.Enabled;
                    category.Created = ca.Create_Time;
                    category.Chindren = new List<BCategory>();
                    List<Product_Class> children = (from pc in db.Product_Class where pc.Parent_ID == category_id select pc).ToList<Product_Class>();
                    if (children.Count > 0)
                    {
                        foreach (Product_Class productclass in children)
                        {
                            BCategory category1 = new BCategory();
                            category1.ID = ca.Product_Class_ID;
                            category1.Mall_ID = ca.Mall_CID;
                            category1.Mall_PID = ca.Mall_PCID;
                            category1.Name = ca.Name;
                            category1.Order = (int)ca.Order;
                            category1.Enabled = ca.Enabled;
                            category1.Created = ca.Create_Time;
                            category.Chindren.Add(category1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }

            return category;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        public List<BCategory> GetCategories(int parentId,bool fromMailShop=false)
        {
            List<BCategory> categories = new List<BCategory>();          
            
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Product_Class> allpcs=null;
                if (!fromMailShop)
                {
                    allpcs = (from pc in db.Product_Class
                              where pc.Shop_ID == this.Shop.Shop_ID
                              select pc).ToList<Product_Class>();
                }
                else
                {
                    allpcs = (from pc in db.Product_Class
                              where pc.Shop_ID == this.Main_Shop.Shop_ID
                              select pc).ToList<Product_Class>();
                }

               List<Product_Class> pcs = (from p in allpcs where p.Parent_ID == parentId select p).ToList<Product_Class>();

               foreach (Product_Class ca in pcs)
               {
                   BCategory category = new BCategory();                 
                   category.ID = ca.Product_Class_ID;
                   category.Mall_ID = ca.Mall_CID;
                   category.Mall_PID = ca.Mall_PCID;
                   category.Name = ca.Name;
                   category.Order = (int)ca.Order;
                   category.Enabled = ca.Enabled;
                   category.Created = ca.Create_Time;
                   category.Chindren = (from cate in allpcs
                                        where cate.Parent_ID == ca.Product_Class_ID
                                        select new BCategory
                                        {
                                            ID = cate.Product_Class_ID,
                                            Mall_ID = cate.Mall_CID,
                                            Mall_PID = cate.Mall_PCID,
                                            Name = cate.Name,
                                            Order = (int)cate.Order,
                                            Enabled = cate.Enabled,
                                            Created = cate.Create_Time
                                        }).ToList<BCategory>();
                   if (ca.Parent_ID > 0)
                   {
                       category.Parent = (from cate in db.Product_Class
                                          where cate.Product_Class_ID == ca.Parent_ID
                                          select new BCategory
                                          {                                              
                                              Created = cate.Create_Time,
                                              Enabled = cate.Enabled,
                                              ID = cate.Product_Class_ID,
                                              Name = cate.Name,
                                              Mall_ID = cate.Mall_CID,
                                              Mall_PID = cate.Mall_PCID,                                              
                                          }).FirstOrDefault<BCategory>();
                   }

                   category.Create_By = (from u in db.User
                                         where u.User_ID == ca.Create_User_ID
                                         select new BUser 
                                         {
                                             ID=u.User_ID,
                                             Name=u.Name,
                                             Mall_ID=u.Mall_ID,
                                             Mall_Name=u.Mall_Name,
                                             EmployeeInfo =(from em in db.Employee where em.User_ID==ca.Create_User_ID select em).FirstOrDefault<Employee>()
                                         }).FirstOrDefault<BUser>();
                   categories.Add(category);
               }
            }
            
            return categories;
        }

        /// <summary>
        /// Get all enabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns>Product_Class list</returns>
        public List<BCategory> GetEnabledCategories(int parentId)
        {
            return (from c in GetCategories(parentId) where c.Enabled == true select c).ToList<BCategory>();
        }

        /// <summary>
        /// Get all disabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BCategory> GetDisabledCategories(int parentId)
        {
            return (from c in GetCategories(parentId) where c.Enabled == false select c).ToList<BCategory>();
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <param name="category">Product_Class object</param>
        /// <returns></returns>
        public bool CreateCategory(BCategory category)
        {
            bool result = false;
            if (category == null)
            {
                throw new KMJXCException("输入错误");
            }

            if (string.IsNullOrEmpty(category.Name))
            {
                throw new KMJXCException("类目名不能为空");
            }

            if (this.CurrentUserPermission.ADD_PRODUCT_CLASS == 0)
            {
                throw new KMJXCException("没有权限添加类目");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Product_Class existed=(from ca in db.Product_Class where ca.Name.ToLower()==category.Name.ToLower() && ca.Shop_ID==this.Main_Shop.Shop_ID select ca).FirstOrDefault<Product_Class>();
                if (existed != null)
                {
                    throw new KMJXCException("名为"+category.Name+"的类目已经存在");
                }

                Product_Class pc = new Product_Class();
                pc.Create_Time = category.Created;
                pc.Create_User_ID = this.CurrentUser.ID;
                pc.Enabled = true;
                pc.Mall_CID = category.Mall_ID;
                pc.Mall_PCID = category.Mall_PID;
                pc.Name = category.Name;
                pc.Order = category.Order;
                pc.Shop_ID = this.Main_Shop.Shop_ID;
                if (category.Parent == null || category.Parent.ID <= 0)
                {
                    pc.Parent_ID = 0;
                }
                else {
                    pc.Parent_ID = category.Parent.ID;
                }
                db.Product_Class.Add(pc);
                db.SaveChanges();
                category.ID = pc.Product_Class_ID;
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Update single category
        /// </summary>
        /// <param name="category">Product_Class object</param>
        /// <returns></returns>
        public bool UpdateCategory(BCategory category)
        {
            bool result = false;
            if (category == null)
            {
                throw new KMJXCException("输入错误");
            }

            if (string.IsNullOrEmpty(category.Name))
            {
                throw new KMJXCException("类目名不能为空");
            }

            if (this.CurrentUserPermission.UPDATE_PRODUCT_CLASS == 0)
            {
                throw new KMJXCException("没有权限更新类目");
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Product_Class oldCategory = (from pc in db.Product_Class where pc.Product_Class_ID == category.ID select pc).ToList<Product_Class>()[0];
                oldCategory.Name = category.Name;
                oldCategory.Enabled = category.Enabled;
                if (category.Parent != null)
                {
                    oldCategory.Parent_ID = category.Parent.ID;
                }
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BCategory> GetOnlineCategories()
        {
            List<BCategory> categories = null;

            List<Product_Class> classes= mallShopManager.GetCategories(this.MainUser);
            if (classes != null && classes.Count > 0)
            {
                List<Product_Class> parents = (from p in classes where p.Mall_PCID == "0" select p).ToList<Product_Class>();
                if (parents != null && parents.Count > 0)
                {
                    foreach (Product_Class pc in parents)
                    {
                        BCategory cate = new BCategory();
                        cate.Name = pc.Name;
                        cate.Mall_ID = pc.Mall_CID;
                        cate.Mall_PID = pc.Mall_PCID;
                        List<Product_Class> children = (from c in classes where c.Mall_PCID == pc.Mall_PCID select c).ToList<Product_Class>();
                        if (children != null && children.Count > 0)
                        {
                            cate.Chindren = new List<BCategory>();
                            foreach (Product_Class childCate in children)
                            {
                                BCategory child = new BCategory();
                                child.Mall_ID = childCate.Mall_CID;
                                child.Mall_PID = childCate.Mall_PCID;
                                child.Name = childCate.Name;
                                cate.Chindren.Add(child);
                            }                            
                        }
                    }
                }
            }
            return categories;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BProperty> GetOnlineProperties()
        {
            List<BProperty> properties = mallShopManager.GetProperities(new Product_Class() { Product_Class_ID=0,Create_User_ID=this.CurrentUser.ID,Shop_ID=this.Main_Shop.Shop_ID}, this.Main_Shop);
            return properties;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyId"></param>
        /// <param name="propName"></param>
        /// <param name="propValues"></param>
        /// <returns></returns>
        public bool UpdateProperty(int propertyId, string propName, List<string> propValues)
        {
            bool result = false;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="propName"></param>
        /// <param name="propValues"></param>
        /// <returns></returns>
        public BProperty CreateProperty(int categoryId,string propName,List<string> propValues)
        {
            BProperty bproperty = null;
            if (string.IsNullOrEmpty(propName))
            {
                throw new KMJXCException("属性名称不能为空");
            }
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                Product_Spec property = new Product_Spec();
                property.Product_Class_ID = categoryId;
                property.Name = propName;
                property.User_ID = this.CurrentUser.ID;
                property.Shop_ID = this.Shop.Shop_ID;
                property.Mall_PID = "";
                property.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                db.Product_Spec.Add(property);
                db.SaveChanges();
                if (property.Product_Class_ID <= 0)
                {
                    throw new KMJXCException("产品属性创建失败");
                }
                bproperty = new BProperty();
                bproperty.ID = property.Product_Spec_ID;
                bproperty.Created_By = this.CurrentUser;
                bproperty.Created = (int)property.Created;
                bproperty.CategoryId = categoryId;
                bproperty.MID = "";
                bproperty.Name = propName;               

                if (propValues != null)
                {
                    if (bproperty.Values == null)
                    {
                        bproperty.Values = new List<Product_Spec_Value>();
                    }
                    foreach (string v in propValues)
                    {
                        Product_Spec_Value psv = new Product_Spec_Value();
                        psv.Mall_PVID = "";
                        psv.Name = v;
                        psv.Product_Spec_ID = property.Product_Spec_ID;
                        psv.Product_Spec_Value_ID = 0;
                        psv.User_ID = this.CurrentUser.ID;
                        db.Product_Spec_Value.Add(psv);
                        db.SaveChanges();
                        bproperty.Values.Add(psv);
                    }
                }


            }
            catch
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return bproperty;
        }

        public BProperty GetProperty(int propId)
        {
            BProperty prop = null;

            return prop;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<BProperty> GetProperties(int categoryId,bool fromMainShop=false)
        {
            List<BProperty> properties = new List<BProperty>();
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                if (!fromMainShop)
                {
                    var props = from prop in db.Product_Spec where prop.Shop_ID == this.Shop.Shop_ID select prop;
                    if (categoryId > 0)
                    {
                        props = props.Where(a => a.Product_Class_ID == categoryId);
                    }

                    properties = (from p in props
                                  select new BProperty
                                  {
                                      CategoryId = categoryId,
                                      Created = (int)p.Created,
                                      Created_By = (from u in db.User where u.User_ID == p.User_ID select new BUser { }).FirstOrDefault<BUser>(),
                                      ID = p.Product_Spec_ID,
                                      MID = p.Mall_PID,
                                      Name = p.Name,
                                      Values = (from ps in db.Product_Spec_Value where ps.Product_Spec_ID == p.Product_Spec_ID select ps).ToList<Product_Spec_Value>()
                                  }).ToList<BProperty>();
                }
                else
                {
                    var props = from prop in db.Product_Spec where prop.Shop_ID == this.Main_Shop.Shop_ID select prop;
                    if (categoryId > 0)
                    {
                        props = props.Where(a => a.Product_Class_ID == categoryId);
                    }

                    properties = (from p in props
                                  select new BProperty
                                  {
                                      CategoryId = categoryId,
                                      Created = (int)p.Created,
                                      Created_By = (from u in db.User where u.User_ID == p.User_ID select new BUser { }).FirstOrDefault<BUser>(),
                                      ID = p.Product_Spec_ID,
                                      MID = p.Mall_PID,
                                      Name = p.Name,
                                      Values = (from ps in db.Product_Spec_Value where ps.Product_Spec_ID == p.Product_Spec_ID select ps).ToList<Product_Spec_Value>()
                                  }).ToList<BProperty>();
                }

            }
            catch
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }
            return properties;
        }
    }
}
