using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using KMBit.Beans;
using KMBit.Models;
using KMBit.DAL;
using KMBit.BL;
using KMBit.BL.Charge;
using KMBit.BL.Agent;
using KMBit.Util;
using System.Collections.Generic;
using KMBit.BL.Admin;
using KMBit.BL.PayAPI.AliPay;
namespace KMBit.Controllers
{
    [Authorize]
    public class AgentController : Controller
    {
        int total = 0;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        AgentManagement agentMgt;
        public AgentController() { }
        public AgentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Agent
        public ActionResult Index()
        {
            AgentManagement agentMgt = new AgentManagement(User.Identity.GetUserId<int>());          
            return View(agentMgt.CurrentLoginUser);
        }

        [HttpGet]
        public ActionResult EditTaocan(int routeId)
        {
            agentMgt = new AgentManagement(User.Identity.GetUserId<int>());
            List<BAgentRoute> routes= agentMgt.FindTaocans(routeId);
            if (routes == null || routes.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的路由不存在", routeId);
                return View("Error");
            }
            BAgentRoute route = routes[0];
            EditAgentRouteModel model = new EditAgentRouteModel()
            {
                Id = route.Route.Id,
                AgencyId = route.Route.User_id,
                Discount = route.Route.Discount,
                Enabled = route.Route.Enabled,
                ResouceTaocans = new int[] { route.Route.Resource_taocan_id },
                ResourceId = route.Route.Resource_Id,
                SalePrice=route.Route.Sale_price
            };
            ViewBag.Ruote = route;           
            return View(model);
        }

        [HttpPost]
        public ActionResult EditTaocan(EditAgentRouteModel model)
        {
            agentMgt = new AgentManagement(User.Identity.GetUserId<int>());
            if(agentMgt.CurrentLoginUser.User.Id!=model.AgencyId)
            {
                ViewBag.Message = "你没有代理此套餐，无法修改套餐信息";
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    if(agentMgt.UpdateTaocanPrice(model.Id, model.SalePrice,model.Enabled))
                    {
                        return RedirectToAction("Taocan");
                    }else
                    {
                        ViewBag.Message = "更新失败";
                        return View(model);
                    }
                    
                }
                catch (KMBitException ex)
                {
                    ViewBag.Message = ex.Message;
                }catch(Exception eex)
                {
                    ViewBag.Message = eex.Message;
                }
                return View("Error");
            }

            return View(model);
        }

        public ActionResult Taocan()
        {
            agentMgt = new AgentManagement(User.Identity.GetUserId<int>());
            List<BAgentRoute> routes = agentMgt.FindTaocans(0);
            total = routes.Count();
            PageItemsResult<BAgentRoute> result = new PageItemsResult<BAgentRoute>() { CurrentPage = 1, Items = routes, PageSize = total, TotalRecords = total };
            KMBit.Grids.KMGrid<BAgentRoute> grid = new KMBit.Grids.KMGrid<BAgentRoute>(result);
            return View(grid);
        }

        public ActionResult OrderDetail(int orderId)
        {
            OrderManagement orderMgt = new OrderManagement(User.Identity.GetUserId<int>());            
            List<BOrder> orders = orderMgt.FindOrders(orderId, 0, 0, 0, 0, null, null, null, 0, 0, out total);
            if (orders == null || orders.Count == 0)
            {
                ViewBag.Message = string.Format("编号为:{0}的充值记录不存在", orderId);
                return View("Error");
            }

            if(orders[0].AgentId!=orderMgt.CurrentLoginUser.User.Id)
            {
                ViewBag.Message = string.Format("编号为:{0}的充值记录为其他代理商的充值信息，不能查看", orderId);
                return View("Error");
            }

            return View(orders[0]);
        }

