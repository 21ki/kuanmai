using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.BL.Admin
{
    public class SiteManagement:BaseManagement
    {
        public SiteManagement(int userId) : base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public SiteManagement(string email) : base(email)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public SiteManagement(BUser user) : base(user)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public Help_Info GetHelpInfo(int id=0,bool isCurrent=true)
        {
            Help_Info info = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from hi in db.Help_Info where hi.IsCurrent==isCurrent select hi;              
                if(id>0)
                {
                    query = query.Where(io=>io.Id==id);
                }
                info = query.OrderByDescending(o=>o.Id).FirstOrDefault<Help_Info>();
            }
            return info;
        }

        public Help_Info CreateHelpInfo(Help_Info newInfo)
        {           
            using (chargebitEntities db = new chargebitEntities())
            {
                if(!string.IsNullOrEmpty(newInfo.About) && !string.IsNullOrEmpty(newInfo.AdminHelp) && !string.IsNullOrEmpty(newInfo.AgentHelp) && !string.IsNullOrEmpty(newInfo.Contact))
                {
                    if(newInfo.UpdateTime<=0)
                    {
                        newInfo.UpdateTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    }
                    if (newInfo.UpdateUser <= 0)
                    {
                        newInfo.UpdateUser = CurrentLoginUser.User.Id;
                    }
                }else
                {
                    throw new KMBitException("所有字段都不能为空");
                }
                newInfo.IsCurrent = true;
                db.Database.ExecuteSqlCommand("Update Help_Info set IsCurrent=0");
                db.Help_Info.Add(newInfo);
                db.SaveChanges();
            }
            return newInfo;
        }
    }
}
