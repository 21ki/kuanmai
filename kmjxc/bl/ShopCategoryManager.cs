using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
namespace KM.JXC.BL
{
    public class ShopCategoryManager:BaseManager
    {
        public ShopCategoryManager(User user,int shop_id)
            : base(user,shop_id)
        {
            
            
        }

        /// <summary>
        /// Get single category information
        /// </summary>
        /// <param name="category_id"></param>
        /// <returns></returns>
        public Product_Class GetCategory(int category_id)
        {
            Product_Class category = null;

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var opc = from pc in db.Product_Class where pc.Product_Class_ID == category_id select pc;
                if (opc.ToList<Product_Class>().Count == 0)
                {
                    throw new KMJXCException("没有找到类目ID为"+category_id+"的类目");
                }

                if (opc.ToList<Product_Class>().Count >1)
                {
                    throw new KMJXCException("类目ID为" + category_id + "的类目个数不止一个",ExceptionLevel.SYSTEM);
                }

                category = opc.ToList<Product_Class>()[0];
            }

            return category;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        public List<Product_Class> GetCategories(int shop_id)
        {
            List<Product_Class> categories = new List<Product_Class>();
            
            int shop_ID = 0;
            if (shop_id > 0)
            {
                shop_ID = shop_id;
            }
            else
            {
                shop_ID = this.Shop_Id;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                var osp = db.Shop.Where(sp=>sp.Shop_ID==shop_ID);
                if (osp.ToList<Shop>().Count == 0)
                {
                    throw new KMJXCException("店铺信息不存在");
                }

                if (osp.ToList<Shop>().Count > 1)
                {
                    throw new KMJXCException("Shop ID 为"+shop_ID+"的店铺不止一个",ExceptionLevel.SYSTEM);
                }

                Shop shop = osp.ToList<Shop>()[0];

                //in local system all the child shops use the same product categories of main shop
                if (shop.Parent_Shop_ID > 0)
                {
                    shop_ID = (int)shop.Parent_Shop_ID;
                }

                var opc = db.Product_Class.Where(pc => pc.Shop_ID == shop_ID).OrderBy(pc => pc.Product_Class_ID);
                categories = opc.ToList<Product_Class>();
            }
            
            return categories;
        }

        /// <summary>
        /// Get all enabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns>Product_Class list</returns>
        public List<Product_Class> GetEnabledCategories(int shop_id)
        {
            return (from c in GetCategories(shop_id) where c.Enabled==true select c).ToList<Product_Class>();
        }

        /// <summary>
        /// Get all disabled categories
        /// </summary>
        /// <param name="shop_id"></param>
        /// <returns></returns>
        public List<Product_Class> GetDisabledCategories(int shop_id)
        {
            return (from c in GetCategories(shop_id) where c.Enabled == false select c).ToList<Product_Class>();
        }

        /// <summary>
        /// Create new category
        /// </summary>
        /// <param name="category">Product_Class object</param>
        /// <returns></returns>
        public bool CreateCategory(Product_Class category)
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
                db.Product_Class.Add(category);
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
        public bool UpdateCategory(Product_Class category)
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
                Product_Class oldCategory = (from pc in db.Product_Class where pc.Product_Class_ID == category.Product_Class_ID select pc).ToList<Product_Class>()[0];
                this.UpdateProperties(oldCategory, category);
                db.SaveChanges();
                result = true;
            }

            return result;
        }
    }
}
