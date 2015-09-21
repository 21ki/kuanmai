using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using KMBit.Util;
using KMBit.DAL;
namespace KMBit
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {

        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) 
        {
            ApplicationUserStore store = new ApplicationUserStore(new KMBit.DAL.chargebitEntities());
            var manager = new ApplicationUserManager(store);
            // Configure validation logic for usernames
            //manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            //{
            //    AllowOnlyAlphanumericUserNames = false,
            //    RequireUniqueEmail = true
            //};

            //// Configure validation logic for passwords
            //manager.PasswordValidator = new PasswordValidator
            //{
            //    RequiredLength = 6,
            //    RequireNonLetterOrDigit = true,
            //    RequireDigit = true,
            //    RequireLowercase = true,
            //    RequireUppercase = true,
            //};

            //// Configure user lockout defaults
            //manager.UserLockoutEnabledByDefault = true;
            //manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            //manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            //// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            //// You can write your own provider and plug it in here.
            //manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is {0}"
            //});
            //manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            //{
            //    Subject = "Security Code",
            //    BodyFormat = "Your security code is {0}"
            //});
            //manager.EmailService = new EmailService();
            //manager.SmsService = new SmsService();
            //var dataProtectionProvider = options.DataProtectionProvider;
            //if (dataProtectionProvider != null)
            //{
            //    manager.UserTokenProvider = 
            //        new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            //}
            return manager;
        }

        public override Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return base.CheckPasswordAsync(user,password);
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(UserManager<ApplicationUser, int> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class ApplicationUserStore : IUserStore<ApplicationUser, int>,
        IUserPasswordStore<ApplicationUser, int>, 
        IUserSecurityStampStore<ApplicationUser, int>,
        IUserLockoutStore<ApplicationUser, int>,
        Microsoft.AspNet.Identity.IUserTwoFactorStore<ApplicationUser, int>
    {
        KMBit.DAL.chargebitEntities content;
        UserStore<IdentityUser> userStore;
        public ApplicationUserStore(KMBit.DAL.chargebitEntities _context)
        {
            this.content = _context;
            if (this.content == null)
            {
                this.content = new chargebitEntities();
            }
            this.userStore = new UserStore<IdentityUser>(this.content);
        }

        public Task CreateAsync(ApplicationUser user)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            content.Users.Add(dbUser);
            return content.SaveChangesAsync();
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (this.content != null)
            {
                this.content.Dispose();
            }
        }

        public Task<ApplicationUser> FindByIdAsync(int userId)
        {
            Users user= content.Users.Find(userId);
            return Task.FromResult(ApplicationUser.DBUserToAppUser(user));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            Users dbUser= content.Users.Where(us => us.Email == userName).FirstOrDefault<KMBit.DAL.Users>();
            return Task.FromResult(ApplicationUser.DBUserToAppUser(dbUser));
        }

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.GetPasswordHashAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.HasPasswordAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.SetPasswordHashAsync(identityUser, passwordHash);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task UpdateAsync(DAL.Users user)
        {
            throw new NotImplementedException();
        }

        private static void SetApplicationUser(ApplicationUser user, IdentityUser identityUser)
        {
            user.PasswordHash = identityUser.PasswordHash;
            user.SecurityStamp = identityUser.SecurityStamp;
            user.Id =int.Parse(identityUser.Id);
            user.UserName = identityUser.UserName;
        }

        private IdentityUser ToIdentityUser(ApplicationUser user)
        {
            return new IdentityUser
            {
                Id = user.Id.ToString(),
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                UserName = user.UserName
            };
        }

        public Task SetSecurityStampAsync(ApplicationUser user, string stamp)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.SetSecurityStampAsync(identityUser, stamp);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<string> GetSecurityStampAsync(ApplicationUser user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.GetSecurityStampAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(ApplicationUser user)
        {
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset lockoutEnd)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            dbUser.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            dbUser.AccessFailedCount++;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            dbUser.AccessFailedCount = 0;
            content.Users.Attach(dbUser);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            dbUser.LockoutEnabled = enabled;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
        {
            Users dbUser = ApplicationUser.AppUserToDBUser(user);
            dbUser.TwoFactorEnabled = enabled;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }
    }

    public class ApplicationUser : KMBit.DAL.Users, IUser<int>
    {
        public string UserName
        {
            get
            {
                return this.Email;
            }

            set
            {
                this.Email = value;
            }
        }

        public static ApplicationUser DBUserToAppUser(Users dbUser)
        {
            if(dbUser==null)
            {
                return null;
            }

            ApplicationUser appUser = new ApplicationUser();
            System.Reflection.PropertyInfo[] properties = appUser.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                System.Reflection.PropertyInfo p = dbUser.GetType().GetProperty(property.Name);
                if(p!=null)
                {
                    property.SetValue(appUser, p.GetValue(dbUser));
                }
            }

            return appUser;
        }

        public static Users AppUserToDBUser(ApplicationUser appUser)
        {
            if(appUser==null)
            {
                return null;
            }

            Users dbUser = new Users();            
            System.Reflection.PropertyInfo[] properties = dbUser.GetType().GetProperties();
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                System.Reflection.PropertyInfo p = appUser.GetType().GetProperty(property.Name);
                if (p != null)
                {
                    property.SetValue(appUser, p.GetValue(dbUser));
                }
            }

            return dbUser;
        }
    }
}
