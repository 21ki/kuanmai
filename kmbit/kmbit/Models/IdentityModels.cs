using System;
using System.Data.Entity;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace KMBit.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class Users : DAL.Users,IUser<int>
    {
        [NotMapped]
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

        public static KMBit.DAL.Users ToDBUser(Users appUser)
        {
            if (appUser == null) {
                return null;
            }
            KMBit.DAL.Users dbUser = new DAL.Users();
            dbUser.Address = appUser.Address;
            dbUser.City_id = appUser.City_id;
            dbUser.Credit_amount = appUser.Credit_amount;
            dbUser.Description = appUser.Description;
            dbUser.Email = appUser.Email;
            dbUser.Id = appUser.Id;
            dbUser.Name = appUser.Name;
            dbUser.PasswordHash = appUser.PasswordHash;
            dbUser.Pay_type = appUser.Pay_type;
            dbUser.Phone = appUser.Phone;
            dbUser.Province_id = appUser.Province_id;
            dbUser.Regtime = appUser.Regtime;
            dbUser.Remaining_amount = appUser.Remaining_amount;
            dbUser.SecurityStamp = appUser.SecurityStamp;
            dbUser.Type = appUser.Type;
            dbUser.Update_time = appUser.Update_time;
            return dbUser;
        }

        public static Users ToAppUser(KMBit.DAL.Users dbUser)
        {
            if (dbUser == null) {
                return null;
            }
            Users appUser = new Users();
            appUser.Address = dbUser.Address;
            appUser.City_id = dbUser.City_id;
            appUser.Credit_amount = dbUser.Credit_amount;
            appUser.Description = dbUser.Description;
            appUser.Email = dbUser.Email;
            appUser.Id = dbUser.Id;
            appUser.Name = dbUser.Name;
            appUser.PasswordHash = dbUser.PasswordHash;
            appUser.Pay_type = dbUser.Pay_type;
            appUser.Phone = dbUser.Phone;
            appUser.Province_id = dbUser.Province_id;
            appUser.Regtime = dbUser.Regtime;
            appUser.Remaining_amount = dbUser.Remaining_amount;
            appUser.SecurityStamp = dbUser.SecurityStamp;
            appUser.Type = dbUser.Type;
            appUser.Update_time = dbUser.Update_time;
            return appUser;
        }

        public static Task<Users> ToAppUserAsync(KMBit.DAL.Users dbUser)
        {
            Task<Users> task = new Task<Users>(delegate () {
                return Users.ToAppUser(dbUser);
            });

            return null;
        }

        public static Task<KMBit.DAL.Users> ToDBUserAsync(Users appUser)
        {
            Task<KMBit.DAL.Users> task = new Task<KMBit.DAL.Users>(delegate () {
                return Users.ToDBUser(appUser);
            });

            return null;
        }
    }
}