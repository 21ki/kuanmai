using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL;
using KMBit.Util;
namespace KMBit.BL
{
    public class CustomerManagement:BaseManagement
    {
        public CustomerManagement(int userId):base(userId)
        {

        }
        public CustomerManagement(string email) : base(email)
        {

        }
        public CustomerManagement(BUser user) : base(user)
        {

        }

        public List<BCustomer> FindCustomers(int agentId,out int total,bool paging=false,int page=1,int pageSize=20)
        {
            total = 0;
            List<BCustomer> customers = new List<BCustomer>();

            return customers;
        }
    }
}
