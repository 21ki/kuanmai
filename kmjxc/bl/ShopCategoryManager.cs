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
namespace KM.JXC.BL
{
    public class ShopCategoryManager:BBaseManager
    {
        public ShopCategoryManager(BUser user,int shop_id)
            : base(user,shop_id)
        {
            
            
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
        public List<BCategory> GetCategories()
        {
            List<BCategory> categories = new List<BCategory>();          
            
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
               List<Product_Class> allpcs=(from pc in db.Product_Class 
                                        where pc.Shop_ID==this.Main_Shop.Shop_ID 
                                        select pc).ToList<Product_Class>();

               List<Product_Class> pcs = (from p in allpcs where p.Parent_ID == 0 select p).ToList<Product_Class>();

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
               }
            }
            
            return categories;
        }

        /// <summary>
        /// Get all enabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns>Product_Class list</returns>
        public List<BCategory> GetEnabledCategories()
        {
            return (from c in GetCategories() where c.Enabled == true select c).ToList<BCategory>();
        }

        /// <summary>
        /// Get all disabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<BCategory> GetDisabledCategories(int shop_id)
        {
            return (from c in GetCategories() where c.Enabled == false select c).ToList<BCategory>();
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
                db.Product_Class.Add(pc);
                db.SaveChanges();
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
    }
}
