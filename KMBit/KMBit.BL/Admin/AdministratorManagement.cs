using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL.Admin
{
    public class AdministratorManagement:BaseManagement
    {
        public AdministratorManagement(int userId):base(userId)
        {

        }
        public AdministratorManagement(BUser user) : base(user)
        {

        }

        public async Task<Users> CreateAdministrator(Users dbUser)
        {
            if(dbUser==null)
            {
                throw new KMBitException("参数不正确");
            }
            if (string.IsNullOrEmpty(dbUser.Email))
            {
                throw new KMBitException("邮箱地址不能为空");
            }
            if (string.IsNullOrEmpty(dbUser.PasswordHash))
            {
                throw new KMBitException("用户密码不能为空");
            }
            if (!CurrentLoginUser.Permission.CREATE_USER)
            {
                throw new KMBitException("没有权限创建用户");
            }

            using (chargebitEntities db = new chargebitEntities())
            {
                Users u = (from usr in db.Users where usr.Email==dbUser.Email select usr).FirstOrDefault<Users>();
                if(u!=null)
                {
                    throw new KMBitException("此邮箱已经注册过，不能重复注册");
                }
                ApplicationUserManager manager = new ApplicationUserManager(new ApplicationUserStore(new chargebitEntities()));
                ApplicationUser appUser = new ApplicationUser();
                appUser.Address = "";
                appUser.AccessFailedCount = 0;
                appUser.City_id = 0;
                appUser.CreatedBy = CurrentLoginUser.User.Id;
                appUser.Credit_amount = 0;
                appUser.Description = "";
                appUser.Email = dbUser.Email;
                appUser.UserName = dbUser.Email;
                appUser.Name = dbUser.Name;
                appUser.PasswordHash = dbUser.PasswordHash;
                appUser.Pay_type = 0;
                appUser.PhoneNumber = dbUser.PhoneNumber;
                appUser.Province_id = 0;
                appUser.Regtime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                appUser.Enabled = dbUser.Enabled;
                appUser.Type = 1;
                appUser.Update_time = appUser.Regtime;
                var result = await manager.CreateAsync(appUser, dbUser.PasswordHash);
                if (result.Succeeded)
                {
                    u = (from usr in db.Users where usr.Email == dbUser.Email select usr).FirstOrDefault<Users>();

                    Admin_Users au = new Admin_Users() { Description = "管理员", IsSuperAdmin = false, IsWebMaster = false, User_Id = u.Id };
                    db.Admin_Users.Add(au);
                    db.SaveChanges();
                }
                return u;
            }
        }

        public void SetAdminStatus(int userId,bool enabled)
        {      
            if(userId==CurrentLoginUser.User.Id)
            {
                throw new KMBitException("不能禁用或启用自己的账户");
            }     
            using (chargebitEntities db= new chargebitEntities())
            {
                Users user = (from u in db.Users where u.Id==userId select u).FirstOrDefault<Users>();
               
                if(user==null)
                { 
                    throw new KMBitException("用户不存在");
                }

                Admin_Users au = (from u in db.Admin_Users where u.User_Id == user.Id select u).FirstOrDefault<Admin_Users>();
                if(au.IsSuperAdmin)
                {
                    if(!CurrentLoginUser.IsWebMaster)
                    {
                        throw new KMBitException("只有网站管理员才能禁用或启用超级管理员账户");
                    }
                }else
                {
                    if (!CurrentLoginUser.IsSuperAdmin)
                    {
                        throw new KMBitException("只有超级管理员才能禁用或启用普通管理员账户");
                    }
                }
                user.Enabled = enabled;
                db.SaveChanges();
            }
        }

        public List<BUser> FindAdministrators()
        {
            List<BUser> users = new List<BUser>();

            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from ua in db.Admin_Users
                            join u in db.Users on ua.User_Id equals u.Id
                            join cu in db.Users on u.CreatedBy equals cu.Id into lcu
                            from lccu in lcu.DefaultIfEmpty()
                            select new BUser
                            {
                                User = u,
                                CreatedBy = lccu
                            };

                users = query.OrderBy(u=>u.User.Regtime).ToList<BUser>();
            }
            users = (from u in users where u.User.Email!=CurrentLoginUser.User.Email select u).ToList<BUser>();
            return users;
        }
    }
}
