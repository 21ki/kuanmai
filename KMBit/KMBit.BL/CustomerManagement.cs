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

        public bool UpdateCustomer(BCustomer customer)
        {
            bool ret = false;
            if (customer == null)
            {
                throw new KMBitException("Customer is NULL");
            }

            if (CurrentLoginUser.IsAdmin)
            {
                //TBD
            }
            else
            {
                if (customer.AgentId > 0 && customer.AgentId != CurrentLoginUser.User.Id)
                {
                    throw new KMBitException("不能为其他代理商创建客户");
                }
            }
            
            using (chargebitEntities db = new chargebitEntities())
            {
                if(customer.Id>0)
                {
                    Customer dbCus = (from c in db.Customer where c.Id==customer.Id  select c).FirstOrDefault<Customer>();
                    if(dbCus==null)
                    {
                        throw new KMBitException(string.Format("编号为{0}的客户不存在",customer.Id));
                    }

                    dbCus.Description = customer.Description != null ? dbCus.Description : dbCus.Description;
                }else
                {
                    if(string.IsNullOrEmpty(customer.Name))
                    {
                        throw new KMBitException("客户名称不能为空");
                    }
                    
                    Customer newCus = new Customer()
                    {
                        AgentId = customer.AgentId,
                        ContactAddress = customer.ContactAddress,
                        ContactPeople = customer.ContactPeople,
                        ContactPhone = customer.ContactPhone,
                        CreatedTime = customer.CreatedTime > 0 ? customer.CreatedTime : DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                        CreditAmount = customer.CreditAmount,
                        Description = customer.Description,
                        Name = customer.Name,
                        OpenId = customer.OpenId,
                        OpenType = customer.OpenType,
                        RemainingAmount = customer.RemainingAmount
                    };
                    db.Customer.Add(newCus);                   
                }
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public List<BCustomer> FindCustomers(int agentId,out int total,bool paging=false,int page=1,int pageSize=20)
        {
            total = 0;
            List<BCustomer> customers = new List<BCustomer>();
            chargebitEntities db = new chargebitEntities();
            try
            {
                var query = from cs in db.Customer
                            join ag in db.Users on cs.AgentId equals ag.Id into lag
                            from llag in lag.DefaultIfEmpty()
                            select new BCustomer
                            {
                                 Agent= llag,
                                 AgentId=cs.AgentId,
                                 ContactAddress=cs.ContactAddress,
                                 ContactPeople=cs.ContactPeople,
                                 ContactPhone=cs.ContactPhone,
                                 CreatedTime=cs.CreatedTime,
                                 CreditAmount=cs.CreditAmount,
                                 Description=cs.Description,
                                 Id=cs.Id,
                                 Name=cs.Name,
                                 OpenId=cs.OpenId,
                                 OpenType=cs.OpenType,
                                 RemainingAmount=cs.RemainingAmount   

                            };

                if(agentId>0)
                {
                    query = query.Where(c=>c.AgentId==agentId);
                }

                query = query.OrderByDescending(cs => cs.CreatedTime);
                if(paging)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 20;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                customers = query.ToList<BCustomer>();
            }
            catch(Exception ex)
            { }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }

            return customers;
        }
    }
}
