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
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ResourceManagement resourceMgt;
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

        public ActionResult EditResource(int id)
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (id <= 0)
            {
                ViewBag.Message = "id格式不正确";
                return View("Error");
            }
            List<BResource> resources = resourceMgt.FindResources(id, null, 0);
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
                    List<BResource> resources = resourceMgt.FindResources(model.Id, null, 0);
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
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            var resources = resourceMgt.FindResources(0, null, 0);
            return View(resources);
        }

        [HttpGet]
        [ValidateInput(false)]
        public ActionResult CreateResourceTaocan()
        {
            int id = 0;
            int.TryParse(Request["resourceId"], out id);
            ResourceTaocanModel mode = new ResourceTaocanModel(); 
            if (id <= 0)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BResource> resources = resourceMgt.FindResources(id, null, 0);
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
            mode.ResoucedId = id;
            return View(mode);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateResourceTaocan(ResourceTaocanModel model)
        {
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            if (ModelState.IsValid) {

            }
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            provinces = resourceMgt.GetAreas(0);
            sps = resourceMgt.GetSps();
            ViewBag.Provinces = new SelectList(provinces, "Id", "Name");
            ViewBag.Cities = new SelectList(resourceMgt.GetAreas(model.Province!=null?(int)model.Province:0), "Id", "Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");

            return View(model);
        }

        [ValidateInput(false)]
        public ActionResult ViewResourceTaoCan(int resourceId)
        {
            int id = resourceId;            
            resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<BResource> resources = resourceMgt.FindResources(id, null, 0);
            if (resources == null || resources.Count == 0)
            {
                ViewBag.Message = "资源信息丢失";
                return View("Error");
            }
            BResource resource = resources[0];            
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            ViewBag.Resource = resource;
            return View();
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
    }
}