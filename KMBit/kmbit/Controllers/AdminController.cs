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
using KMBit.BL.Charge;
using KMBit.DAL;
using KMBit.Filters;
namespace KMBit.Controllers
{
    [Authorize]
    [AdminFilter(Message ="非管理员账户请不要试图访问管理员后台界面，多次尝试，系统将自动锁定账户")]    
    public class AdminController : Controller
    {
        int total;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ResourceManagement resourceMgt;
        private AgentAdminMenagement agentAdminMgt;
        private AdministratorManagement adminMgt;
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
        public ActionResult Index()
        {
            UserManagement userMgr = new UserManagement(User.Identity.GetUserId<int>());
            //ViewBag.LoginUser = userMgr.CurrentLoginUser;   
            return View(userMgr.CurrentLoginUser);
        }
               
        public ActionResult ChangePassword()
        {
            return View();
        }        
      
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
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            ResourceModel model = new ResourceModel() { Enabled = true, Id = 0 };
            return View("CreateResource", model);
        }

        [ValidateInput(false)]
        public ActionResult EditResource(int resourceId)
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (resourceId <= 0)
            {
                ViewBag.Message = "资源编号不正确";
                return View("Error");
            }
            List<BResource> resources = resourceMgt.FindResources(resourceId, null, 0,out total);
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

        [HttpGet]
        public ActionResult ConfigureResource(int resourceId)
        {
            if(resourceId<=0)
            {
                ViewBag.Message = "落地资源编号不正确";
                return View("Error");
            }

            ResourceConfigModel model = new ResourceConfigModel() { Id=0,ResoucedId=resourceId, InterfaceAssemblyName= "KMBit.BL.dll" };
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if(!resourceMgt.CurrentLoginUser.Permission.CONFIGURE_RESOURCE)
            {
                ViewBag.Message = "没有权限配置落地资源接口信息";
                return View("Error");
            }

            try
            {
                KMBit.DAL.Resrouce_interface api = resourceMgt.GetResrouceInterface(resourceId);
                if (api != null)
                {
                    model.Id = api.Id;
                    model.InterfaceName = api.Interface_classname;
                    model.Password = KMAes.DecryptStringAES(api.Userpassword);
                    model.UserName = api.Username;
                    model.ResoucedId = api.Resource_id;
                    model.ApiUrl = api.APIURL;
                    model.CallBack = api.CallBackUrl;
                    model.ProductFetchUrl = api.ProductApiUrl;
                }
            }
            catch (KMBitException ex)
            {
                ViewBag.Message = ex.Message;
                return View("Error");
            }
            
            return View(model);
        }


        [HttpPost]
        public ActionResult ConfigureResource(ResourceConfigModel model)
        {
            if(ModelState.IsValid)
            {
                resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
                Resrouce_interface api = new Resrouce_interface()
                { Id=model.Id,Resource_id=model.ResoucedId, APIURL=model.ApiUrl, ProductApiUrl=model.ProductFetchUrl, CallBackUrl=model.CallBack, Interface_classname= model.InterfaceName, Interface_assemblyname=model.InterfaceAssemblyName, Username=model.UserName,Userpassword=model.Password };

                if(resourceMgt.UpdateResrouceInterface(api))
                {
                    return RedirectToAction("Resources");
                }
            }

            return View();
        }

