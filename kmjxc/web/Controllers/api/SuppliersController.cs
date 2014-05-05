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
using KM.JXC.Common.KMException;
namespace KM.JXC.Web.Controllers.api
{
    public class SuppliersController : ApiController
    {
        [HttpPost]
        public PQGridData GetSuppliers()
        {
            PQGridData data = new PQGridData();
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;           
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            SupplierManager supplierManager = new SupplierManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int page = 1;
            int pageSize = 30;
            int total = 0;

            int.TryParse(request["page"],out page);
            int.TryParse(request["pageSize"], out pageSize);

            data.data = supplierManager.GetSupplies(0, page, pageSize, out total);
            return data;
        }

        [HttpPost]
        public ApiMessage Create()
        {
            ApiMessage message = new ApiMessage() { Status="ok"};
            HttpContextBase context = (HttpContextBase)Request.Properties["MS_HttpContext"];
            HttpRequestBase request = context.Request;
            string user_id = User.Identity.Name;
            UserManager userMgr = new UserManager(int.Parse(user_id), null);
            BUser user = userMgr.CurrentUser;
            SupplierManager supplierManager = new SupplierManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
            int province_id = 0;
            int city_id = 0;
            int district_id = 0;
            string address = request["address"];
            string name = request["name"];
            string fax = request["fax"];
            string phone = request["phone"];
            string postcode = request["postcode"];
            string contact = request["contact"];
            int.TryParse(request["province_id"],out province_id);
            int.TryParse(request["city_id"],out city_id);
            int.TryParse(request["district_id"],out district_id);
            try
            {
                Supplier supplier = new Supplier();
                supplier.Name = name;
                supplier.Phone = phone;
                supplier.PostalCode = postcode;
                supplier.Province_ID = province_id;
                supplier.City_ID = city_id;
                supplier.Address = address;
                supplier.Create_Time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                supplier.Enabled = true;
                supplier.Fax = fax;
                supplier.Shop_ID = supplierManager.Shop.Shop_ID;
                supplier.Contact_Person = contact;
                supplier.User_ID = supplierManager.CurrentUser.ID;
                if (supplierManager.CreateSupplier(supplier))
                {
                    message.Status = "ok";
                    message.Message = "供应商创建成功";
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
    }
}