using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

using KMBit.BL;
using KMBit.BL.Admin;
using KMBit.Models;
using KMBit.Util;
using KMBit.Beans;
namespace KMBit.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        int total;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ResourceManagement resourceMgt;
        private AgentAdminMenagement agentAdminMgt;
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
        public AdminController()
        {
            
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        // GET: Admin
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "密码已经修改成功."
                : message == ManageMessageId.SetPasswordSuccess ? " 密码已经被设置成功."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "未知错误，请联系管理员."
                : message == ManageMessageId.AddPhoneSuccess ? "电话号码被成功添加."
                : message == ManageMessageId.RemovePhoneSuccess ? "电话号码被成功删除."
                : "";

            var userId = User.Identity.GetUserId<int>();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId.ToString())
            };
            return View(model);
        }

        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
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

        public ActionResult CreateResource()
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.Area> provinces = resourceMgt.GetAreas(0);
            List<KMBit.DAL.Sp> sps = resourceMgt.GetSps();
            //List<SelectListItem> selProvinces = (from p in provinces select new SelectListItem { Text=p.Name,Value=p.Id.ToString() }).ToList<SelectListItem>();            
            ViewBag.Provinces = new SelectList(provinces,"Id","Name");
            ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View("CreateResource");
        }

        [ValidateInput(false)]
        public ActionResult EditResource(int id)
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (id <= 0)
            {
                ViewBag.Message = "id格式不正确";
                return View("Error");
            }
            List<BResource> resources = resourceMgt.FindResources(id, null, 0,out total);
            if (resources == null)
            {
                ViewBag.Message = "试图编辑的资源不存在";
                return View("Error");
            }

            BResource resouce = resources[0];
            ResourceModel model = new ResourceModel()
            {
                Address = resouce.Resource.Address,
                City = (int)resouce.Resource.City_Id,
                Contact = resouce.Resource.Contact,
                Description = resouce.Resource.Description,
                Email = resouce.Resource.Email,
                Enabled = resouce.Resource.Enabled,
                Id = resouce.Resource.Id,
                Name = resouce.Resource.Name,
                Province = resouce.Resource.Province_Id,
                SP = resouce.Resource.SP_Id
            };
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(resourceMgt.GetAreas(model.Province), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View("CreateResource", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateResource(ResourceModel model)
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            if (ModelState.IsValid)
            {
                DAL.Resource resource = null;
                if (model.Id > 0)
                {
                    List<BResource> resources = resourceMgt.FindResources(model.Id, null, 0,out total);
                    if (resources == null)
                    {
                        ViewBag.Message = "试图编辑的资源不存在";
                        return View("Error");
                    }
                    BResource bresouce = resources[0];
                    resource = bresouce.Resource;
                }
                else
                {                    
                    resource = new DAL.Resource();
                    resource.Name = model.Name;
                    resource.CreatedBy = User.Identity.GetUserId<int>();
                    resource.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                }

                resource.Address = model.Address;
                resource.City_Id = model.City;
                resource.Contact = model.Contact;               
                resource.Description = model.Description;
                resource.Email = model.Email;
                resource.Enabled = model.Enabled;                
                resource.Province_Id = model.Province;
                resource.SP_Id = model.SP;
                resource.UpdatedBy = User.Identity.GetUserId<int>();
                resource.Updated_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);

                try
                {
                    bool result = false;
                    if(resource.Id<=0)
                    {
                        result = resourceMgt.CreateResource(resource);                       
                    }else
                    {
                        result = resourceMgt.UpdateResource(resource);
                    }
                    if (result)
                    {
                        return RedirectToAction("Resources");
                    }
                    
                }
                catch (KMBitException ex)
                {
                    ViewBag.Exception = ex;
                }
                catch (Exception nex)
                {
                    ViewBag.ErrMsg = nex.Message;
                }
                finally
                {
                }
            }

            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(resourceMgt.GetAreas(model.Province), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View(model);
        }

        public ActionResult Resources()
        {
            int pageSize = 20;
            int requestPage = 1;
            int.TryParse(Request["page"], out requestPage);
            requestPage = requestPage == 0 ? 1 : requestPage;
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());            
            var resources = resourceMgt.FindResources(0, null, 0,out total,requestPage,pageSize,true);
            PageItemsResult<BResource> result = new PageItemsResult<BResource>() { CurrentPage = requestPage, Items = resources, PageSize = pageSize, TotalRecords = total };
            KMBit.Grids.KMGrid<BResource> grid = new Grids.KMGrid<BResource>(result);
            return View(grid);
        }

        [HttpGet]
        [ValidateInput(false)]
        public ActionResult CreateResourceTaocan(int? resourceId)
        {
            if(resourceId==null)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            int id = (int)resourceId;            
            ResourceTaocanModel mode = new ResourceTaocanModel() { ResoucedId=id}; 
            if (id <= 0)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());            
            List<BResource> resources = resourceMgt.FindResources(id, null, 0, out total);
            if(resources == null || resources.Count==0)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            BResource resource = resources[0];
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            ViewBag.Resource = resource;
            mode.Enabled = true;           
            return View(mode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateResourceTaocan(ResourceTaocanModel model)
        {
            return UpdateResourceTaocan(model);
        }

        [HttpGet]
        public ActionResult UpdateResourceTaocan(int taocanId)
        {           
            if(taocanId<=0)
            {
                ViewBag.Message = "套餐信息丢失";
                return View("Error");
            }
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BResourceTaocan> resourceTaocans = resourceMgt.FindResourceTaocans(taocanId, 0, 0, out total, 1, 1);
            if(total==0)
            {
                ViewBag.Message = "编号为"+taocanId+"的套餐不存在";
                return View("Error");
            }
            BResourceTaocan bTaocan = resourceTaocans[0];
            ResourceTaocanModel model = new ResourceTaocanModel() { City= bTaocan.Taocan.Area_id, Enabled=bTaocan.Taocan.Enabled, Id=bTaocan.Taocan.Id, Province=0, PurchasePrice=bTaocan.Taocan.Purchase_price, Quantity=bTaocan.Taocan.Quantity,
            ResoucedId=bTaocan.Taocan.Resource_id, SalePrice=bTaocan.Taocan.Sale_price, SP= bTaocan.Taocan.Sp_id};
            if(bTaocan.City!=null && bTaocan.City.Id>0)
            {
                model.Province = bTaocan.City.Upid;
            }           

            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            if (model.City > 0)
            {
                ViewBag.Cities = new SelectList(resourceMgt.GetAreas((int)model.Province), "Id", "Name");
            }
            else
            {
                ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            }
            
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            ViewBag.Resource = bTaocan.Resource;
            return View("CreateResourceTaocan",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateResourceTaocan(ResourceTaocanModel model)
        {
            bool ret = false;
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (ModelState.IsValid)
            {
                KMBit.DAL.Resource_taocan taocan = null;
                if(model.Id>0)
                {
                    int total = 0;
                    List<BResourceTaocan> ts = resourceMgt.FindResourceTaocans(model.Id, 0, 0, out total);
                    if (total == 1)
                    {
                        taocan = ts[0].Taocan;
                    }
                }else
                {
                    taocan = new DAL.Resource_taocan();
                    taocan.CreatedBy = User.Identity.GetUserId<int>();
                    taocan.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    taocan.Quantity = model.Quantity;
                    taocan.Resource_id = model.ResoucedId;
                }
                
                taocan.Area_id = model.City != null ? (int)model.City : 0;                            
                taocan.Enabled = model.Enabled;
                taocan.Purchase_price = model.PurchasePrice;
                taocan.Sale_price = model.SalePrice;
                taocan.Sp_id = model.SP != null ? (int)model.SP : 0;               
                taocan.UpdatedBy = User.Identity.GetUserId<int>();
                taocan.Updated_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                if (model.Id <= 0)
                {
                    ret = resourceMgt.CreateResourceTaocan(taocan);
                }else
                {
                    ret = resourceMgt.UpdateResourceTaocan(taocan);
                }
                
                if (ret)
                {
                    return Redirect("/Admin/ViewResourceTaoCan?resourceId=" + model.ResoucedId);
                }
            }
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(resourceMgt.GetAreas(model.Province != null ? (int)model.Province : 0), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");

            return View("CreateResourceTaocan",model);
        }

        [ValidateInput(false)]
        public ActionResult ViewResourceTaoCan(int? resourceId)
        {
            int id = resourceId??0;            
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BResource> resources = resourceMgt.FindResources(id, null, 0,out total);
            if (resources == null || resources.Count == 0)
            {               
                return View("Error");
            }
            int pageSize = 25;
            int requestPage = 1;
            int.TryParse(Request["page"], out requestPage);
            requestPage = requestPage == 0 ? 1 : requestPage;
            List<BResourceTaocan> resourceTaocans = resourceMgt.FindResourceTaocans(0, id, 0, out total, requestPage, pageSize,true);
            PageItemsResult<BResourceTaocan> result = new PageItemsResult<BResourceTaocan>() { CurrentPage = requestPage, Items = resourceTaocans, PageSize = pageSize, TotalRecords = total };
            KMBit.Grids.KMGrid<BResourceTaocan> grid = new Grids.KMGrid<BResourceTaocan>(result);
            ViewBag.Resource = resources[0];
            return View(grid);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        [HttpGet]
        public ActionResult Agencies()
        {
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            int pageSize = 30;
            int requestPage = 1;
            int.TryParse(Request["page"], out requestPage);
            requestPage = requestPage == 0 ? 1 : requestPage;
            List<BUser> users = agentAdminMgt.FindAgencies(0, null, null, 0, 0, out total,requestPage,pageSize);
            PageItemsResult<BUser> result = new PageItemsResult<BUser>() { CurrentPage = requestPage, Items = users, PageSize = pageSize, TotalRecords = total };
            KMBit.Grids.KMGrid<BUser> grid = new Grids.KMGrid<BUser>(result);
            return View(grid);
        }

        public ActionResult CreateAgency()
        {
            CreateAgencyModel model = new CreateAgencyModel() {Enabled=true };
            agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.User_type> userTypes = agentAdminMgt.GetUserTypes();
            List<KMBit.DAL.Area> provinces = agentAdminMgt.GetAreas(0);
            List<KMBit.DAL.PayType> payTypes = agentAdminMgt.GetPayTypes();

            ViewBag.UserTypes = new SelectList(userTypes, "Id", "Name");
            ViewBag.PayTypes = new SelectList(payTypes, "Id", "Name");
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            return View(model);
        }
        
        public ActionResult ViewAgency(int agencyId)
        {
            return View();
        }

        public ActionResult EditAgency(int agencyId)
        {
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            List<BUser> users = agentAdminMgt.FindAgencies(agencyId, null, null, 0, 0,out total);
            if(users==null || users.Count==0)
            {
                ViewBag.Message =string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }

            BUser agency = users[0];
            CreateAgencyModel model = new CreateAgencyModel() { Enabled = agency.User.Enabled, Address=agency.User.Address, City=agency.User.City_id, Description=agency.User.Description,
                                                                Phone=agency.User.PhoneNumber,Email=agency.User.Email, Id=agencyId, Name=agency.User.Name, PayType=agency.User.Pay_type, Type=agency.User.Type,Province=agency.User.Province_id};           
            List<KMBit.DAL.User_type> userTypes = agentAdminMgt.GetUserTypes();
            List<KMBit.DAL.Area> provinces = agentAdminMgt.GetAreas(0);
            List<KMBit.DAL.PayType> payTypes = agentAdminMgt.GetPayTypes();

            ViewBag.UserTypes = new SelectList(userTypes, "Id", "Name");
            ViewBag.PayTypes = new SelectList(payTypes, "Id", "Name");
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            if(model.Province>0)
            {
                ViewBag.Cities = new SelectList(agentAdminMgt.GetAreas((int)model.Province), "Id", "Name");
            }
            else {
                ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            }
            
            ViewBag.Agency = agency;
            return View("CreateAgency",model);
        }

        public ActionResult AgentRoutes(int? agencyId)
        {
            if(agencyId==null)
            {
                ViewBag.Message = "参数非法";
                return View("Error");
            }

            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            List<BUser> users = agentAdminMgt.FindAgencies((int)agencyId, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }

            BUser agency = users[0];

            List<BAgentRoute> routes = agentAdminMgt.FindRoutes((int)agencyId, 0, 0, out total, 1, 30, true);
            ViewBag.Agency = agency;
            return View();
        }

        [HttpGet]
        public ActionResult CreateAgentRoute()
        {
            int agencyId=0;
            if (Request["agencyId"] == null)
            {
                ViewBag.Message = "参数非法";
                return View("Error");
            }
            int.TryParse(Request["agencyId"], out agencyId);
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());

            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BUser> users = agentAdminMgt.FindAgencies((int)agencyId, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }

            BUser agency = users[0];
            ViewBag.Agency = agency;
            CreateAgentRouteModel mode = new CreateAgentRouteModel() { Enabled = true, AgencyId = agency.User.Id };
            List<BResource> resources = resourceMgt.FindResources(0, null, 0, out total);
            ViewBag.Resources = new SelectList((from r in resources select r.Resource).ToList<KMBit.DAL.Resource>(), "Id", "Name");
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            //taocans = resourceMgt.FindResourceTaocans(0, 1, 0, out total);
            ViewBag.ResourceTaocans = new SelectList((from t in taocans select new { Id = t.Taocan.Id, Name = t.City.Name + " " + t.SP.Name + " " + t.Taocan.Quantity }).ToList(), "Id", "Name");
            return View(mode);
        }

        [HttpPost]
        public ActionResult CreateAgentRoute(CreateAgentRouteModel model)
        {
            if (model == null)
            {
                ViewBag.Message = "参数非法";
                return View("Error");
            }
            int agencyId = model.AgencyId;
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());

            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BUser> users = agentAdminMgt.FindAgencies((int)agencyId, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }

            BUser agency = users[0];
            ViewBag.Agency = agency;
            CreateAgentRouteModel mode = new CreateAgentRouteModel() { Enabled = true, AgencyId = agency.User.Id };
            List<BResource> resources = resourceMgt.FindResources(0, null, 0, out total);
            ViewBag.Resources = new SelectList((from r in resources select r.Resource).ToList<KMBit.DAL.Resource>(), "Id", "Name");
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            taocans = resourceMgt.FindResourceTaocans(0, mode.ResourceId, 0, out total);            
            ViewBag.ResourceTaocans = new SelectList((from t in taocans select new { Id = t.Taocan.Id, Name = t.City.Name + " " + t.SP.Name + " " + t.Taocan.Quantity }).ToList(), "Id", "Name");
            return View(mode);
        }
        public async Task<ActionResult> UpdateAgency(CreateAgencyModel model)
        {
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            bool result = false;
            try
            {
                if (ModelState.IsValid)
                {
                    KMBit.DAL.Users dbUser = new DAL.Users();
                    dbUser.Address = model.Address;
                    dbUser.City_id = model.City != null ? (int)model.City : 0;
                    dbUser.Province_id = model.Province != null ? (int)model.Province : 0;
                    dbUser.Description = model.Description;
                    dbUser.Enabled = model.Enabled;
                    dbUser.Pay_type = model.PayType;
                    dbUser.Type = model.Type;
                    dbUser.PhoneNumber = model.Phone;

                    if (model.Id > 0)
                    {
                        dbUser.Id = model.Id;
                        dbUser.Update_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        result = agentAdminMgt.UpdateAgency(dbUser);
                    }
                    else
                    {
                        dbUser.Email = model.Email;
                        dbUser.PasswordHash = "111111";
                        dbUser.Name = model.Name;
                        dbUser.CreatedBy = User.Identity.GetUserId<int>();
                        dbUser.Regtime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                        result = await agentAdminMgt.CreateAgency(dbUser);
                    }

                    if(result)
                    {
                        return RedirectToAction("Agencies");
                    }else
                    {
                        ViewBag.Exception = new KMBitException("创建失败");
                    }
                }
            }
            catch (KMBitException ex)
            {
                ViewBag.Exception = ex;
            }

            List<KMBit.DAL.User_type> userTypes = agentAdminMgt.GetUserTypes();
            List<KMBit.DAL.Area> provinces = agentAdminMgt.GetAreas(0);
            List<KMBit.DAL.PayType> payTypes = agentAdminMgt.GetPayTypes();

            ViewBag.UserTypes = new SelectList(userTypes, "Id", "Name");
            ViewBag.PayTypes = new SelectList(payTypes, "Id", "Name");
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            if (model.Province != null)
            {
                ViewBag.Cities = new SelectList(agentAdminMgt.GetAreas((int)model.Province), "Id", "Name");
            }
            else
            {
                ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            }

            return View("CreateAgency",model);
        }
    }
}