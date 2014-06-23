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
                    category.Children = new List<BCategory>();
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
                            category.Children.Add(category1);
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
        public List<BCategory> GetCategories(int? parentId,bool fromMailShop=false)
        {
            List<BCategory> categories = new List<BCategory>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var allpcs = (from pc in db.Product_Class
                              
                              select pc);

                int[] childs=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    allpcs = allpcs.Where(c => (c.Shop_ID == this.Shop.Shop_ID || childs.Contains(c.Shop_ID)));
                }
                else
                {
                    allpcs = allpcs.Where(c => (c.Shop_ID == this.Shop.Shop_ID || c.Shop_ID==this.Main_Shop.Shop_ID));
                }
                
                if (parentId != null)
                {
                    allpcs = allpcs.Where(a => a.Parent_ID == parentId);
                }
                List<Product_Class> pcs = allpcs.ToList<Product_Class>();

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
                    category.Children = (from cate in db.Product_Class
                                         where cate.Parent_ID == ca.Product_Class_ID
                                         select new BCategory
                                         {
                                             ID = cate.Product_Class_ID,
                                             Mall_ID = cate.Mall_CID,
                                             Mall_PID = cate.Mall_PCID,
                                             Name = cate.Name,
                                             Order = (int)cate.Order,
                                             Enabled = cate.Enabled,
                                             Created = cate.Create_Time,
                                             Created_By = (from u in db.User
                                                          where u.User_ID == cate.Create_User_ID
                                                          select new BUser
                                                          {
                                                              ID = u.User_ID,
                                                              Name = u.Name,
                                                              Mall_ID = u.Mall_ID,
                                                              Mall_Name = u.Mall_Name,
                                                              //EmployeeInfo = (from em in db.Employee where em.User_ID == cate.Create_User_ID select em).FirstOrDefault<Employee>()
                                                          }).FirstOrDefault<BUser>()
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

                    category.Created_By = (from u in db.User
                                          where u.User_ID == ca.Create_User_ID
                                          select new BUser
                                          {
                                              ID = u.User_ID,
                                              Name = u.Name,
                                              Mall_ID = u.Mall_ID,
                                              Mall_Name = u.Mall_Name,
                                              //EmployeeInfo = (from employee in db.Employee
                                              //                where employee.User_ID == u.User_ID
                                              //                select new BEmployee
                                              //                {
                                              //                    ID = employee.Employee_ID,
                                              //                    Name = employee.Name
                                              //                }).FirstOrDefault<BEmployee>(),
                                          }).FirstOrDefault<BUser>();

                    category.Shop = (from sp in db.Shop
                                     where sp.Shop_ID == ca.Shop_ID
                                     select new BShop
                                     {
                                         ID = sp.Shop_ID,
                                         Title = sp.Name,
                                         Description = sp.Description,
                                         Created = (int)sp.Created

                                     }).FirstOrDefault<BShop>();

                   if(category.Shop.ID==this.Main_Shop.Shop_ID)
                   {
                       category.FromMainShop=true;
                   }else if(childs!=null && childs.Length>0 && childs.Contains(category.Shop.ID))
                   {
                       category.FromChildShop=true;
                   }
                    
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
                if (category.Shop == null || category.Shop.ID == 0)
                {
                    pc.Shop_ID = this.Shop.Shop_ID;
                }
                else
                {
                    pc.Shop_ID = category.Shop.ID;
                }
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
                int[] child_shops = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();

                Product_Class oldCategory = (from pc in db.Product_Class where pc.Product_Class_ID == category.ID select pc).ToList<Product_Class>()[0];

                if (oldCategory == null)
                {
                    throw new KMJXCException("您要修改的类目信息不存在");
                }

                if (this.Shop.Shop_ID != this.Main_Shop.Shop_ID)
                {
                    if (oldCategory.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能修改主店铺类目");
                    }

                    if (oldCategory.Shop_ID == this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能其他主店铺类目");
                    }
                }
                else
                {
                    if (oldCategory.Shop_ID != this.Main_Shop.Shop_ID || !child_shops.Contains(oldCategory.Shop_ID))
                    {
                        throw new KMJXCException("您无法修改其他店铺的类目，只能修改主店铺或者子店铺类目");
                    }
                }
                
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
                            cate.Children = new List<BCategory>();
                            foreach (Product_Class childCate in children)
                            {
                                BCategory child = new BCategory();
                                child.Mall_ID = childCate.Mall_CID;
                                child.Mall_PID = childCate.Mall_PCID;
                                child.Name = childCate.Name;
                                cate.Children.Add(child);
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
        public bool UpdatePropValue(int propertyId, string value)
        {
            bool result = false;
            return result;
        }

        public bool AddNewPropValue(int propertyId, List<string> values)
        {
            bool result = false;
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                int[] child_shops = (from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                Product_Spec ps = (from pc in db.Product_Spec where pc.Product_Spec_ID == propertyId select pc).FirstOrDefault<Product_Spec>();
                if (ps == null)
                {
                    throw new KMJXCException("属性丢失,不能添加属性值");
                }

                if (this.Shop.Shop_ID != this.Main_Shop.Shop_ID)
                {
                    if (ps.Shop_ID == this.Main_Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能修改主店铺产品库存属性");
                    }

                    if (ps.Shop_ID == this.Shop.Shop_ID)
                    {
                        throw new KMJXCException("您不能其他主店铺产品库存属性");
                    }
                }
                else
                {
                    if (ps.Shop_ID != this.Main_Shop.Shop_ID && !child_shops.Contains(ps.Shop_ID))
                    {
                        throw new KMJXCException("您不能修改其他店铺的产品库存属性，只能修改主店铺或者子店铺产品库存属性");
                    }
                }

                StringBuilder error = new StringBuilder();
                if (values != null && values.Count > 0)
                {
                    
                    foreach (string value in values)
                    {
                        Product_Spec_Value pv = (from psv in db.Product_Spec_Value where psv.Product_Spec_ID == propertyId && psv.Name == value select psv).FirstOrDefault<Product_Spec_Value>();
                        if (pv != null)
                        {
                            error.Append("属性值:"+value+" 已经存在<br/>");
                            continue;
                        }
                        pv = new Product_Spec_Value();
                        pv.Product_Spec_ID = propertyId;
                        pv.Name = value;
                        pv.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        pv.User_ID = this.CurrentUser.ID;
                        db.Product_Spec_Value.Add(pv);
                    }

                    db.SaveChanges();
                    result = true;
                    if (!string.IsNullOrEmpty(error.ToString()))
                    {
                        result = false;
                        throw new KMJXCException(error.ToString());
                    }
                }
            }
            catch (KMJXCException ex)
            {
                throw ex;
            }
            catch (Exception nex)
            {

            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="propName"></param>
        /// <param name="propValues"></param>
        /// <returns></returns>
        public BProperty CreateProperty(int categoryId,string propName,List<string> propValues,int shop_id=0)
        {
            BProperty bproperty = null;
            if (string.IsNullOrEmpty(propName))
            {
                throw new KMJXCException("属性名称不能为空");
            }
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                var existed = from props in db.Product_Spec where props.Name==propName select props;

                if (categoryId > 0)
                {
                    //existed = existed.Where(a=>a.Product_Class_ID==categoryId);
                }

                if (this.Shop.Parent_Shop_ID > 0)
                {
                    var pexisted = existed.Where(a=>a.Shop_ID==this.Main_Shop.Shop_ID);
                    if (pexisted.FirstOrDefault<Product_Spec>() != null)
                    {
                        throw new KMJXCException("主店铺已经有此属性，不能重复创建，请使用现有主店铺的属性");
                    }
                }

                existed = existed.Where(a => a.Shop_ID == this.Shop.Shop_ID);
                if (existed.FirstOrDefault<Product_Spec>() != null)
                {
                    throw new KMJXCException("此属性已经存在，不能重复创建");
                }

                Product_Spec property = new Product_Spec();
                property.Product_Class_ID = categoryId;
                property.Name = propName;
                property.User_ID = this.CurrentUser.ID;
                property.Shop_ID = this.Shop.Shop_ID;
                if (shop_id > 0)
                {
                    property.Shop_ID = shop_id;
                }
                property.Mall_PID = "";
                property.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                db.Product_Spec.Add(property);
                db.SaveChanges();
                if (property.Product_Spec_ID <= 0)
                {
                    throw new KMJXCException("属性创建失败");
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
                        psv.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        db.Product_Spec_Value.Add(psv);                       
                    }
                    db.SaveChanges();
                    bproperty.Values=(from pv in db.Product_Spec_Value where pv.Product_Spec_ID==property.Product_Spec_ID select pv).ToList<Product_Spec_Value>();
                }
            }
            catch(KMJXCException ex)
            {
                throw ex;
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
        /// <param name="property_id"></param>
        /// <returns></returns>
        public List<Product_Spec_Value> GetPropertyValues(int property_id)
        {
            List<Product_Spec_Value> values;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                values = (from pv in db.Product_Spec_Value where pv.Product_Spec_ID == property_id select pv).ToList<Product_Spec_Value>();
            }

            return values;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public bool DisableCategory(int categoryId)
        {
            bool result = false;
            if (this.CurrentUserPermission.DISABLE_CATEGORY == 0)
            {
                throw new KMJXCException("没有权限禁用类目");
            }
            KuanMaiEntities db = new KuanMaiEntities();
            try
            {
                Product_Class cate=(from pc in db.Product_Class where pc.Product_Class_ID==categoryId select pc).FirstOrDefault<Product_Class>();
                if (cate == null)
                {
                    throw new KMJXCException("要操作的类目不存在");
                }

                cate.Enabled = false;
                db.SaveChanges();
                result = true;
            }
            catch (KMJXCException kex)
            {
                throw kex;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (db != null)
                {
                    db.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<BProperty> GetProperties(int categoryId, int propId=0, string sortBy = null, string dir = null)
        {
            List<BProperty> properties = new List<BProperty>();
            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                int[] childs=(from c in this.DBChildShops select c.Shop_ID).ToArray<int>();
                var props = from prop in db.Product_Spec 
                            //where (prop.Shop_ID == this.Shop.Shop_ID || prop.Shop_ID == this.Main_Shop.Shop_ID) 
                            select prop;

                if (this.Shop.Shop_ID == this.Main_Shop.Shop_ID)
                {
                    props = props.Where(prop => (prop.Shop_ID == this.Shop.Shop_ID || childs.Contains(prop.Shop_ID)));
                }
                else
                {
                    props = props.Where(prop => (prop.Shop_ID == this.Shop.Shop_ID || prop.Shop_ID==this.Main_Shop.Shop_ID));
                }

                if (categoryId > 0)
                {
                    props = props.Where(a => a.Product_Class_ID == categoryId);
                }

                if (propId > 0)
                {
                    props=props.Where(a => a.Product_Spec_ID == propId);
                }

                var efProps = from p in props
                              join shop in db.Shop on p.Shop_ID equals shop.Shop_ID into LShop
                              from l_shop in LShop.DefaultIfEmpty()
                              join user in db.User on p.User_ID equals user.User_ID into LUser
                              from l_user in LUser.DefaultIfEmpty()
                              join category in db.Product_Class on p.Product_Class_ID equals category.Product_Class_ID into LCategory
                              from l_category in LCategory.DefaultIfEmpty()
                              select new BProperty
                              {
                                  ID = p.Product_Spec_ID,
                                  MID = p.Mall_PID,
                                  Name = p.Name,
                                  Shop = l_shop != null ? new BShop
                                  {
                                      ID = l_shop.Shop_ID,
                                      Title = l_shop.Name,
                                      Created = (int)l_shop.Created,
                                      Description = l_shop.Description
                                  } : new BShop
                                  {
                                      ID = 0,
                                      Title = "",
                                      Created = 0,
                                      Description = ""
                                  },
                                  CategoryId = p.Product_Class_ID,
                                  Created = (int)p.Created,
                                  Created_By = l_user != null ? new BUser
                                  {
                                      ID = l_user.User_ID,
                                      Name = l_user.Name,
                                      Mall_Name = l_user.Mall_Name
                                  } :
                                  new BUser
                                  {
                                      ID = 0,
                                      Name = "",
                                      Mall_Name = ""
                                  },

                                  //Values = (from ps in db.Product_Spec_Value where ps.Product_Spec_ID == p.Product_Spec_ID select ps).ToList<Product_Spec_Value>()
                                  Category = l_category
                              };
               
                properties = efProps.ToList<BProperty>();
                if (properties.Count > 0)
                {
                    foreach (BProperty p in properties)
                    {
                        p.Values = (from ps in db.Product_Spec_Value where ps.Product_Spec_ID == p.ID select ps).ToList<Product_Spec_Value>();
                        BShop shop = p.Shop;
                        if (shop != null)
                        {
                            if (shop.ID==this.Main_Shop.Shop_ID)
                            {
                                p.FromMainShop = true;
                                //p.Shop.Parent = (from sp in db.Shop
                                //                 where sp.Shop_ID == shop.Parent_Shop_ID
                                //                 select new BShop
                                //                 {
                                //                     ID = sp.Shop_ID,
                                //                     Title = sp.Name,
                                //                     Created = (int)sp.Created,
                                //                 }).FirstOrDefault<BShop>();
                            }
                            else if(childs!=null && childs.Contains(p.Shop.ID)){
                                p.FromChildShop = true;
                            }
                        }
                    }
                }

            }
            catch(Exception ex)
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
