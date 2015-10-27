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

        public List<BCustomerReChargeHistory> FindCustomerChargeHistoies(int agentId, int customerId, out int total, bool paging = false, int page = 1, int pageSize = 20)
        {
            total = 0;
            List<BCustomerReChargeHistory> histories = new List<BCustomerReChargeHistory>();
            using (chargebitEntities db = new chargebitEntities())
            {
                var query=from rc in db.Customer_Recharge 
                          join c in db.Customer on rc.CustomerId equals c.Id
                          join ag in db.Users on rc.AgentId equals ag.Id into lag
                          from llag in lag.DefaultIfEmpty()
                          select new BCustomerReChargeHistory
                          {
                             Customer=c,
                             History=rc,
                             User=llag
                          };

                if(agentId>0)
                {
                    query = query.Where(o=>o.History.AgentId==agentId);
                }
                if(customerId>0)
                {
                    query = query.Where(o=>o.History.CustomerId==customerId);
                }
                query = query.OrderByDescending(o=>o.History.CreatedTime);
                total = query.Count();
                if(paging)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 20;
                    query = query.Skip((page-1)*pageSize).Take(pageSize);
                }

                histories = query.ToList<BCustomerReChargeHistory>();
            }
            return histories;
        }

        public bool SaveCustomer(BCustomer customer)
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
                Customer dbCus = null;
                if (customer.Id>0)
                {
                    dbCus = (from c in db.Customer where c.Id==customer.Id  select c).FirstOrDefault<Customer>();
                    if(dbCus==null)
                    {
                        throw new KMBitException(string.Format("编号为{0}的客户不存在",customer.Id));
                    }

                    dbCus.Description = customer.Description != null ? dbCus.Description : dbCus.Description;
                    dbCus.ContactEmail = customer.ContactEmail;
                    dbCus.ContactAddress = customer.ContactAddress;
                    dbCus.ContactPeople = customer.ContactPeople;
                    dbCus.ContactPhone = customer.ContactPhone;
                }
                else
                {
                    if(string.IsNullOrEmpty(customer.Name))
                    {
                        throw new KMBitException("客户名称不能为空");
                    }

                    Customer existed = (from c in db.Customer where c.Name==customer.Name select c).FirstOrDefault<Customer>();
                    if(existed!=null)
                    {
                        throw new KMBitException(string.Format("名称为:{0}的客户已经存在",customer.Name));
                    }
                    dbCus = new Customer()
                    {
                        AgentId = customer.AgentId,
                        ContactEmail=customer.ContactEmail,
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
                    db.Customer.Add(dbCus);                   
                }
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public void CustomerRecharge(int customerId,int agentId,float amount)
        {
            if(customerId>0 && agentId>0 && amount>0)
            {
                using (chargebitEntities db = new chargebitEntities())
                {
                    Customer cus = (from c in db.Customer where c.Id==customerId select c).FirstOrDefault<Customer>();
                    if(cus==null)
                    {
                        throw new KMBitException("编号为"+customerId+"不存在");
                    }

                    if(cus.AgentId!=agentId)
                    {
                        throw new KMBitException("编号为" + customerId + "的客户不属于编号为"+agentId+"代理商");
                    }

                    Customer_Recharge charge = new Customer_Recharge() { AgentId = agentId, Amount = amount, CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now), CustomerId = customerId };
                    db.Customer_Recharge.Add(charge);
                    db.SaveChanges();
                    cus.RemainingAmount += amount;
                    db.SaveChanges();
                }
            }else
            {
                throw new KMBitException("输入参数不正确");
            }
        }

        public List<BCustomer> FindCustomers(int agentId,int customerId,out int total,bool paging=false,int page=1,int pageSize=20)
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
                                 ContactEmail=cs.ContactEmail,
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
                if(customerId>0)
                {
                    query = query.Where(c => c.Id == customerId);
                }
                query = query.OrderByDescending(cs => cs.CreatedTime);
                total = query.Count();
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
