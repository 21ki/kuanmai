﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Security;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using System.Data.Entity.Validation;

using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL
{
    public class UserManagement : BaseManagement
    {
        private ApplicationUserManager userManager;

        public IDataProtectionProvider DataProtectionProvider { get; set; }

        public UserManagement(BUser user):base(user)
        {
            this.logger = log4net.LogManager.GetLogger(this.GetType());
        }
        public UserManagement(string email):base(email)
        {
            this.logger = log4net.LogManager.GetLogger(this.GetType());
        }
        public UserManagement(int userId):base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(UserManagement));
            }

            userManager = new ApplicationUserManager(new ApplicationUserStore(new KMBit.DAL.chargebitEntities()));
        }

        public SystemStatus GetLaji()
        {
            SystemStatus laji;
            using (chargebitEntities db = new chargebitEntities())
            {
                List<LaJi> lajis = (from l in db.LaJi where l.PId == 3 orderby l.UP descending select l).ToList<LaJi>();
                if (lajis.Count > 1)
                {
                    db.Database.SqlQuery<int>("delete * from LaJi");
                    LaJi tmp = new LaJi() { PId = 3, UP = true };
                    db.LaJi.Add(tmp);
                    db.SaveChanges();
                    laji = SystemStatus.RUNNING;                    
                }
                else
                {
                    if(lajis[0].UP)
                    {
                        laji = SystemStatus.RUNNING;
                    }
                    else
                    {
                        laji = SystemStatus.DOWN;
                    }
                }
            }
            return laji;
        }

        public void ShutDownSystem()
        {
            if(!CurrentLoginUser.IsWebMaster)
            {
                throw new KMBitException("你不是系统管理员（站长），没有权限软关闭系统");
            }

            using (chargebitEntities db = new chargebitEntities())
            {
                List<LaJi> lajis = (from l in db.LaJi where l.PId == 3 orderby l.UP descending select l).ToList<LaJi>();
                LaJi lj = lajis[0];
                lj.UP = false;
                db.SaveChanges();
            }
        }

        public void StartSystem()
        {
            if (!CurrentLoginUser.IsWebMaster)
            {
                throw new KMBitException("你不是系统管理员（站长），没有权限软开启系统");
            }

            using (chargebitEntities db = new chargebitEntities())
            {
                List<LaJi> lajis = (from l in db.LaJi where l.PId == 3 orderby l.UP descending select l).ToList<LaJi>();
                LaJi lj = lajis[0];
                lj.UP = true;
                db.SaveChanges();
            }
        }

        public async Task<bool> CreateNewUserAsync(ApplicationUser user)
        {
            if(!CurrentLoginUser.Permission.CREATE_USER)
            {
                throw new KMBitException("没有权限创建代理商");
            }
            bool ret = false;      
            if(string.IsNullOrEmpty(user.Email))
            {
                throw new KMBitException("邮箱不能为空");
            }
            if(string.IsNullOrEmpty(user.PasswordHash))
            {
                throw new KMBitException("密码不能为空");
            }
            if(user.Type<=0)
            {
                user.Type = 2;
            }
                
            if(user.Pay_type==0)
            {
                throw new KMBitException("付费类型不能为空");
            }
            user.CreatedBy = CurrentLoginUser.User.Id;
            user.Regtime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            user.Update_time = user.Regtime;
            var result = await userManager.CreateAsync(user, user.PasswordHash);
            if(result.Succeeded)
            {
                ret = true;
            }
            return ret;
        }

        public bool UpdateUserInfo(ApplicationUser user)
        {            
            if(user.Id<=0 || string.IsNullOrEmpty(user.Email))
            {
                throw new KMBitException("信息不完整，不能更新");
            }
            if (!CurrentLoginUser.Permission.UPDATE_USER&& CurrentLoginUser.User.Id!=user.Id && CurrentLoginUser.User.Email!=user.Email)
            {
                throw new KMBitException("没有权限执行此操作");
            }

            bool ret = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                Users dbuser = ApplicationUser.AppUserToDBUser(user);
                db.Users.Attach(dbuser);
                db.SaveChanges();
                ret = true;
            }
            return ret;
        }

        public void CreateLoginLog(Login_Log log)
        {
            using (chargebitEntities db = new chargebitEntities())
            {
                if(log!=null)
                {
                    if(!string.IsNullOrEmpty(log.LoginIP) && log.UserId>0 && log.LoginTime>0)
                    {
                        db.Login_Log.Add(log);
                        db.SaveChanges();
                    }                    
                }               
            }
        }

        public async Task<bool> SetUserPassword(int userId, string password)
        {
            logger.Info("SetUserPassword");
            if (userId == 0)
            {
                throw new KMBitException("用户编号不能为0");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new KMBitException("密码不能为空");
            }           
            ApplicationUserManager manager = null;
            BUser requestedUser = GetUserInfo(userId);
            if(!requestedUser.IsAdmin)
            {
                if (!CurrentLoginUser.Permission.UPDATE_USER_PASSWORD)
                {
                    throw new KMBitException("没有权限设置用户密码");
                }
            }else if(requestedUser.IsSuperAdmin)
            {
                if (!CurrentLoginUser.IsWebMaster)
                {
                    throw new KMBitException("只有站长可以设置超级管理员密码");
                }
            }else if(requestedUser.IsWebMaster)
            {
                throw new KMBitException("任何人都没有权限修改站长密码");
            }
            bool ret = false;
            try
            {
                var provider = DataProtectionProvider != null ? DataProtectionProvider : new DpapiDataProtectionProvider("Sample");
                logger.Info("provider is created");
                manager = new ApplicationUserManager(new ApplicationUserStore(new chargebitEntities()));
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser, int>(provider.Create("usertoken"));
                logger.Info("DataProtectorTokenProvider is created");
                string code = manager.GeneratePasswordResetToken(userId);
                logger.Info("code:" + code);
                //logger.Info("newpassword:" + password);
                var result = await manager.ResetPasswordAsync(userId, code, password);
                if (result.Succeeded)
                {
                    logger.Info("SetUserPassword succeed");
                    ret = true;
                }
                else
                {
                    logger.Info("SetUserPassword failure");
                    ret = false;
                }
            }
            catch (DbEntityValidationException dbex)
            {
                var errorMessages = dbex.EntityValidationErrors
                     .SelectMany(x => x.ValidationErrors)
                     .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(dbex.Message, " The validation errors are: ", fullErrorMessage);
                logger.Error(exceptionMessage);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
            finally
            {
                if (manager != null)
                {
                    manager.Dispose();
                }
            }
            logger.Info("Leaving SetUserPassword");
            return ret;
        }
    }
}
