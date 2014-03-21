using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
namespace KM.JXC.BL.Open.TaoBao
{
    public class TaoBaoUserManager : BaseManager, IUserManager
    {
        public Mall_Type MallType { get; set; }
        public Access_Token Access_Token { get; set; }
        public Open_Key Open_Key { get; set; }
        public TaoBaoUserManager(Access_Token token, int mall_type_id)
            : base(mall_type_id)
        {
            this.Access_Token = token;
            this.Open_Key = this.GetAppKey();
            this.MallType = this.GetMallType();
        }

        public User GetUser(string user_id, string user_name)
        {
            throw new NotImplementedException();
        }

        public User GetSubUser(string user_id, string user_name)
        {
            throw new NotImplementedException();
        }

        public List<User> GetSubUsers(string user_id)
        {
            throw new NotImplementedException();
        }
    }
}
