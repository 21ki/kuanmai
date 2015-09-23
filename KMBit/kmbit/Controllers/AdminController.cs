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
            ResourceManagement resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.Area> provinces = resourceMgt.GetAreas(0);
            List<KMBit.DAL.Sp> sps = resourceMgt.GetSps();
            //List<SelectListItem> selProvinces = (from p in provinces select new SelectListItem { Text=p.Name,Value=p.Id.ToString() }).ToList<SelectListItem>();            
            ViewBag.Provinces = new SelectList(provinces,"Id","Name");
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateResource(CreateResourceViewModel model)
        {
            ResourceManagement resourceMgt = new ResourceManagement(User.Identity.GetUserId<int>());
            List<KMBit.DAL.Area> provinces = null;
            List<KMBit.DAL.Sp> sps = null;
            if (ModelState.IsValid)
            {
                DAL.Resource resource = new DAL.Resource()
                {
                    Address = model.Address,
                    City_Id = model.City,
                    Contact = model.Contact,
                    CreatedBy = User.Identity.GetUserId<int>(),
                    Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                    Description = model.Description,
                    Email = model.Email,
                    Enabled = true,
                    Name = model.Name,
                    Province_Id = model.Province,
                    SP_Id = model.SP,
                    UpdatedBy = User.Identity.GetUserId<int>(),
                    Updated_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now)
                };

                try
                {
                    if (resourceMgt.CreateResource(resource))
                    {
                        return RedirectToAction("Resources");
                    }
                }
                catch (KMBitException ex)
                {
                    ViewBag.ErrMsg = ex.Message;
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
            ViewBag.SPs = new SelectList(sps, "Id", "Name");
            return View(model);
        }

        public ActionResult Resources()
        {
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