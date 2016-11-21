using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
using KMBit.BL;
using KMBit.DAL;
using KMBit.Util;
namespace KMBit.BL
{
    public class PaymentManagement:BaseManagement
    {
        public PaymentManagement(int userId):base(userId)
        {

        }

        public PaymentManagement(string email) : base(email)
        {

        }
        public PaymentManagement(BUser user) : base(user)
        {

        }

        public BPaymentHistory CreateChargeAccountPayment(int userId, float amount, int tranfserType)
        {
            BPaymentHistory payment = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                Payment_history p = new Payment_history()
                {
                    Amount = amount,
                    ChargeOrderId = 0,
                    CreatedTime = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now),
                    Tranfser_Type = tranfserType,
                    User_id = userId,
                    PayType = 1,
                    Status=0,
                    OperUserId=0
                };
                db.Payment_history.Add(p);
                db.SaveChanges();
                payment = new BPaymentHistory()
                {
                    Amount = p.Amount,
                    ChargeOrderId = p.ChargeOrderId,
                    CreatedTime = p.CreatedTime,
                    Id = p.Id,
                    PaymentAccount = p.PaymentAccount,
                    PaymentTradeId = p.PaymentTradeId,
                    PayType = p.PayType,
                    Pay_time = p.Pay_time,
                    Tranfser_Type = p.Tranfser_Type,
                    User_id = p.User_id,
                    OperUserId = p.OperUserId,
                    Status=p.Status,

                };
            }
            return payment;
        }

        public bool UpdateAccountMoneyAfterPayment(BPaymentHistory payment)
        {
            bool result = false;
            using (chargebitEntities db = new chargebitEntities())
            {
                if(string.IsNullOrEmpty(payment.PaymentTradeId))
                {
                    throw new KMBitException("支付记录没有宝行支付系统的交易号");
                }

                if(payment.User_id<=0)
                {
                    throw new KMBitException("支付记录没有包含代理商信息");
                }

                if (payment.Amount <= 0)
                {
                    throw new KMBitException("支付记录没有包含支付金额");
                }

                Users user = (from u in db.Users where u.Id == payment.User_id select u).FirstOrDefault<Users>();
                if(user==null)
                {
                    throw new KMBitException(string.Format("没有找到ID为{0}的用户",payment.User_id));
                }

                Payment_history dbpayment = (from p in db.Payment_history where p.User_id == user.Id && p.Id == payment.Id && p.Status==0 && p.PayType==1 select p).FirstOrDefault<Payment_history>();
                if(dbpayment!=null)
                {
                    //it's ready for chargeprocess to sync this amount to user Remaining_amount
                    dbpayment.Status = 1;
                }
                ///user.Remaining_amount += payment.Amount;
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        public List<BPaymentHistory> FindAgentPayments(int paymentId, int agentId, int orderId,int? payType, int tranfserType,int oprUserId,int? status, out int total, bool paging = false, int pageSize = 30, int page = 1)
        {
            total = 0;
            List<BPaymentHistory> payments = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from p in db.Payment_history
                            join u in db.Users on p.User_id equals u.Id into lu
                            from llu in lu.DefaultIfEmpty()
                            join u1 in db.Users on p.OperUserId equals u1.Id into lu1
                            from llu1 in lu1.DefaultIfEmpty()
                            where p.PayType!=0                          
                            select new BPaymentHistory
                            {
                                Amount = p.Amount,
                                ChargeOrderId = p.ChargeOrderId,
                                CreatedTime = p.CreatedTime,
                                Id = p.Id,
                                PaymentAccount = p.PaymentAccount,
                                PaymentTradeId = p.PaymentTradeId,
                                PayType = p.PayType,
                                Pay_time = p.Pay_time,
                                Status=p.Status,
                                Tranfser_Type = p.Tranfser_Type,
                                UserName = llu != null ? llu.Name : "",
                                User_id = p.User_id,
                                OperUserId = p.OperUserId,
                                OprUser = llu1 != null ? llu1.Email : "",
                                StatusText= p.Status==0 ? "未支付":p.Status==1 ? "未处理":p.Status==2 ? "已处理":"",
                                PayTypeText = p.PayType == 0 ? "前台用户支付" : p.PayType == 1 ? "代理商自主充值" : p.PayType == 2 ? "管理员后台充值" : "",
                                TranfserTypeText = p.Tranfser_Type == 0 ? "": p.Tranfser_Type == 1 ? "支付宝" : p.Tranfser_Type == 2 ? "网银" : ""
                            };

                if (paymentId > 0)
                {
                    query = query.Where(p => p.Id == paymentId);
                }
                if (agentId > 0)
                {
                    query = query.Where(p => p.User_id == agentId);
                }
                if (orderId > 0)
                {
                    query = query.Where(p => p.ChargeOrderId == orderId);
                }
                if (oprUserId > 0)
                {
                    query = query.Where(p => p.OperUserId == oprUserId);
                }
                if (payType !=null)
                {
                    query = query.Where(p => p.PayType == (int)payType);
                }
                if (tranfserType > 0)
                {
                    query = query.Where(p => p.Tranfser_Type == tranfserType);
                }
                if (status !=null)
                {
                    query = query.Where(p => p.Status == (int)status);
                }

                query = query.OrderByDescending(p => p.CreatedTime);
                total = query.Count();

                if (paging)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 30;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                payments = query.ToList<BPaymentHistory>();
            }
            return payments;
        }

        public List<BPaymentHistory> FindPayments(int paymentId,int userId, int orderId,out int total,bool paging=false,int pageSize=30,int page=1)
        {
            total = 0;
            List<BPaymentHistory> payments = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from p in db.Payment_history
                            join u in db.Users on p.User_id equals u.Id into lu
                            from llu in lu.DefaultIfEmpty()
                            where !string.IsNullOrEmpty(p.PaymentTradeId)
                            select new BPaymentHistory
                            {
                                Amount = p.Amount,
                                ChargeOrderId = p.ChargeOrderId,
                                CreatedTime = p.CreatedTime,
                                Id = p.Id,
                                PaymentAccount = p.PaymentAccount,
                                PaymentTradeId = p.PaymentTradeId,
                                PayType = p.PayType,
                                Pay_time = p.Pay_time,
                                Tranfser_Type = p.Tranfser_Type,
                                UserName = llu != null ? llu.Name : "",
                                User_id = p.User_id
                            };

                if(paymentId>0)
                {
                    query=query.Where(p=>p.Id==paymentId);
                }
                if(userId>0)
                {
                    query=query.Where(p=>p.User_id==userId);
                }
                if(orderId>0)
                {
                    query=query.Where(p => p.ChargeOrderId == orderId);
                }

                query=query.OrderByDescending(p => p.CreatedTime);
                total = query.Count();
                
                if(paging)
                {
                    page=page>0?page: 1;
                    pageSize = pageSize > 0 ? pageSize : 30;
                    query=query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                payments = query.ToList<BPaymentHistory>();
            }
            return payments;
        }

        public List<BPaymentHistory> FindUnProcessedOnLinePayments(int paymentId, int userId, int orderId, out int total, bool paging = false, int pageSize = 30, int page = 1)
        {
            total = 0;
            List<BPaymentHistory> payments = null;
            using (chargebitEntities db = new chargebitEntities())
            {
                var query = from p in db.Payment_history
                            join u in db.Users on p.User_id equals u.Id into lu
                            from llu in lu.DefaultIfEmpty()
                            where string.IsNullOrEmpty(p.PaymentTradeId)
                            select new BPaymentHistory
                            {
                                Amount = p.Amount,
                                ChargeOrderId = p.ChargeOrderId,
                                CreatedTime = p.CreatedTime,
                                Id = p.Id,
                                PaymentAccount = p.PaymentAccount,
                                PaymentTradeId = p.PaymentTradeId,
                                PayType = p.PayType,
                                Pay_time = p.Pay_time,
                                Tranfser_Type = p.Tranfser_Type,
                                UserName = llu != null ? llu.Name : "",
                                User_id = p.User_id
                            };

                if (paymentId > 0)
                {
                    query = query.Where(p => p.Id == paymentId);
                }
                if (userId > 0)
                {
                    query = query.Where(p => p.User_id == userId);
                }
                if (orderId > 0)
                {
                    query = query.Where(p => p.ChargeOrderId == orderId);
                }

                query = query.OrderByDescending(p => p.CreatedTime);
                total = query.Count();

                if (paging)
                {
                    page = page > 0 ? page : 1;
                    pageSize = pageSize > 0 ? pageSize : 30;
                    query = query.Skip((page - 1) * pageSize).Take(pageSize);
                }

                payments = query.ToList<BPaymentHistory>();
            }
            return payments;
        }
    }
}
