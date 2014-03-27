using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;
using KM.JXC.Common.KMException;
using Top.Api.Request;
using Top.Api.Response;

namespace KM.JXC.BL.Open.TaoBao
{
    internal class TaoBaoUserManager : OBaseManager, IOUserManager
    {       
        public TaoBaoUserManager(Access_Token token, int mall_type_id)
            : base(mall_type_id,token)
        {
           
        }

        /// <summary>
        /// Get user information from mall
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public BUser GetUser(string user_id, string user_name)
        {
            UserGetRequest req = new UserGetRequest();
            req.Fields = "user_id,uid,nick";
            req.Nick = "";
            UserGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if (response.IsError || response.User==null)
            {
                throw new KMJXCException(response.ErrMsg);
            }
            BUser user = new BUser();
            user.ID = 0;
            user.Mall_ID = response.User.Uid;
            user.Mall_Name = response.User.Nick;
            user.Type = new Mall_Type() {  Mall_Type_ID=this.Mall_Type_ID};
            user.Parent = null;
            user.Password = "";           
            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mall_name"></param>
        /// <returns></returns>
        public BUser GetSubUserFullInfo(string mall_name)
        {
            BUser user = new BUser();
            return user;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="user_name"></param>
        /// <returns></returns>
        public BUser GetSubUser(string user_id, string user_name)
        {
            BUser user=null;
            SubuserFullinfoGetRequest req = new SubuserFullinfoGetRequest();
            req.Fields = "subuser_email,user_email,sub_id,sub_nick,user_nick,user_id,duty_name,duty_id,employee_name,entry_date,office_phone,sex,user_email,employee_id";
            req.SubNick = user_name;
            SubuserFullinfoGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if (response.IsError)
            {
                throw new KMJXCException("在"+this.MallType.Name+"没有找到用户"+user_name,ExceptionLevel.ERROR);
            }

            user = new BUser();
            user.Mall_Name = user_name;
            user.Mall_ID = response.SubFullinfo.SubId.ToString();
            user.Type = this.MallType;
            user.Parent = new BUser();
            user.Parent.Mall_ID = response.SubFullinfo.UserId.ToString();
            user.Parent.Mall_Name = response.SubFullinfo.UserNick;
            user.Parent.Type = this.MallType;
            user.Parent.Parent = null;
            Employee employee = new Employee(); ;
            user.EmployeeInfo = employee;
            employee.Department = response.SubFullinfo.DepartmentName;
            employee.Duty = response.SubFullinfo.DutyName;
            employee.Email = response.SubFullinfo.SubuserEmail;
            employee.Gendar = response.SubFullinfo.Sex.ToString();
            employee.Name = response.SubFullinfo.EmployeeName;
            employee.Phone = response.SubFullinfo.OfficePhone;
            return user;
        }

        /// <summary>
        /// Get sub accounts from mall
        /// </summary>
        /// <param name="mainUser"></param>
        /// <returns></returns>
        public List<BUser> GetSubUsers(BUser mainUser)
        {
            List<BUser> users = new List<BUser>();
            SubusersGetRequest req = new SubusersGetRequest();
            req.UserNick = mainUser.Mall_Name;            
            SubusersGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            if (response.IsError)
            {
                throw new KMJXCException(response.ErrMsg);
            }

            foreach (Top.Api.Domain.SubAccountInfo subaccount in response.Subaccounts)
            {
                BUser u = new BUser();
                u = this.GetSubUser(subaccount.SubId.ToString(), subaccount.SubNick);
                u.Parent = mainUser;
                users.Add(u);
            }
            return users;
        }
    }
}