        public ActionResult ChargeOrders(OrderSearchModel searchModel)
        {
            OrderManagement orderMgt = new OrderManagement(User.Identity.GetUserId<int>());  
            int pageSize = 30;
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(searchModel.StartTime))
            {
                DateTime.TryParse(searchModel.StartTime, out sDate);
            }
            if (!string.IsNullOrEmpty(searchModel.EndTime))
            {
                DateTime.TryParse(searchModel.EndTime, out eDate);
            }
            long sintDate = sDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(sDate) : 0;
            long eintDate = eDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(eDate) : 0;
            int page = 1;
            if (Request["page"] != null)
            {
                int.TryParse(Request["page"], out page);
            }
            searchModel.Page = page;
            searchModel.AgencyId = User.Identity.GetUserId<int>();
            List<BOrder> orders = orderMgt.FindOrders(searchModel.OrderId != null ? (int)searchModel.OrderId : 0,
                                                      searchModel.AgencyId != null ? (int)searchModel.AgencyId : 0,
                                                      searchModel.ResourceId != null ? (int)searchModel.ResourceId : 0,
                                                      searchModel.ResourceTaocanId != null ? (int)searchModel.ResourceTaocanId : 0,
                                                      searchModel.RuoteId != null ? (int)searchModel.RuoteId : 0,
                                                      searchModel.SPName, searchModel.MobileNumber,
                                                      searchModel.Status,
                                                      sintDate,
                                                      eintDate,
                                                      out total,
                                                      pageSize,
                                                      searchModel.Page, true);
            PageItemsResult<BOrder> result = new PageItemsResult<BOrder>() { CurrentPage = searchModel.Page, Items = orders, PageSize = pageSize, TotalRecords = total, EnablePaging = true };
            KMBit.Grids.KMGrid<BOrder> grid = new Grids.KMGrid<BOrder>(result);
            BigOrderSearchModel model = new BigOrderSearchModel() { SearchModel = searchModel, OrderGrid = grid };  
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            agentMgt = new AgentManagement(orderMgt.CurrentLoginUser);
            List<BAgentRoute> routes = agentMgt.FindTaocans(0);
            taocans = (from r in routes select r.Taocan).ToList<BResourceTaocan>();
            ViewBag.Taocans = new SelectList((from t in taocans select new { Id = t.Taocan.Id, Name = t.Taocan2.Name }), "Id", "Name");
            ViewBag.StatusList = new SelectList((from s in StaticDictionary.GetChargeStatusList() select new { Id = s.Id, Name = s.Value }), "Id", "Name");
            return View(model);
        }
        public ActionResult PayHistories()
        {
            PaymentManagement payMgr = new PaymentManagement(User.Identity.GetUserId<int>());
            int page = 1;
            int pageSize = 30;
            int.TryParse(Request["page"],out page);
            page = page > 0 ? page : 1;
            List<BPaymentHistory> payments = payMgr.FindPayments(0, User.Identity.GetUserId<int>(), 0, out total, true, pageSize, page);
            PageItemsResult<BPaymentHistory> result = new PageItemsResult<BPaymentHistory>() { CurrentPage = page, Items = payments, PageSize = pageSize, TotalRecords = total, EnablePaging = true };
            KMBit.Grids.KMGrid<BPaymentHistory> grid = new Grids.KMGrid<BPaymentHistory>(result);
            return View(grid);
        }
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Charge()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Charge(AgentChargeModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ChargeBridge cb = new ChargeBridge();
                    ChargeOrder order = new ChargeOrder() { Payed = false, OperateUserId = 0, AgencyId = User.Identity.GetUserId<int>(), Id = 0, Province = model.Province, City = model.City, MobileSP =model.SPName, MobileNumber = model.Mobile, OutId = "", ResourceId = 0, ResourceTaocanId = model.ResourceTaocanId, RouteId = model.RouteId, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };

                    OrderManagement orderMgt = new OrderManagement();
                    order = orderMgt.GenerateOrder(order);
                    ChargeResult result = cb.Charge(order);
                    ViewBag.Message = result.Message;                    
                }
                catch (KMBitException ex)
                {
                    ViewBag.Message = ex.Message;
                }finally
                {
                    model = new AgentChargeModel();
                }
            }

            return View(model);
        }
        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId<int>(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public ActionResult ChargeReports()
        {
            return View();
        }

        [HttpGet]
        public ActionResult AgentGuide()
        {
            SiteManagement siteMgr = new SiteManagement(User.Identity.GetUserId<int>());
            Help_Info info = siteMgr.GetHelpInfo();
            return View(info);
        }

        [HttpGet]
        public ActionResult ChargeAccount()
        {
            ChargeAccountModel model = new ChargeAccountModel() { TransferType = 1 };
            ViewBag.Message = Request["message"];         
            return View(model);
        }

        [HttpPost]
        public ActionResult ChargeAccount(ChargeAccountModel model)
        {
            PaymentManagement payMgr = new PaymentManagement(User.Identity.GetUserId<int>());
            if(model.TransferType==1)
            {
                AlipayConfig config = new AlipayConfig(System.IO.Path.Combine(Request.PhysicalApplicationPath, "Config\\AliPayConfig.xml"));
                Submit submit = new Submit(config);
                BPaymentHistory payment = null;
                try
                {
                    payment = payMgr.CreateChargeAccountPayment(User.Identity.GetUserId<int>(), model.Amount, model.TransferType);
                    if (payment == null)
                    {
                        ViewBag.Message = "充值失败";
                        return View(model);
                    }

                }catch(Exception ex)
                {
                    ViewBag.Message = "充值失败";
                    return View(model);
                }
                
                SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
                sParaTemp.Add("partner", config.Partner);
                sParaTemp.Add("seller_email", config.Email);
                sParaTemp.Add("_input_charset", config.Input_charset.ToLower());
                sParaTemp.Add("service", "create_direct_pay_by_user");
                sParaTemp.Add("payment_type", "1");
                sParaTemp.Add("notify_url", config.Notify_Url);
                sParaTemp.Add("return_url", config.Return_Url);
                sParaTemp.Add("out_trade_no", payment.Id.ToString());
                sParaTemp.Add("subject", string.Format("宽迈网络账户充值 {0} 元", model.Amount));
                sParaTemp.Add("total_fee", model.Amount.ToString("0.00"));
                sParaTemp.Add("body", string.Format("宽迈网络账户充值 {0} 元", model.Amount));
                sParaTemp.Add("show_url", "");
                sParaTemp.Add("seller_id", config.Partner);
                //sParaTemp.Add("anti_phishing_key", "");
                //sParaTemp.Add("exter_invoke_ip", "");

                //建立请求
                string sHtmlText = submit.BuildRequest(sParaTemp, "get", "确认");
                //Response.Write("ok");
                Response.Clear();
                Response.Charset = "utf-8";
                Response.Write(sHtmlText);
            }
            return View(model);
        }
    }
}