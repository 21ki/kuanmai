using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;
namespace KM.JXC.Web.Controllers.api
{
    public class ProductsController : BaseApiController
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
            string suppliers = request["sids"];
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

                if (!string.IsNullOrEmpty(suppliers))
                {
                    product.Suppliers = new List<Supplier>();
                    string[] sids = suppliers.Split(',');
                    foreach (string sid in sids)
                    {
                        product.Suppliers.Add(new Supplier() { Supplier_ID = int.Parse(sid), Enabled = true });
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
        public ApiMessage BatchEditCategory()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            ApiMessage message = new ApiMessage();
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            string products=request["products"];
            if (string.IsNullOrEmpty(products))
            {
                message.Status = "failed";
                message.Message = "没有选择产品，不能批量编辑类目";
                return message;
            }

            int[] product_ids = base.ConvertToIntArrar(products);
            int category = 0;
            int.TryParse(request["category"],out category);
            try
            {
                bool ret = pdtManager.BatchUpdateCategory(category, product_ids);
                if (ret)
                {
                    message.Status = "ok";
                }
            }
            catch (KMJXCException kex)
            {
                message.Status = "failed";
                message.Message = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Message = "未知错误";
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
                        if (group.IndexOf("|") <= 0) {
                            continue;
                        }

                        if (group.Split('|').Length < 2) {
                            continue;
                        }
                        string groupp = group.Split('|')[1];
                        BProduct child = new BProduct();
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

                pdtManager.CreateProduct(product);
                message.Status = "ok";
                message.Item = product;
            }
            catch (KM.JXC.Common.KMException.KMJXCException kex)
            {
                message.Status = "failed";
                message.Item = kex.Message;
            }
            catch (Exception ex)
            {
                message.Status = "failed";
                message.Item = "未知错误，请联系客服";
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

            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }

            if (request["cid"] != null && request["cid"].ToString() != "" && request["cid"].ToString()!="0")
            {
                int cid = 0;
                int.TryParse(request["cid"],out cid);
                if (cid > 0) {
                    category_id = cid;
                }
            }
            keyword = request["keyword"];
            int[] sids = null;
            string suppliers = request["suppliers"];
            if (!string.IsNullOrEmpty(suppliers))
            {
                string[] ss = suppliers.Split(',');
                sids = new int[ss.Length];
                for (int i = 0; i < ss.Length; i++)
                {
                    sids[i] = int.Parse(ss[i]);
                }
            }
            bool includeProps = false;
            if (!string.IsNullOrEmpty(request["include_prop"]) && request["include_prop"] == "1")
            {
                includeProps = true;
            }
            else
            {
                includeProps = false;
            }

            int[] product_ids = null;
            if (!string.IsNullOrEmpty(request["product_ids"]))
            {
                product_ids = base.ConvertToIntArrar(request["product_ids"]);
            }

            bool paging = true;

            if (!string.IsNullOrEmpty(request["paging"]) && request["paging"]=="0")
            {
                paging=false;
            }

            bool includeSupplier = false;
            if (!string.IsNullOrEmpty(request["include_supplier"]) && request["include_supplier"] == "1")
            {
                includeSupplier = true;
            }
            else
            {
                includeSupplier = false;
            }
            data.data = pdtManager.SearchProducts(product_ids, sids, keyword, "", 0, 0, category_id, page, pageSize, out total, includeProps, paging, includeSupplier);
            data.totalRecords = total;
            data.curPage = page;
            data.pageSize = pageSize;
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
            string mall_id = request["mall_id"];
            int.TryParse(request["product_id"],out product_id);
           
            try
            {
                product = pdtManager.GetProductFullInfo(product_id, mall_id);
                if (product != null)
                {
                    message.Item = product;
                }
                else
                {
                    message.Status = "ok";
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

            return message;
        }

        [HttpPost]
        public ApiMessage GetProductProperties()
        {
            ApiMessage message = new ApiMessage() { Status = "ok" };
            List<BProduct> properties = new List<BProduct>();           
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            ProductManager pdtManager = new ProductManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int product_id = 0;           
            int.TryParse(request["product_id"], out product_id);

            try
            {
                properties = pdtManager.GetProductProperties(product_id);
                if (properties != null)
                {
                    message.Item = properties;
                }
                else
                {
                    message.Status = "ok";
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

            return message;
        }

        [HttpPost]
        public PQGridData GetBuyOrders()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int total = 0;
            int page = 1;
            int pageSize = 30;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"],out pageSize);
            int order_id = 0;
            int supplier_id = 0;
            int.TryParse(request["order_id"],out order_id);
            int.TryParse(request["supplier_id"], out supplier_id);
            string keyword = request["keyword"];
            int[] orders = null;
            if (order_id > 0)
            {
                orders = new int[] { order_id};
            }
            int[] suppliers = null;
            if (supplier_id > 0)
            {
                suppliers = new int[] { supplier_id};
            }
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            data.data = buyManager.SearchBuyOrders(orders, null, suppliers, null, keyword, 0, 0, page, pageSize, out total);
            data.totalRecords = total;
            return data;
        }
       
        [HttpPost]
        public PQGridData GetBuys()
        {
            PQGridData data = new PQGridData();            
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int total = 0;
            int page = 1;
            int pageSize = 30;
            int.TryParse(request["page"], out page);
            int.TryParse(request["pageSize"], out pageSize);
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 30;
            }
            data.data = buyManager.SearchBuys(null,null,null,null,null,0,0,page,pageSize,out total);
            data.totalRecords = total;
            return data;
        }

        [HttpPost]
        public List<BBuyDetail> GetBuyDetails()
        {
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int buy_id = 0;
            int.TryParse(request["buy_id"],out buy_id);
            List<BBuyDetail> details = buyManager.GetBuyDetails(buy_id);
            return details;
        }       

        [HttpPost]
        public ApiMessage CreateBuyOrder()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                long writedate=0;
                long issuedate = 0;
                long enddate = 0;
                int supplier_id = 0;
                int order_user = 0;
                string odetails = request["order_products"];
                string desc=request["description"];
                if (!string.IsNullOrEmpty(odetails))
                {
                    odetails = HttpUtility.UrlDecode(odetails);
                }
                if (!string.IsNullOrEmpty(request["writedate"]))
                {
                    writedate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["writedate"]));
                }
                
                if (!string.IsNullOrEmpty(request["issuedate"]))
                {
                    issuedate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["issuedate"]));
                }
                
                if (!string.IsNullOrEmpty(request["enddate"]))
                {
                    enddate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["enddate"]));
                }

                int.TryParse(request["supplier"],out supplier_id);
                int.TryParse(request["order_user"],out order_user);
                BBuyOrder order = new BBuyOrder();
                order.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                order.Created_By = new BUser() { ID = buyManager.CurrentUser.ID };
                order.Description = desc;
                order.EndTime = (Int32)enddate;
                order.InsureTime = (Int32)issuedate;
                order.OrderUser = new BUser() { ID=order_user};
                order.Shop = new BShop() { ID=buyManager.Shop.Shop_ID};
                order.Status = 0;
                order.Supplier = new Supplier() {  Supplier_ID=supplier_id};
                order.WriteTime = (Int32)writedate;
                if (!string.IsNullOrEmpty(odetails))
                {
                    JArray jOrders = JArray.Parse(odetails);
                    order.Details = new List<BBuyOrderDetail>();
                    if (jOrders != null && jOrders.Count > 0)
                    {
                        for (int i = 0; i < jOrders.Count; i++)
                        {
                            JObject jOrder = (JObject)jOrders[i];
                            int parent_product_id = (int)jOrder["product_id"];
                            JArray cjorders = (JArray)jOrder["orders"];
                            if (cjorders != null && cjorders.Count > 0)
                            {
                                for (int j = 0; j < cjorders.Count; j++)
                                {
                                    BBuyOrderDetail oDetail = new BBuyOrderDetail();
                                    oDetail.Parent_Product_ID = parent_product_id;
                                    oDetail.Product = new BProduct() { ID=(int)cjorders[j]["child_id"]};
                                    double price = 0;
                                    int quantity = 0;
                                    double.TryParse(cjorders[j]["price"].ToString(), out price);
                                    int.TryParse(cjorders[j]["quantity"].ToString(), out quantity);
                                    oDetail.Quantity = quantity;
                                    oDetail.Status = 0;
                                    oDetail.Price = price;
                                    if (quantity <= 0 || price<=0)
                                    {
                                        continue;
                                    }
                                    order.Details.Add(oDetail);
                                }
                            }
                        }
                    }
                }

                bool result = buyManager.CreateNewBuyOrder(order);
                if (result)
                {
                    message.Status = "ok";
                }
                else {
                    message.Status = "failed";
                    message.Message = "创建失败";
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
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public ApiMessage CreateBuyPrice()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                string details = request["price_details"];
                string desc = request["desc"];
                string title = request["title"];
                int shopId = 0;
                int.TryParse(request["shop_id"], out shopId);
                if (!string.IsNullOrEmpty(details))
                {
                    details = HttpUtility.UrlDecode(details);
                }

                JArray jDetails = JArray.Parse(details);
                BBuyPrice buyPrice = new BBuyPrice() { Desc = desc, Title = title, Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
                buyPrice.Shop = new BShop() { ID = buyManager.Shop.Shop_ID };
                if (shopId > 0)
                {
                    buyPrice.Shop = new BShop() { ID = shopId };
                }
                buyPrice.Details = new List<BBuyPriceDetail>();
                for (int i = 0; i < jDetails.Count(); i++)
                {
                    JObject jDetail = (JObject)jDetails[i];
                    BBuyPriceDetail bDetail = new BBuyPriceDetail();
                    bDetail.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    bDetail.Price = (double)jDetail["price"];
                    bDetail.Product = new BProduct() { ID = (int)jDetail["sku_id"], ParentID = (int)jDetail["product_id"] };
                    bDetail.Supplier = new BSupplier() { ID = (int)jDetail["supplier_id"] };
                    buyPrice.Details.Add(bDetail);
                }

                bool result = buyManager.CreateBuyPrice(buyPrice);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbex)
            {
                message.Status = "failed";
                message.Message = dbex.Message;
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
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public ApiMessage UpdateBuyOrder()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                int oid = 0;
                long writedate = 0;
                long issuedate = 0;
                long enddate = 0;
                int supplier_id = 0;
                int order_user = 0;
                string odetails = request["order_products"];
                string desc = request["description"];
                int.TryParse(request["oid"],out oid);

                if (!string.IsNullOrEmpty(odetails))
                {
                    odetails = HttpUtility.UrlDecode(odetails);
                }

                if (!string.IsNullOrEmpty(request["writedate"]))
                {
                    writedate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["writedate"]));
                }

                if (!string.IsNullOrEmpty(request["issuedate"]))
                {
                    issuedate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["issuedate"]));
                }

                if (!string.IsNullOrEmpty(request["enddate"]))
                {
                    enddate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["enddate"]));
                }

                int.TryParse(request["supplier"], out supplier_id);
                int.TryParse(request["order_user"], out order_user);
                BBuyOrder order = new BBuyOrder();
                order.ID = oid;
                //order.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                //order.Created_By = new BUser() { ID = buyManager.CurrentUser.ID };
                order.Description = desc;
                order.EndTime = enddate;
                order.InsureTime = issuedate;
                order.OrderUser = new BUser() { ID = order_user };
                order.Shop = new BShop() { ID = buyManager.Shop.Shop_ID };
                order.Status = 0;
                order.Supplier = new Supplier() { Supplier_ID = supplier_id };
                order.WriteTime = writedate;
                if (!string.IsNullOrEmpty(odetails))
                {
                    JArray jOrders = JArray.Parse(odetails);
                    order.Details = new List<BBuyOrderDetail>();
                    if (jOrders != null && jOrders.Count > 0)
                    {
                        for (int i = 0; i < jOrders.Count; i++)
                        {
                            JObject jOrder = (JObject)jOrders[i];
                            int parent_product_id = (int)jOrder["product_id"];
                            JArray cjorders = (JArray)jOrder["orders"];
                            if (cjorders != null && cjorders.Count > 0)
                            {
                                for (int j = 0; j < cjorders.Count; j++)
                                {
                                    BBuyOrderDetail oDetail = new BBuyOrderDetail();
                                    oDetail.Parent_Product_ID = parent_product_id;
                                    double price = 0;
                                    int quantity = 0;
                                    double.TryParse(cjorders["price"].ToString(), out price);
                                    int.TryParse(cjorders["quantity"].ToString(), out quantity);

                                    oDetail.Product = new BProduct() { ID = (int)cjorders[j]["child_id"] };
                                    oDetail.Quantity = quantity;
                                    oDetail.Status = 0;
                                    oDetail.Price = price;
                                    if (quantity <= 0)
                                    {
                                        continue;
                                    }
                                    order.Details.Add(oDetail);
                                }
                            }
                        }
                    }
                }

                bool result = buyManager.UpdateBuyOrder(order);
                if (result)
                {
                    message.Status = "ok";
                }
                else
                {
                    message.Status = "failed";
                    message.Message = "更新失败";
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
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }

        [HttpPost]
        public ApiMessage VerifyOrder()
        {
            ApiMessage message = new ApiMessage();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            try
            {
                int oid = 0;
                long comeDate = 0;
                string odetails = request["order_products"];
                string desc = request["description"];
                int.TryParse(request["oid"], out oid);

                if (!string.IsNullOrEmpty(request["comedate"]))
                {
                    comeDate = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(request["comedate"]));
                }

                BBuy buy = new BBuy();
                buy.ID = 0;
                buy.Order = new BBuyOrder() { ID = oid };
                buy.ComeDate = comeDate;
                buy.Description = desc;
                buy.Created = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                buy.Shop = new BShop() { ID = buyManager.Shop.Shop_ID };
                buy.User = new BUser() { ID = buyManager.CurrentUser.ID };
               
                if (!string.IsNullOrEmpty(odetails))
                {
                    odetails = HttpUtility.UrlDecode(odetails);
                    buy.Details = new List<BBuyDetail>();

                    JArray jsons = JArray.Parse(odetails);
                    if (jsons != null && jsons.Count > 0)
                    {

                        for (int i = 0; i < jsons.Count; i++)
                        {
                            JObject jOrder = (JObject)jsons[i];
                            int parent_product_id = (int)jOrder["product_id"];
                            JArray cjorders = (JArray)jOrder["orders"];

                            if (cjorders != null && cjorders.Count > 0)
                            {
                                for (int j = 0; j < cjorders.Count; j++)
                                {
                                    JObject json = (JObject)cjorders[j];
                                    BBuyDetail oDetail = new BBuyDetail();
                                    oDetail.Buy_Order_ID = oid;
                                    double price = 0;
                                    int quantity = 0;
                                    double.TryParse(json["price"].ToString(), out price);
                                    int.TryParse(json["quantity"].ToString(), out quantity);
                                    oDetail.Price = price;
                                    oDetail.Product = new BProduct() { ID = (int)json["child_id"] };
                                    oDetail.Quantity = quantity;
                                    
                                    if (quantity <= 0)
                                    {
                                        continue;
                                    }
                                    oDetail.Parent_Product_ID = parent_product_id;
                                    buy.Details.Add(oDetail);
                                }
                            }
                        }
                    }
                }

                bool result = buyManager.VerifyBuyOrder(buy);
                
                if (result)
                {
                    message.Status = "ok";
                }
                else
                {
                    message.Status = "failed";
                    message.Message = "验货单创建失败";
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
                message.Message = ex.Message;
            }
            finally
            {

            }

            return message;
        }
    }
}