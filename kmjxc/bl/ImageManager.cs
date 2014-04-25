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
    public class ImageManager : BBaseManager
    { 
        public ImageManager(BUser user, Shop shop, Permission permission)
            : base(user, shop, permission)
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        public void UpdateImage(KM.JXC.DBA.Image image)
        {
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                Image imge = (from img in db.Image where img.ID == image.ID select img).FirstOrDefault<Image>();
                if (imge == null)
                {
                    throw new KMJXCException("图片不存在");
                }
                base.UpdateProperties(imge, image);
                db.SaveChanges();               
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public bool CreateImage(KM.JXC.DBA.Image image)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {               
                db.Image.Add(image);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image_id"></param>
        /// <returns></returns>
        public bool DeleteImage(int image_id,out Image image)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                image=(from img in db.Image where img.ID==image_id select img).FirstOrDefault<Image>();
                if (image == null)
                {
                    throw new KMJXCException("图片不存在");
                }

                db.Image.Remove(image);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image_id"></param>
        /// <returns></returns>
        public bool DeleteProductImage(int product_id)
        {
            bool result = false;
            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                List<Image> images = (from img in db.Image where img.ProductID == product_id select img).ToList<Image>();

                foreach (Image img in images)
                {
                    db.Image.Remove(img);
                }
                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
