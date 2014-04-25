using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Globalization;
using KM.JXC.BL;
using KM.JXC.DBA;
using KM.JXC.Web.Filters;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
namespace KM.JXC.Web.Controllers
{
    public class ImageController : Controller
    {
        //
        // GET: /Image/
        [HttpPost]
        public JsonResult Upload()
        {
            JsonResult res = new JsonResult();
            ApiMessage message = new ApiMessage() { Status = "ok" };
            int len = Request.Files["Filedata"].ContentLength;
            string name = Request.Files["Filedata"].FileName;
            string uid = Request["authid"];
            int user_id = 0;
            int.TryParse(uid,out user_id);

            //if (user_id <= 0) {
            //    message.Status = "failed";
            //    message.Message = "未登录用户不能上传图片";
            //    res.Data = message;
            //    return res;
            //}

            int size = len / (1024);

            if (size > 2 * 1024)
            {
                message.Status = "failed";
                message.Message = "上传的文件大小不能超过3M";
            }
            else
            {
                string user = HttpContext.User.Identity.Name;
                UserManager userMgr = new UserManager(int.Parse(user), null);
                ImageManager imgMgr = new ImageManager(userMgr.CurrentUser,userMgr.Shop,userMgr.CurrentUserPermission);
                string fileName = Path.GetFileName(name);
                string fileExt = Path.GetExtension(name);
                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmss_ffff", DateTimeFormatInfo.InvariantInfo) + fileExt;
                string dir1 = DateTime.Now.ToString("yyyy");
                string dir2 = DateTime.Now.ToString("MM");
                string dir3 = DateTime.Now.ToString("dd");
                string dir4 = DateTime.Now.Hour.ToString();
                string rootPath=Request.PhysicalApplicationPath+@"Content\Uploads\Images";  
                string absPath=@"/Content/Uploads/Images";
                string location = Path.Combine(rootPath, dir1, dir2, dir3, dir4);
                absPath = absPath + "/" + dir1 + "/" + dir2 + "/" + dir3 + "/" + dir4;
                if (!Directory.Exists(location))
                {
                    Directory.CreateDirectory(location);
                }

                KM.JXC.DBA.Image img = new KM.JXC.DBA.Image();
                img.UserID = user_id;
                img.ProductID = 0;
                img.Path = "";
                img.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                img.FileName = name;
                img.UserID = userMgr.CurrentUser.ID;
                imgMgr.CreateImage(img);

                System.Drawing.Image image = System.Drawing.Image.FromStream(Request.Files["Filedata"].InputStream);
                //if(image.Width>600){
                //    ImageUtil.ThumbPic(Request.Files["Filedata"].InputStream, 600, 0, location, fileName, true);
                //}

                ImageUtil.CutForCustom(Request.Files["Filedata"].InputStream, Path.Combine(location, newFileName), 600, 700, 80);

                if (System.IO.File.Exists(Path.Combine(location, newFileName)))
                {
                    img.Path = absPath + "/" + newFileName;
                }
                message.Status = "ok";
                message.Message = "succeed";
                imgMgr.UpdateImage(img);
                message.Item = img;
            }

            res.Data = message;            
            return res;
        }

        [HttpPost]
        public JsonResult Delete()
        {
            JsonResult res = new JsonResult();
            ApiMessage message = new ApiMessage() { Status = "ok" };
            string user = HttpContext.User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user), null);
            ImageManager imgMgr = new ImageManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            res.Data = message;
            int imgId = 0;
            int.TryParse(Request["image_id"], out imgId);

            try
            {
                KM.JXC.DBA.Image image = null;
                if (!imgMgr.DeleteImage(imgId, out image))
                {
                    message.Status = "failed";
                }
                else
                {
                    //Delete image from disk
                    string rootPath = Request.PhysicalApplicationPath;
                    string filePath = rootPath + image.Path;
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                        message.Status = "ok";
                    }
                }
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
            }
            finally
            {
            }

            return res;
        }
    }
}
