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
using KMBit.BL.Agent;
using System.Collections.Generic;

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
            return View();
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

        public ActionResult ChargeHistories()
        {
            return View();
        }
        public ActionResult PayHistories()
        {
            return View();
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
            return View();
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
    }
}