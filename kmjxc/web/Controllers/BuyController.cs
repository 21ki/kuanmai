using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using KM.JXC.BL;
using KM.JXC.BL.Models;
using KM.JXC.DBA;
using Newtonsoft.Json.Linq;
using KM.JXC.Web.Models;
using KM.JXC.Common.Util;
using KM.JXC.Common.KMException;
namespace KM.JXC.Web.Controllers
{
    public class BuyController : Controller
    {
        //
        // GET: /Buy/

        public ActionResult Buy()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }

        public ActionResult AddOrder()
        {
            return View();
        }

        public ActionResult Suppliers()
        {
            return View();
        }

        public ActionResult Price()
        {
            return View();
        }

        public ActionResult PriceDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的询价单编号"));
            }

            int oId = 0;
            int.TryParse(id, out oId);
            string uid = HttpContext.User.Identity.Name;
            if (oId <= 0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的询价单编号"));
            }
            BBuyPrice buyPrice = null;
            try
            {
                UserManager userMgr = new UserManager(int.Parse(uid), null);
                BUser user = userMgr.CurrentUser;
                BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
                buyPrice = buyManager.GetBuyPriceFullInfo(oId);
            }
            catch (KMJXCException kex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode(kex.Message));
            }
            catch
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("未知错误"));
            }
            return View(buyPrice);
        }

        public ActionResult BuyOrderDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的验货单编号"));
            }

            int oId = 0;
            int.TryParse(id,out oId);
            string uid = HttpContext.User.Identity.Name;
            BBuyOrder order = null;
            if (oId <= 0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的验货单编号"));
            }
            try
            {
                UserManager userMgr = new UserManager(int.Parse(uid), null);
                BUser user = userMgr.CurrentUser;
                BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
                order = buyManager.GetBuyOrderFullInfo(oId);
            }
            catch (KMJXCException kex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode(kex.Message));
            }
            catch
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("未知错误"));
            }
            return View(order);
        }

        public ActionResult BuyDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的验货单编号"));
            }

            int bId = 0;
            int.TryParse(id, out bId);
            string uid = HttpContext.User.Identity.Name;
            if (bId <=0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("请输入正确的验货单编号"));
            }
            BBuy buy = null;
            try
            {
                UserManager userMgr = new UserManager(int.Parse(uid), null);
                BUser user = userMgr.CurrentUser;
                BuyManager buyManager = new BuyManager(userMgr.CurrentUser, userMgr.Shop, userMgr.CurrentUserPermission);
                buy = buyManager.GetBuyFullInfo(bId);                
            }
            catch (KMJXCException kex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode(kex.Message));
            }
            catch (Exception ex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("未知错误"));
            }

            return View(buy);
        }

        public ActionResult SupplierDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("URL地址里的供应商编号不能为空"));
            }

            int oId = 0;
            int.TryParse(id, out oId);
            string uid = HttpContext.User.Identity.Name;
            if (oId <= 0)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("URL地址里的供应商编号只能为数字"));
            }

            BSupplier supplier = null;
            try
            {
                UserManager userMgr = new UserManager(int.Parse(uid), null);
                BUser user = userMgr.CurrentUser;
                SupplierManager supplierManager = new SupplierManager(user, userMgr.Shop, userMgr.CurrentUserPermission);
                supplier = supplierManager.GetSupplierFullInfo(oId);
                if (supplier == null)
                {
                    return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("没有找到对应的供应商信息"));
                }
            }
            catch (KMJXCException kex)
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode(kex.Message));
            }
            catch
            {
                return Redirect("/Home/Error?message=" + HttpUtility.UrlEncode("未知错误"));
            }
            return View(supplier);
        }
    }
}