        [HttpGet]
        public ActionResult ImportProduct(int resourceId)
        {
            if (resourceId <= 0)
            {
                ViewBag.Message = "落地资源编号不正确";
                return View("Error");
            }

            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (!resourceMgt.CurrentLoginUser.Permission.CONFIGURE_RESOURCE)
            {
                ViewBag.Message = "没有权限配置落地资源接口信息";
                return View("Error");
            }

            try
            {
                ChargeBridge bridge = new ChargeBridge();
                bridge.ImportResourceProducts(resourceId,User.Identity.GetUserId<int>());
                return Redirect("/Admin/ViewResourceTaoCan?resourceId=" + resourceId);
            }
            catch(KMBitException ex)
            {

                ViewBag.Message = ex.Message;
                return View("Error");
            }
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
                    List<BResource> resources = resourceMgt.FindResources((int)model.Id, null, 0, out total);
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
                    if (resource.Id <= 0)
                    {
                        result = resourceMgt.CreateResource(resource);
                    } else
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
            else
            {
                string validationErrors = string.Join(",",
                    ModelState.Values.Where(E => E.Errors.Count > 0)
                    .SelectMany(E => E.Errors)
                    .Select(E => E.ErrorMessage)
                    .ToArray());

                ViewBag.ErrMsg = validationErrors;
            }

            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(resourceMgt.GetAreas(model.Province), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View("CreateResource",model);
        }

        public ActionResult Resources()
        {
            int pageSize = 20;
            int requestPage = 1;
            int.TryParse(Request["page"], out requestPage);
            requestPage = requestPage == 0 ? 1 : requestPage;
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if(!resourceMgt.CurrentLoginUser.Permission.VIEW_RESOURCE)
            {
                ViewBag.Message = "没有权限查看资源";
                return View("Error");
            }
            ViewBag.LoginUser = resourceMgt.CurrentLoginUser;
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
            ResourceTaocanModel mode = new ResourceTaocanModel() { ResoucedId=id,EnabledDiscount=true}; 
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
            mode.EnabledDiscount = true;          
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
            ResourceTaocanModel model = new ResourceTaocanModel() { Serial=bTaocan.Taocan.Serial, City= bTaocan.Taocan.Area_id, Enabled=bTaocan.Taocan.Enabled, Id=bTaocan.Taocan.Id, Province=0, PurchasePrice=bTaocan.Taocan.Purchase_price, Quantity=bTaocan.Taocan.Quantity,
            ResoucedId=bTaocan.Taocan.Resource_id, SalePrice=bTaocan.Taocan.Sale_price, SP= bTaocan.Taocan.Sp_id,Discount=bTaocan.Taocan.Resource_Discount,EnabledDiscount=bTaocan.Taocan.EnableDiscount};
            if(bTaocan.Province!=null && bTaocan.Province.Id>0)
            {
                model.Province = bTaocan.Province.Upid;
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
            List<BResource> resources = resourceMgt.FindResources(model.ResoucedId, null, 0, out total);
            if (resources == null || resources.Count == 0)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            BResource resource = resources[0];

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
                
                taocan.Area_id = model.Province != null ? (int)model.Province : 0;                            
                taocan.Enabled = model.Enabled;
                taocan.Purchase_price = model.PurchasePrice;
                taocan.EnableDiscount = model.EnabledDiscount;
                taocan.Resource_Discount = model.Discount;
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
            ViewBag.Resource = resource;
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
            int pageSize = 50;
            int requestPage = 1;
            int.TryParse(Request["page"], out requestPage);
            requestPage = requestPage == 0 ? 1 : requestPage;
            List<BResourceTaocan> resourceTaocans = resourceMgt.FindResourceTaocans(0, id, 0, out total, requestPage, pageSize,false);
            PageItemsResult<BResourceTaocan> result = new PageItemsResult<BResourceTaocan>() { CurrentPage = requestPage, Items = resourceTaocans, PageSize = resourceTaocans.Count, TotalRecords = total };
            result.EnablePaging = false;
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
            ViewBag.LoginUser = agentAdminMgt.CurrentLoginUser;
            return View(grid);
        }

        public ActionResult CreateAgency()
        {
            CreateAgencyModel model = new CreateAgencyModel() {Enabled=true,Id=0};
            agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.User_type> userTypes = agentAdminMgt.GetUserTypes();
            List<KMBit.DAL.Area> provinces = agentAdminMgt.GetAreas(0);
            List<KMBit.DAL.PayType> payTypes = agentAdminMgt.GetPayTypes();

            ViewBag.UserTypes = new SelectList(userTypes, "Id", "Name");
            ViewBag.PayTypes = new SelectList(payTypes, "Id", "Name");
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            return View("CreateAgency",model);
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

        [HttpGet]
       
        public ActionResult AgentRoutes(int agencyId)
        {
            if(agencyId<=0)
            {
                ViewBag.Message = "参数非法";
                return View("Error");
            }
            int page = 1;
            int.TryParse(Request["page"], out page);
            page = page > 0 ? page : 1;
            int pageSize = 30;
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            List<BUser> users = agentAdminMgt.FindAgencies((int)agencyId, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }
            BUser agency = users[0];
            List<BAgentRoute> routes = agentAdminMgt.FindRoutes(0,(int)agencyId, 0, 0, out total,null, page, pageSize, true);
            PageItemsResult<BAgentRoute> result = new PageItemsResult<BAgentRoute>() { CurrentPage=page, Items=routes, PageSize=30, TotalRecords=total,EnablePaging=true };
            KMBit.Grids.KMGrid<BAgentRoute> grid = new KMBit.Grids.KMGrid<BAgentRoute>(result);
            ViewBag.Agency = agency;
            return View(grid);
        }

        [HttpGet]
        public ActionResult EditAgentRoute(int routeId)
        {
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());

            List<BAgentRoute> routes = agentAdminMgt.FindRoutes(routeId, 0, 0, 0, out total);
            if(routes==null || routes.Count==0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的路由不存在",routeId);
                return View("Error");
            }
            BAgentRoute route = routes[0];
            List<BUser> users = agentAdminMgt.FindAgencies(route.Route.User_id, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }

            BUser agency = users[0];
            CreateAgentRouteModel model = new CreateAgentRouteModel()
            {
                Id = route.Route.Id,
                AgencyId = route.Route.User_id,
                Discount = route.Route.Discount,
                Enabled = route.Route.Enabled,
                ResouceTaocans = new int[] { route.Route.Resource_taocan_id },
                ResourceId = route.Route.Resource_Id
            };

            ViewBag.Agency = agency;
            ViewBag.Ruote = route;
            return View("EditAgentRoute", model);
        }

        [HttpPost]
        public ActionResult EditAgentRoute(CreateAgentRouteModel model)
        {
            if (agentAdminMgt == null)
                agentAdminMgt = new AgentAdminMenagement(User.Identity.GetUserId<int>());
            if (ModelState.IsValid)
            {
                if (agentAdminMgt.UpdateAgentRuote(model.Id, model.Discount, model.Enabled))
                {
                    return Redirect("/Admin/AgentRoutes?agencyId=" + model.AgencyId);
                }
                
            }else
            {
                string validationErrors = string.Join(",",
                    ModelState.Values.Where(E => E.Errors.Count > 0)
                    .SelectMany(E => E.Errors)
                    .Select(E => E.ErrorMessage)
                    .ToArray());

                ViewBag.ErrMsg = validationErrors;
            }

            List<BAgentRoute> routes = agentAdminMgt.FindRoutes(model.Id, 0, 0, 0, out total);
            if (routes == null || routes.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的路由不存在", model.Id);
                return View("Error");
            }
            BAgentRoute route = routes[0];
            List<BUser> users = agentAdminMgt.FindAgencies(route.Route.User_id, null, null, 0, 0, out total);
            if (users == null || users.Count == 0)
            {
                ViewBag.Message = string.Format("编号为 {0} 的代理商存在");
                return View("Error");
            }
            ViewBag.Agency = users[0];
            ViewBag.Ruote = route;
            return View("EditAgentRoute",model);
        }

        [HttpGet]
        public ActionResult CreateAgentRoute(int agencyId)
        {            
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
            CreateAgentRouteModel model = new CreateAgentRouteModel() { Enabled = true, AgencyId = agency.User.Id,Id=0 };
            List<BResource> resources = resourceMgt.FindResources(0, null, 0, out total);
            ViewBag.Resources = new SelectList((from r in resources select r.Resource).ToList<KMBit.DAL.Resource>(), "Id", "Name");
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            //taocans = resourceMgt.FindResourceTaocans(0, 1, 0, out total);
            //ViewBag.ResourceTaocans = new SelectList((from t in taocans select new { Id = t.Taocan.Id, Name = t.City.Name + " " + t.SP.Name + " " + t.Taocan.Quantity }).ToList(), "Id", "Name");
            ViewBag.ResourceTaocans1 = taocans;
            return View("CreateAgentRoute",model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAgentRoute(CreateAgentRouteModel model)
        {            
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
            if(ModelState.IsValid)
            {
                KMBit.DAL.Agent_route ruote;
                if(model.ResouceTaocans.Length>0)
                {
                    foreach(int tId in model.ResouceTaocans)
                    {
                        ruote = new DAL.Agent_route()
                        {
                            CreatedBy = User.Identity.GetUserId<int>(),
                            Create_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                            Discount = model.Discount,
                            Enabled = model.Enabled,
                            Resource_Id = model.ResourceId,
                            Resource_taocan_id = tId,
                            User_id = model.AgencyId
                        };

                        bool ret = agentAdminMgt.CreateRoute(ruote);
                    }
                }

                return Redirect("/Admin/AgentRoutes?agencyId=" + model.AgencyId);
            }
            List<BResource> resources = resourceMgt.FindResources(0, null, 0, out total);
            ViewBag.Resources = new SelectList((from r in resources select r.Resource).ToList<KMBit.DAL.Resource>(), "Id", "Name");
            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            if(model.ResourceId>0)
            {
                taocans = resourceMgt.FindEnabledResourceTaocansForAgent(model.ResourceId,model.AgencyId);
            }
            
            ViewBag.ResourceTaocans1 = taocans;
            return View(model);
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
                    dbUser.City_id = model.City ;
                    dbUser.Province_id = model.Province ;
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
                        //Task.Run(()=> agentAdminMgt.CreateAgency(dbUser));
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
            if (model.Province >0)
            {
                ViewBag.Cities = new SelectList(agentAdminMgt.GetAreas((int)model.Province), "Id", "Name");
            }
            else
            {
                ViewBag.Cities = new SelectList(new List<KMBit.DAL.Area>(), "Id", "Name");
            }

            return View("CreateAgency",model);
        }

        [HttpGet]
        public ActionResult Administrators()
        {
            if (adminMgt == null)
            {
                adminMgt = new AdministratorManagement(User.Identity.GetUserId<int>());
            }
            
            if (!adminMgt.CurrentLoginUser.IsSuperAdmin && !adminMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长可以查看管理员";
                return View("Error");
            }
            ViewBag.LoginUser = adminMgt.CurrentLoginUser;
            return View(adminMgt.FindAdministrators());
        }

        [HttpGet]
        public ActionResult ChargeOrders(OrderSearchModel searchModel)
        {
            OrderManagement orderMgt = new OrderManagement(User.Identity.GetUserId<int>());
            agentAdminMgt = new AgentAdminMenagement(orderMgt.CurrentLoginUser);
            resourceMgt = new ResourceManagement(orderMgt.CurrentLoginUser);
            if (!orderMgt.CurrentLoginUser.Permission.CHARGE_HISTORY)
            {
                ViewBag.Message = "没有权限查看流量充值记录";
                return View("Error");
            }
            int pageSize = 30;
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            if(!string.IsNullOrEmpty(searchModel.StartTime))
            {
                DateTime.TryParse(searchModel.StartTime, out sDate);
            }
            if (!string.IsNullOrEmpty(searchModel.EndTime))
            {
                DateTime.TryParse(searchModel.EndTime, out eDate);
            }
            long sintDate = sDate!=DateTime.MinValue?DateTimeUtil.ConvertDateTimeToInt(sDate):0;
            long eintDate= eDate != DateTime.MinValue ? DateTimeUtil.ConvertDateTimeToInt(eDate) : 0;
            int page = 1;
            if(Request["page"]!=null)
            {
                int.TryParse(Request["page"],out page);
            }
            searchModel.Page = page;
            List<BOrder> orders = orderMgt.FindOrders(searchModel.OrderId!=null?(int)searchModel.OrderId:0, 
                                                      searchModel.AgencyId!=null?(int)searchModel.AgencyId:0, 
                                                      searchModel.ResourceId!=null?(int)searchModel.ResourceId:0, 
                                                      searchModel.ResourceTaocanId!=null?(int)searchModel.ResourceTaocanId:0, 
                                                      searchModel.RuoteId!=null?(int)searchModel.RuoteId:0, 
                                                      searchModel.SPName, searchModel.MobileNumber,
                                                      searchModel.Status,
                                                      sintDate,
                                                      eintDate,
                                                      out total,
                                                      pageSize,
                                                      searchModel.Page, true);
            PageItemsResult<BOrder> result = new PageItemsResult<BOrder>() { CurrentPage = searchModel.Page, Items = orders, PageSize = pageSize, TotalRecords = total,EnablePaging=true };
            KMBit.Grids.KMGrid<BOrder> grid = new Grids.KMGrid<BOrder>(result);
            BigOrderSearchModel model = new BigOrderSearchModel() { SearchModel = searchModel, OrderGrid = grid };

            List<KMBit.Beans.BUser> agencies = agentAdminMgt.FindAgencies(0, null, null, 0, 0, out total, 0, 0, false,null);
            List<BResource> resources = new List<BResource>();
            if(searchModel.AgencyId!=null)
            {
                resources = agentAdminMgt.FindAgentResources((int)searchModel.AgencyId);
            }else
            {
                resources = resourceMgt.FindResources(0,null,0,out total);
            }
            ViewBag.Agencies = new SelectList((from a in agencies select a.User).ToList<Users>(),"Id","Name");
            ViewBag.Resources = new SelectList((from r in resources select r.Resource).ToList<Resource>(), "Id", "Name");

            List<BResourceTaocan> taocans = new List<BResourceTaocan>();
            if(searchModel.ResourceId!=null)
            {
                if(searchModel.AgencyId==null)
                {
                    taocans = resourceMgt.FindResourceTaocans((int)searchModel.ResourceId, 0, false);
                }
                else
                {
                    taocans = agentAdminMgt.FindAgencyResourceTaocans((int)searchModel.AgencyId, (int)searchModel.ResourceId);
                }                
            }
            ViewBag.Taocans = new SelectList((from t in taocans select new { Id=t.Taocan.Id,Name=t.Taocan2.Name}), "Id", "Name");
            ViewBag.StatusList = new SelectList((from s in StaticDictionary.GetChargeStatusList() select new { Id=s.Id,Name=s.Value}),"Id","Name");
            return View(model);
        }

        [HttpGet]
        public ActionResult ChargeQueue()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CreateAdmin()
        {
            if (adminMgt == null)
            {
                adminMgt = new AdministratorManagement(User.Identity.GetUserId<int>());
            }
            if (!adminMgt.CurrentLoginUser.IsSuperAdmin && !adminMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长才能新建管理员";
                return View("Error");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateAdmin(CreateAdminModel model)
        {
            if(adminMgt==null)
            {
                adminMgt = new AdministratorManagement(User.Identity.GetUserId<int>());
            }
            if (!adminMgt.CurrentLoginUser.IsSuperAdmin && !adminMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长才能新建管理员";
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                KMBit.DAL.Users newUser = new DAL.Users() { Enabled = true, Email = model.Email, Name = model.Name, PasswordHash = "123456789" };
                newUser = await adminMgt.CreateAdministrator(newUser);
                if(newUser.Id>0)
                {
                    return Redirect("/Admin/Administrators");
                }
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult SetAdminStatus(int userId,bool status)
        {
            if (adminMgt == null)
            {
                adminMgt = new AdministratorManagement(User.Identity.GetUserId<int>());
            }
            if (!adminMgt.CurrentLoginUser.IsSuperAdmin && !adminMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长才能禁用管理员";
                return View("Error");
            }
            adminMgt.SetAdminStatus(userId, status);
            return Redirect("/Admin/Administrators");
        }
        

        [HttpGet]
        public ActionResult AdminPermissions(int userId)
        {     
            if(userId==0)
            {
                return View("Error");
            }
            
            PermissionManagement permissionMgt = new PermissionManagement(User.Identity.GetUserId<int>());
            if(!permissionMgt.CurrentLoginUser.IsSuperAdmin && !permissionMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长才能查看或编辑管理员权限";
                return View("Error");
            }
            BUser user = permissionMgt.GetUserInfo(userId);
            Permissions permissions = user.Permission;
            ViewBag.User = user;
            return View(permissions);
        }

        [HttpPost]
        public ActionResult AdminPermissions(Permissions model)
        {
            PermissionManagement permissionMgt = new PermissionManagement(User.Identity.GetUserId<int>());
            if (!permissionMgt.CurrentLoginUser.IsSuperAdmin && !permissionMgt.CurrentLoginUser.IsWebMaster)
            {
                ViewBag.Message = "只有超级管理员和站长才能编辑管理员权限";
                return View("Error");
            }
            if (ModelState.IsValid)
            {
                int id = int.Parse(Request["userId"]);
                permissionMgt.GrantUserPermissions(id, model);
                return Redirect("/Admin/Administrators");
            }           
            
            return View(model);
        }

        [HttpGet]
        public ActionResult Charge()
        {
            BaseManagement baseMgt = new BaseManagement(User.Identity.GetUserId<int>());
            if (!baseMgt.CurrentLoginUser.Permission.CHARGE_BYTE)
            {
                ViewBag.Message = "没有权限充值流量";
                return View("Error");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Charge(ChargeModel model)
        {
            BaseManagement baseMgt = new BaseManagement(User.Identity.GetUserId<int>());
            if (!baseMgt.CurrentLoginUser.Permission.CHARGE_BYTE)
            {
                ViewBag.Message = "没有权限充值流量";
                return View("Error");
            }
           
            if (ModelState.IsValid)
            {
                ChargeBridge cb = new ChargeBridge();
                ChargeOrder order = new ChargeOrder() { ChargeType=2, Payed=true, OperateUserId=User.Identity.GetUserId<int>(), AgencyId = 0, Id = 0, Province=model.Province,City=model.City, MobileSP = model.SPName, MobileNumber = model.Mobile, OutId = "", ResourceId = 0, ResourceTaocanId = model.ResourceTaocanId, RouteId = 0, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now) };
              
                OrderManagement orderMgt = new OrderManagement();
                order = orderMgt.GenerateOrder(order);               
                ChargeResult result = cb.Charge(order);
                ViewBag.Message = result.Message;
            }

            return View();
        }

        [HttpGet]
        public ActionResult OrderDetail(int orderId)
        {
            OrderManagement orderMgt = new OrderManagement(User.Identity.GetUserId<int>());
            if (!orderMgt.CurrentLoginUser.Permission.CHARGE_HISTORY)
            {
                ViewBag.Message = "没有权限查看流量充值记录";
                return View("Error");
            }
            List<BOrder> orders = orderMgt.FindOrders(orderId, 0, 0, 0, 0, null, null, null, 0, 0, out total);
            if(orders==null || orders.Count==0)
            {
                ViewBag.Message = string.Format("编号为:{0}的充值记录不存在",orderId);
                return View("Error");
            }
            return View(orders[0]);
        }

        [HttpGet]
        public ActionResult Reports()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Refound()
        {
            return View();
        }

        [HttpGet]
        public ActionResult SetUserPassword(int userId)
        {
            SetPasswordViewModel model = new SetPasswordViewModel() { Id = userId };
            UserManagement userMgr = new UserManagement(User.Identity.GetUserId<int>());
            BUser user = userMgr.GetUserInfo(userId);
            if(user!=null)
            {
                ViewBag.User = user;
                return View(model);
            }else
            {
                ViewBag.Message = string.Format("编号为{0}的用户不存在",userId);
                return View("Error");
            }           
        }

        [HttpPost]
        public async Task<ActionResult> SetUserPassword(SetPasswordViewModel model)
        {
            UserManagement userMgr = new UserManagement(User.Identity.GetUserId<int>());
            try
            {
                BUser user = userMgr.GetUserInfo(model.Id);
                if (ModelState.IsValid)
                {
                    userMgr.DataProtectionProvider = Startup.DataProtectionProvider;
                    bool ret= await userMgr.SetUserPassword(model.Id, model.NewPassword);                    
                    //var result = await userMgr.AddPasswordAsync(User.Identity.GetUserId<int>(), model.NewPassword);
                    if (!user.IsAdmin)
                    {
                        return RedirectToAction("Agencies");
                    }else
                    {
                        return RedirectToAction("Administrators");
                    }
                    
                }else
                {
                    
                    ViewBag.User = user;
                    return View(model);
                }
                
            }
            catch(KMBitException ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View("Error");
        }

        [HttpGet]
        public ActionResult AdminGuide()
        {
            SiteManagement siteMgr = new SiteManagement(User.Identity.GetUserId<int>());
            Help_Info info = siteMgr.GetHelpInfo();
            return View(info);
        }

        [HttpGet]
        public ActionResult SiteInfo()
        {
            SiteManagement siteMgr = new SiteManagement(User.Identity.GetUserId<int>());
            Help_Info info = siteMgr.GetHelpInfo();
            if(info==null)
            {
                info = new Help_Info() { About = "", AdminHelp = "", AgentHelp = "", Contact = "" };
            }
            return View(info);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SiteInfo(Help_Info model)
        {
            SiteManagement siteMgr = new SiteManagement(User.Identity.GetUserId<int>());            
            try
            {
                siteMgr.CreateHelpInfo(model);
            }
            catch(KMBitException ex)
            {
                ViewBag.Message = ex.Message;
            }

            return View();
        }
    }
}