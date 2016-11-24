using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Text;

using KMBit.Beans;
using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.BL.Agent;
using KMBit.DAL;
using KMBit.Util;
using KMBit.Models;

namespace KMBit.Controllers.api
{
    [Authorize]
    public class AdminController : BaseApiController
    {

        [AcceptVerbs("POST")]
        public ApiMessage GetAgencyResources()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentAdminMenagement agentMgtMgr = new AgentAdminMenagement(User.Identity.Name);
            int agencyId = 0;
            int.TryParse(request["agencyId"],out agencyId);
            if(agencyId>0)
            {
                List<BResource> rs = agentMgtMgr.FindAgentResources(agencyId);
                message.Status = "OK";
                message.Item = rs;
            }else
            {
                ResourceManagement resourceMgr = new ResourceManagement(agentMgtMgr.CurrentLoginUser);
                int total = 0;
                List<BResource> rs = resourceMgr.FindResources(0, null, 0, out total);
                message.Status = "OK";
                message.Item = rs;
            }
           
            return message;
        }

        [AcceptVerbs("POST")]
        public ApiMessage GetAgencyResourceTaocans()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            AgentAdminMenagement agentMgtMgr = new AgentAdminMenagement(User.Identity.Name);
            int agencyId = 0;
            int resourceId = 0;
            int.TryParse(request["agencyId"], out agencyId);
            int.TryParse(request["resourceId"], out resourceId);
            if(agencyId==0 && resourceId==0)
            {
                message.Status = "ERROR";
                message.Item = null;
                message.Message = "代理商编号和资源编号都不能为空";
                return message;
            }
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            if(resourceId>0 && agencyId>0)
            {
                taocans = agentMgtMgr.FindAgencyResourceTaocans(agencyId, resourceId);
            }else if(resourceId>0 && agencyId<=0)
            {
                ResourceManagement resourceMgr = new ResourceManagement(agentMgtMgr.CurrentLoginUser);
                taocans = resourceMgr.FindResourceTaocans(resourceId, 0, false);
            }
           
            message.Status = "OK";
            message.Item = taocans;
            return message;
        }

        [AcceptVerbs("POST")]
        public ApiMessage ExportOrders()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            int orderId = 0;
            int agencyId = 0;
            int resourceId = 0;
            int resourceTaocanId = 0;
            int status = 0;
            string mobileNumber = null;
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            if (request["OrderId"]!=null)
            {
                int.TryParse(request["OrderId"].ToString(),out orderId);
            }
            if (request["AgencyId"] != null)
            {
                int.TryParse(request["AgencyId"].ToString(), out agencyId);
            }
            if(request["Status"]!=null)
            {
                int.TryParse(request["Status"],out status);
            }
            if (request["ResourceId"] != null)
            {
                int.TryParse(request["ResourceId"].ToString(), out resourceId);
            }
            if (request["ResourceTaocanId"] != null)
            {
                int.TryParse(request["ResourceTaocanId"].ToString(), out resourceTaocanId);
            }
            mobileNumber = request["MobileNumber"];
            if (!string.IsNullOrEmpty(request["StartTime"]))
            {
                DateTime.TryParse(request["StartTime"], out sDate);
            }
            if (!string.IsNullOrEmpty(request["EndTime"]))
            {
                DateTime.TryParse(request["EndTime"], out eDate);
            }
            int total = 0;
            long sintDate = sDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(sDate) : 0;
            long eintDate = eDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(eDate) : 0;
            OrderManagement orderMgr = new OrderManagement(User.Identity.Name);
            List<BOrder> orders = orderMgr.FindOrders(orderId,
                                                      agencyId,
                                                      resourceId,
                                                      resourceTaocanId,
                                                      0,
                                                      null, mobileNumber,
                                                      new int[] { status},
                                                      sintDate,
                                                      eintDate,
                                                      out total,
                                                      0,
                                                      0, false);

            message.Status = "OK";

            string path = request.PhysicalApplicationPath;
            string fileName = orderMgr.CurrentLoginUser.User.Id +"_"+ DateTime.Now.ToString("yyyyMMddHHmm")+".csv";
            string fullPath = System.IO.Path.Combine(path+"\\Temp",fileName);           
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                fs = new FileStream(fullPath, FileMode.Create);
                sw = new StreamWriter(fs,Encoding.UTF8);
                sw.WriteLine("编号,资源,代理商,手机号,省份,城市,平台成本价,平台售价,代理商代理价,代理商售价,时间,状态");
                foreach (BOrder order in orders)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(order.Id.ToString() + ",");
                    builder.Append(order.ReseouceName + ",");
                    builder.Append((order.AgentName != null ? order.AgentName.ToString() : "") + ",");
                    builder.Append(order.MobilePhone + ",");
                    builder.Append((order.MobileProvince != null ? order.MobileProvince : "") + ",");
                    builder.Append((order.MobileCity != null ? order.MobileCity : "") + ",");
                    builder.Append(order.PlatformCostPrice.ToString("0.00") + ",");
                    builder.Append(order.PlatformSalePrice.ToString("0.00") + ",");
                    builder.Append(order.PurchasePrice.ToString("0.00") + ",");
                    builder.Append(order.SalePrice.ToString("0.00") + ",");
                    builder.Append(DateTimeUtil.ConvertToDateTime(order.CreatedTime).ToString("yyyy-MM-dd HH:mm") + ",");
                    builder.Append(order.StatusText != null ? order.StatusText : "");
                    sw.WriteLine(builder.ToString());
                }
            }
            catch(Exception ex)
            {
                logger.Fatal(ex);
            }
            finally
            {                
                if(sw!=null)
                {
                    sw.Close();
                }
                if (fs != null)
                {
                    fs.Close();
                }
            }
            message.Item = "http://"+request.Url.Authority+"/Temp/"+ fileName;
            return message;
        }

        [AcceptVerbs("POST")]
        public ApiMessage GetResourceAndAgentReports()
        {
            this.IniRequest();
            ApiMessage message = new ApiMessage();
            OrderManagement orderMgr = new OrderManagement(User.Identity.Name);
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(request["startTime"]))
            {
                DateTime.TryParse(request["startTime"], out sDate);
            }
            if (!string.IsNullOrEmpty(request["endTime"]))
            {
                DateTime.TryParse(request["endTime"], out eDate);
            }
            long sintDate = sDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(sDate) : 0;
            long eintDate = eDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(eDate) : 0;
            ChartReport report = orderMgr.SearchResourceAndAgentReport(sintDate,eintDate);
            message.Status = "OK";
            message.Item = report;
            return message;
        }
    }
}
