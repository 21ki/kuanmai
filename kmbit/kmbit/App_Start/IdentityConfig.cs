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
    public class ApplicationUserManager : UserManager<DAL.Users,int>
    {
        public ApplicationUserManager(IUserStore<DAL.Users,int> store)
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

        public override Task<bool> CheckPasswordAsync(DAL.Users user, string password)
        {
            return base.CheckPasswordAsync(user,password);
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<DAL.Users, int>
    {
        public ApplicationSignInManager(UserManager<DAL.Users,int> userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    public class ApplicationUserStore : IUserStore<DAL.Users,int>,
        IUserPasswordStore<DAL.Users,int>, 
        IUserSecurityStampStore<DAL.Users,int>,
        IUserLockoutStore<DAL.Users,int>,
        Microsoft.AspNet.Identity.IUserTwoFactorStore<DAL.Users, int>
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

        public Task CreateAsync(DAL.Users user)
        {
            content.Users.Add(user);
            return content.SaveChangesAsync();
        }

        public Task DeleteAsync(DAL.Users user)
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

        public Task<DAL.Users> FindByIdAsync(int userId)
        {
            return content.Users.FindAsync(userId);
        }

        public Task<DAL.Users> FindByNameAsync(string userName)
        {
            return content.Users.Where(us => us.Email == userName).FirstOrDefaultAsync<KMBit.DAL.Users>();
        }

        public Task<string> GetPasswordHashAsync(DAL.Users user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.GetPasswordHashAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<bool> HasPasswordAsync(DAL.Users user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.HasPasswordAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task SetPasswordHashAsync(DAL.Users user, string passwordHash)
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

        private static void SetApplicationUser(DAL.Users user, IdentityUser identityUser)
        {
            user.PasswordHash = identityUser.PasswordHash;
            user.SecurityStamp = identityUser.SecurityStamp;
            user.Id =int.Parse(identityUser.Id);
            user.UserName = identityUser.UserName;
        }

        private IdentityUser ToIdentityUser(DAL.Users user)
        {
            return new IdentityUser
            {
                Id = user.Id.ToString(),
                PasswordHash = user.PasswordHash,
                SecurityStamp = user.SecurityStamp,
                UserName = user.UserName
            };
        }

        public Task SetSecurityStampAsync(DAL.Users user, string stamp)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.SetSecurityStampAsync(identityUser, stamp);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<string> GetSecurityStampAsync(DAL.Users user)
        {
            var identityUser = ToIdentityUser(user);
            var task = userStore.GetSecurityStampAsync(identityUser);
            SetApplicationUser(user, identityUser);
            return task;
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(Users user)
        {
            return
                Task.FromResult(user.LockoutEndDateUtc.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc))
                    : new DateTimeOffset());
        }

        public Task SetLockoutEndDateAsync(Users user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<int> IncrementAccessFailedCountAsync(Users user)
        {
            user.AccessFailedCount++;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(Users user)
        {
            user.AccessFailedCount = 0;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(Users user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(Users user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(Users user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(Users user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task SetTwoFactorEnabledAsync(Users user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            content.Users.Attach(user);
            content.SaveChanges();
            return Task.FromResult(0);
        }
    }
}
