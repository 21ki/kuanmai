using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
namespace KM.JXC.Web.Controllers.api
{
    public class ProductsController : ApiController
    {
        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ApiMessage UpdateProduct()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            ApiMessage message = new ApiMessage();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int product_id = 0;
            int categoryId = 0;
            string description = "";
            string props = "";
            string images = "";
            string title = "";
            int.TryParse(request["cid"], out categoryId);
            int.TryParse(request["product_id"],out product_id);
            description = request["desc"];
            title = request["title"];
            images = request["images"];
            props = request["props"];
            try
            {
                BProduct product = new BProduct();
                product.ID = product_id;
                product.Parent = null;
                product.Category = new BCategory() { ID = categoryId };
                product.Title = title;
                product.Description = description;
                product.Properties = null;
                product.FileRootPath = request.PhysicalApplicationPath;
                if (!string.IsNullOrEmpty(images))
                {
                    product.Images = new List<Image>();
                    string[] ims = images.Split(',');
                    foreach (string img in ims)
                    {
                        int image_id = 0;
                        int.TryParse(img,out image_id);
                        if (image_id > 0)
                        {
                            product.Images.Add(new Image() { ID = image_id });
                        }
                    }
                }
                if (!string.IsNullOrEmpty(props))
                {
                    if (product.Children == null)
                    {
                        product.Children = new List<BProduct>();
                    }
                    string[] groups = props.Split(';');
                    foreach (string group in groups)
                    {
                        string groupp = group.Split('|')[1];
                        int pdtId = int.Parse(group.Split('|')[0]);
                        BProduct child = new BProduct();
                        child.ID = pdtId;
                        child.Title = product.Title;
                        child.Description = product.Description;
                        child.Category = product.Category;
                        List<BProductProperty> properties = new List<BProductProperty>();
                        string[] pops = groupp.Split(',');
                        foreach (string pop in pops)
                        {
                            BProductProperty prop = new BProductProperty();
                            prop.PID = int.Parse(pop.Split(':')[0]);
                            prop.PVID = int.Parse(pop.Split(':')[1]);
                            properties.Add(prop);
                        }
                        child.Properties = properties;
                        product.Children.Add(child);
                    }
                }

                pdtManager.UpdateProduct(ref product);
                message.Status = "ok";
                message.Item = product;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = ex.Message;
            }
            return message;
        }

        [HttpPost]
        [System.Web.Mvc.ValidateInput(false)]
        public ApiMessage Create()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            ApiMessage message = new ApiMessage();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int categoryId = 0;
            string description = "";
            string props = "";
            string images = "";
            string title = "";
            string suppliers = "";
            int.TryParse(request["cid"],out categoryId);
            description = request["desc"];
            title=request["title"];
            images = request["images"];
            props=request["props"];
            suppliers = request["sids"];

            try
            {
                BProduct product = new BProduct();
                product.Parent = null;
                product.Category = new BCategory() { ID =categoryId};
                product.Title = title;
                product.Description = description;
                product.Properties = null;
                if (!string.IsNullOrEmpty(images))
                {
                    product.Images = new List<Image>();
                    string[] ims = images.Split(',');
                    foreach (string img in ims) {
                        product.Images.Add(new Image() { ID=int.Parse(img) });
                    }
                }

                if (!string.IsNullOrEmpty(suppliers))
                {
                    product.Suppliers = new List<Supplier>();
                    string[] sids = suppliers.Split(',');
                    foreach (string sid in sids) 
                    {
                        product.Suppliers.Add(new Supplier() { Supplier_ID=int.Parse(sid), Enabled=true});
                    }
                }

                if (!string.IsNullOrEmpty(props))
                {
                    if (product.Children == null)
                    {
                        product.Children = new List<BProduct>();
                    }
                    string[] groups = props.Split(';');
                    foreach (string group in groups)
                    {
                        BProduct child = new BProduct();
                        child.Title = product.Title;
                        child.Description = product.Description;
                        child.Category = product.Category;                        
                        List<BProductProperty> properties = new List<BProductProperty>();
                        string[] pops = group.Split(',');
                        foreach (string pop in pops)
                        {
                            BProductProperty prop = new BProductProperty();
                            prop.PID = int.Parse(pop.Split(':')[0]);
                            prop.PVID = int.Parse(pop.Split(':')[1]);
                            properties.Add(prop);
                        }
                        child.Properties = properties;
                        product.Children.Add(child);
                    }
                }

                pdtManager.CreateProduct(product);
                message.Status = "ok";
                message.Item = product;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
            }
            catch (Exception ex)
            {
            }

            return message;
        }

        [HttpPost]
        public PQGridData SearchProducts()
        {
            PQGridData data = new PQGridData();
            int page = 0;
            int pageSize = 30;
            int total = 0;
            int? category_id = null;
            string keyword = "";
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"],out pageSize);
            if (request["cid"] != null && request["cid"].ToString() != "" && request["cid"].ToString()!="0")
            {
                int cid = 0;
                int.TryParse(request["cid"],out cid);
                if (cid > 0) {
                    category_id = cid;
                }
            }
            keyword = request["keyword"];
            data.data=pdtManager.SearchProducts(keyword, "", 0, 0, category_id, page, pageSize, out total);
            data.totalRecords = total;
            data.curPage = page;
            return data;
        }

        [HttpPost]
        public ApiMessage GetFullInfo()
        {
            ApiMessage message = new ApiMessage() { Status="ok" };
            BProduct product=null;
            message.Item = product;
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int product_id = 0;
            int.TryParse(request["product_id"],out product_id);
            try
            {
                product = pdtManager.GetProductFullInfo(product_id);
                message.Item = product;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
            }

            return message;
        }
    }
}