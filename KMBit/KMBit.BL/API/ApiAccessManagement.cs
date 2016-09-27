using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;
namespace KMBit.BL.API
{
    public class ApiAccessManagement
    {
        public BUser GetUserByAccesstoken(string token)
        {
            BUser user = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                user = (from u in db.Users where u.AccessToken == token
                        select new BUser
                        {
                            User= u
                        }).FirstOrDefault<BUser>();
                
            }
            return user;
        }

        public bool VerifyApiSignature(string securityToken,string queryStr,string signature)
        {
            bool result = false;
            queryStr += "&key=" + securityToken;
            if(Util.UrlSignUtil.GetMD5(queryStr)==signature)
            {
                result = true;
            }
            return result;
        }
    }
}
