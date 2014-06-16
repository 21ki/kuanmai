using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using KM.JXC.DBA;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Models;
using KM.JXC.Common.KMException;
using KM.JXC.Common;
using KM.JXC.Common.Util;

using TB = Top.Api.Domain;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;

namespace KM.JXC.BL.Open.TaoBao
{
    public class TaobaoTradeManager : OBaseManager,IOTradeManager
    {
        private string[] status = new string[] { "WAIT_BUYER_CONFIRM_GOODS", "SELLER_CONSIGNED_PART", "TRADE_CLOSED", "TRADE_FINISHED" };

        public TaobaoTradeManager(Access_Token token, int mall_type_id)
            : base(mall_type_id,token)
        {
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade_id"></param>
        public void SyncSingleTrade(string trade_id)
        {
        }

        /// <summary>
        /// Sync mall trades by trade created datetime.
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="status"></param>
        /// <param name="totalTrades"></param>
        /// <param name="hasNextPage"></param>
        /// <returns></returns>
        public List<BSale> SyncMallTrades(DateTime? sDate, DateTime? eDate, string status)
        {
            List<BSale> trades = new List<BSale>();
            long totalTrades = 0;
            bool hasNextPage = false;            
            long totalPage = 0;
            long curPage = 1;
            if (!string.IsNullOrEmpty(status) && this.status.Contains(status))
            {
                List<BSale> tmpTrades = this.SyncTrades(sDate, eDate, status, curPage, out totalTrades, out hasNextPage);
                if (tmpTrades != null && tmpTrades.Count > 0)
                {
                    trades = trades.Concat(tmpTrades).ToList<BSale>();
                }
                //if (totalTrades % pageSize == 0)
                //{
                //    totalPage = totalTrades / pageSize;
                //}
                //else
                //{
                //    totalPage = totalTrades / pageSize + 1;
                //}

                //while ((totalPage - 1) >= 0)
                //{
                //    tmpTrades = this.SyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                //    if (tmpTrades != null)
                //    {
                //        trades = trades.Concat(tmpTrades).ToList<BSale>();
                //    }
                //    totalPage--;
                //}                

                while (hasNextPage)
                {
                    curPage++;
                    tmpTrades = this.SyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                    if (tmpTrades != null && tmpTrades.Count>0)
                    {
                        trades = trades.Concat(tmpTrades).ToList<BSale>();
                    }
                }
            }
            else
            {
                foreach (string state in this.status)
                {
                    List<BSale> tmpTrades = this.SyncTrades(sDate, eDate, state, curPage, out totalTrades, out hasNextPage);
                    if (tmpTrades != null && tmpTrades.Count > 0)
                    {
                        trades = trades.Concat(tmpTrades).ToList<BSale>();
                    }
                    //if (totalTrades % pageSize == 0)
                    //{
                    //    totalPage = totalTrades / pageSize;
                    //}
                    //else
                    //{
                    //    totalPage = totalTrades / pageSize + 1;
                    //}

                    //while ((totalPage - 1) >= 0)
                    //{
                    //    tmpTrades = this.SyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                    //    if (tmpTrades != null)
                    //    {
                    //        trades = trades.Concat(tmpTrades).ToList<BSale>();
                    //    }
                    //    totalPage--;
                    //}

                    while (hasNextPage)
                    {
                        curPage++;
                        tmpTrades = this.SyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                        if (tmpTrades != null && tmpTrades.Count > 0)
                        {
                            trades = trades.Concat(tmpTrades).ToList<BSale>();
                        }
                    }
                }
            }
            return trades;
        }

        /// <summary>
        /// Sync mall trades by trade's modified time which interval is one day.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="status"></param>
        /// <param name="totalTrades"></param>
        /// <param name="hasNextPage"></param>
        /// <returns></returns>
        public List<BSale> IncrementSyncMallTrades(long lastEndTime, long timeEnd, string status)
        {
            List<BSale> trades = new List<BSale>();
            long totalTrades = 0;
            bool hasNextPage = false;
           
            long totalPage = 0;
            long curPage=1;
            DateTime sDate = DateTime.MinValue;
            DateTime eDate = DateTime.MinValue;
            long timeNow = timeEnd; //DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);           
            long endTime = 0;
            if (lastEndTime == 0)
            {
                eDate = DateTime.Now;
                sDate = eDate.AddDays(-1);                
            }
            else
            {
                sDate = DateTimeUtil.ConvertToDateTime(lastEndTime);
                
                eDate = sDate.AddDays(1);
                endTime = DateTimeUtil.ConvertDateTimeToInt(eDate);
            }

            if (!string.IsNullOrEmpty(status))
            {
                while (endTime <= timeNow)
                {
                    DateTime tmpDate = DateTimeUtil.ConvertToDateTime(endTime);
                    List<BSale> tmpTrades = this.IncrementSyncTrades(sDate, eDate, status, curPage, out totalTrades, out hasNextPage);
                    //if (totalTrades % pageSize == 0)
                    //{
                    //    totalPage = totalTrades / pageSize;
                    //}
                    //else
                    //{
                    //    totalPage = totalTrades / pageSize + 1;
                    //}

                    //while ((totalPage - 1) >= 0)
                    //{
                    //    tmpTrades = this.IncrementSyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                    //    if (tmpTrades != null)
                    //    {
                    //        trades = trades.Concat(tmpTrades).ToList<BSale>();
                    //    }
                    //    totalPage--;
                    //}

                    if (tmpTrades != null)
                    {
                        trades = trades.Concat(tmpTrades).ToList<BSale>();
                    }
                    while (hasNextPage)
                    {
                        curPage++;
                        tmpTrades = this.IncrementSyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);

                        if (tmpTrades != null)
                        {
                            trades = trades.Concat(tmpTrades).ToList<BSale>();
                        }                        
                    }

                    sDate = DateTimeUtil.ConvertToDateTime(endTime);
                    if (endTime == timeNow)
                    {
                        break;
                    }
                    else if ((endTime + 24 * 3600 * 1000) > timeNow)
                    {
                        endTime = timeNow;
                    }
                    else
                    {
                        endTime += 24 * 3600 * 1000;
                    }
                    eDate = DateTimeUtil.ConvertToDateTime(endTime);                    
                }
            }
            else
            {
                
                foreach (string state in this.status)
                {
                    long tmpendTime = endTime;
                    while (tmpendTime <= timeNow)
                    {
                        DateTime tmpDate = DateTimeUtil.ConvertToDateTime(endTime);
                        List<BSale> tmpTrades = this.IncrementSyncTrades(sDate, eDate, state, curPage, out totalTrades, out hasNextPage);
                        //if (totalTrades % pageSize == 0)
                        //{
                        //    totalPage = totalTrades / pageSize;
                        //}
                        //else
                        //{
                        //    totalPage = totalTrades / pageSize + 1;
                        //}

                        //while ((totalPage - 1) >= 0)
                        //{
                        //    tmpTrades = this.IncrementSyncTrades(sDate, eDate, status, totalPage, out totalTrades, out hasNextPage);
                        //    if (tmpTrades != null)
                        //    {
                        //        trades = trades.Concat(tmpTrades).ToList<BSale>();
                        //    }
                        //    totalPage--;
                        //}

                        if (tmpTrades != null)
                        {
                            trades = trades.Concat(tmpTrades).ToList<BSale>();
                        }
                        while (hasNextPage)
                        {
                            curPage++;
                            tmpTrades = this.IncrementSyncTrades(sDate, eDate, state, totalPage, out totalTrades, out hasNextPage);

                            if (tmpTrades != null)
                            {
                                trades = trades.Concat(tmpTrades).ToList<BSale>();
                            }
                        }

                        sDate = DateTimeUtil.ConvertToDateTime(tmpendTime);
                        if (tmpendTime == timeNow)
                        {
                            break;
                        }
                        else if ((tmpendTime + 24 * 3600 * 1000) > timeNow)
                        {
                            tmpendTime = timeNow;
                        }
                        else 
                        {
                            tmpendTime += 24 * 3600 * 1000;
                        }
                        eDate = DateTimeUtil.ConvertToDateTime(tmpendTime);
                    }
                }
            }

            return trades;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <param name="totalTrades"></param>
        /// <param name="hasNextPage"></param>
        /// <returns></returns>
        private List<BSale> IncrementSyncTrades(DateTime? sDate, DateTime? eDate, string status, long page, out long totalTrades, out bool hasNextPage)
        {
            totalTrades = 0;
            hasNextPage = false;
            TradesSoldIncrementGetRequest req = new TradesSoldIncrementGetRequest();
            req.Fields = "total_fee,buyer_nick,created,tid,status, payment, discount_fee, adjust_fee, post_fee,price,adjust_fee,receiver_city,receiver_district,receiver_name,receiver_state,receiver_mobile,receiver_phone,received_payment";
            req.Fields += ",orders.title,orders.pic_path,orders.price,orders.num,orders.iid,orders.num_iid,orders.sku_id,orders.refund_status,orders.status,orders.oid,orders.total_fee,orders.payment,orders.discount_fee,orders.adjust_fee,orders.sku_properties_name,orders.item_meal_name,orders.buyer_rate,orders.seller_rate,orders.outer_iid,orders.outer_sku_id,orders.refund_id,orders.seller_type";

            if (sDate != null)
            {
                req.StartModified = sDate;
            }

            if (eDate != null)
            {
                req.EndModified = eDate;
            }

            if (!string.IsNullOrEmpty(status))
            {
                req.Status = status;
            }
            //req.BuyerNick = "zhangsan";
            //req.Type = "game_equipment";
            //req.ExtType = "service";
            //req.RateStatus = "RATE_UNBUYER";
            //req.Tag = "time_card";
            req.PageNo = page;
            req.PageSize = 50L;
            req.UseHasNext = true;
            TradesSoldIncrementGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);
            List<BSale> sales = new List<BSale>();
            if (response.IsError)
            {
                return sales;
            }
            if (response.Trades != null)
            {
                hasNextPage = response.HasNext;
                totalTrades = response.TotalResults;
                foreach (TB.Trade trade in response.Trades)
                {
                    bool containRefound = false;
                    BSale sale = new BSale();
                    sale.Status = trade.Status;
                    sale.SaleDateTime = DateTimeUtil.ConvertDateTimeToInt(Convert.ToDateTime(trade.Created));
                    sale.Sale_ID = trade.Tid.ToString();
                    sale.Orders = new List<BOrder>();
                    sale.Synced = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    if (!string.IsNullOrEmpty(trade.PostFee))
                    {
                        sale.Post_Fee = double.Parse(trade.PostFee);
                    }
                    if (!string.IsNullOrEmpty(trade.Payment))
                    {
                        sale.Amount = double.Parse(trade.Payment);
                    }
                    sale.Buyer = new BCustomer() { Type = new BMallType() { ID = this.Mall_Type_ID }, Mall_ID = trade.BuyerNick, Name = trade.ReceiverName, Address = trade.ReceiverAddress, Phone = trade.ReceiverMobile, Province = new Common_District { name = trade.ReceiverState }, City = new Common_District() { name = trade.ReceiverCity } };
                    if (trade.Orders != null)
                    {
                        foreach (TB.Order o in trade.Orders)
                        {
                            //if (string.IsNullOrEmpty(o.OuterIid))
                            //{
                            //    continue;
                            //}
                            BOrder order = new BOrder();
                            if (!string.IsNullOrEmpty(o.Payment))
                            {
                                order.Amount = double.Parse(o.Payment);
                            }

                            if (!string.IsNullOrEmpty(o.Price))
                            {
                                order.Price = double.Parse(o.Price);
                            }

                            order.Quantity = int.Parse(o.Num.ToString());
                            order.Status = o.Status;
                            order.Status1 = 0;
                            if (!string.IsNullOrEmpty(o.RefundStatus) && o.RefundStatus == "SUCCESS")
                            {
                                containRefound = true;
                                order.Status1 = 1;
                            }
                            order.StockStatus = 0;
                            order.Discount = string.IsNullOrEmpty(o.DiscountFee) ? double.Parse(o.DiscountFee) : 0;


                            if (!string.IsNullOrEmpty(o.OuterIid))
                            {
                                int pid = 0;
                                int.TryParse(o.OuterIid, out pid);
                                order.Parent_Product_ID = pid;

                                if (!string.IsNullOrEmpty(o.OuterSkuId))
                                {
                                    int pcid = 0;
                                    int.TryParse(o.OuterSkuId, out pcid);
                                    order.Product_ID = pcid;
                                }
                                else
                                {
                                    order.Product_ID = order.Parent_Product_ID;
                                }
                            }
                            order.Order_ID = o.Oid.ToString();
                            order.Mall_PID = o.NumIid.ToString();
                            order.ImageUrl = "";
                            if (!string.IsNullOrEmpty(o.PicPath))
                            {
                                order.ImageUrl = o.PicPath;
                            }

                            sale.Orders.Add(order);
                        }
                    }

                    sale.HasRefound = containRefound;
                    sales.Add(sale);
                }
            }
            return sales;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sDate"></param>
        /// <param name="eDate"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private List<BSale> SyncTrades(DateTime? sDate, DateTime? eDate, string status, long page, out long totalTrades, out bool hasNextPage, bool onlyReFound = false)
        {
            totalTrades = 0;
            hasNextPage = false;
            TradesSoldGetRequest req = new TradesSoldGetRequest();
            req.Fields = "total_fee,buyer_nick,created,tid,status, payment, discount_fee, adjust_fee, post_fee,price,adjust_fee,receiver_city,receiver_district,receiver_name,receiver_state,receiver_mobile,receiver_phone,received_payment";
            req.Fields += ",orders.title,orders.pic_path,orders.price,orders.num,orders.iid,orders.num_iid,orders.sku_id,orders.refund_status,orders.status,orders.oid,orders.total_fee,orders.payment,orders.discount_fee,orders.adjust_fee,orders.sku_properties_name,orders.item_meal_name,orders.buyer_rate,orders.seller_rate,orders.outer_iid,orders.outer_sku_id,orders.refund_id,orders.seller_type";

            if (sDate != null)
            {
                req.StartCreated = sDate;
            }

            if (eDate != null)
            {
                req.EndCreated = eDate;
            }

            if (!string.IsNullOrEmpty(status))
            {
                req.Status = status;
            }
            //req.BuyerNick = "zhangsan";
            //req.Type = "game_equipment";
            //req.ExtType = "service";
            //req.RateStatus = "RATE_UNBUYER";
            //req.Tag = "time_card";
            req.PageNo = page;
            req.PageSize = 50L;
            req.UseHasNext = true;
            TradesSoldGetResponse response = client.Execute(req, this.Access_Token.Access_Token1);

            List<BSale> sales = new List<BSale>();
            if (response.IsError) 
            {
                return sales;
            }

            if (response.Trades != null)
            {
                hasNextPage = response.HasNext;
                totalTrades = response.TotalResults;
                foreach (TB.Trade trade in response.Trades)
                {
                    if (trade.Tid <= 0) 
                    {
                        continue;
                    }
                    var existedSales=from esale in sales where esale.Sale_ID==trade.Tid.ToString() select esale;
                    if (existedSales.Count() > 0)
                    {
                        continue;
                    }

                    bool containRefound = false;
                    BSale sale = new BSale();
                    sale.Status=trade.Status;
                    sale.SaleDateTime = DateTimeUtil.ConvertDateTimeToInt( Convert.ToDateTime(trade.Created));
                    sale.Sale_ID = trade.Tid.ToString();
                    sale.Orders = new List<BOrder>();
                    sale.Synced = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    if (!string.IsNullOrEmpty(trade.PostFee))
                    {
                        sale.Post_Fee = double.Parse(trade.PostFee);
                    }
                    if (!string.IsNullOrEmpty(trade.Payment))
                    {
                        sale.Amount = double.Parse(trade.Payment);
                    }

                    sale.Province = new Common_District() { name=trade.ReceiverState };
                    sale.City = new Common_District() { name = trade.ReceiverState };
                    sale.Buyer = new BCustomer() {Type=new BMallType(){ ID=this.Mall_Type_ID},  Mall_ID = trade.BuyerNick, Name = trade.ReceiverName, Address = trade.ReceiverAddress, Phone = trade.ReceiverMobile, Province = new Common_District { name = trade.ReceiverState }, City = new Common_District() { name=trade.ReceiverCity } };
                    if (trade.Orders != null)
                    {
                        foreach (TB.Order o in trade.Orders)
                        {
                            if (o.Oid <= 0)
                            {
                                continue;
                            }

                            var existedOrders = from so in sale.Orders where so.Order_ID == o.Oid.ToString() select so;
                            if (existedOrders.Count() > 0)
                            {
                                continue;
                            }

                            //if (string.IsNullOrEmpty(o.OuterIid))
                            //{
                            //    continue;
                            //}
                            BOrder order = new BOrder();
                            if (!string.IsNullOrEmpty(o.Payment))
                            {
                                order.Amount = double.Parse(o.Payment);
                            }

                            if (!string.IsNullOrEmpty(o.Price))
                            {
                                order.Price = double.Parse(o.Price);
                            }
                           
                            order.Quantity = int.Parse(o.Num.ToString());
                            order.Status = o.Status;
                            order.Status1 = 0;
                            if (!string.IsNullOrEmpty(o.RefundStatus) && o.RefundStatus == "SUCCESS") 
                            {
                                containRefound = true;
                                order.Status1 = 1;
                                sale.Amount = sale.Amount - order.Amount;
                            }
                            order.StockStatus = 0;
                            order.Discount = string.IsNullOrEmpty(o.DiscountFee) ? double.Parse(o.DiscountFee) : 0;
                           

                            if (!string.IsNullOrEmpty(o.OuterIid))
                            {
                                int pid = 0;
                                int.TryParse(o.OuterIid,out pid);
                                order.Parent_Product_ID = pid;

                                if (!string.IsNullOrEmpty(o.OuterSkuId))
                                {
                                    int pcid = 0;
                                    int.TryParse(o.OuterSkuId,out pcid);
                                    order.Product_ID = pcid;
                                }
                                else 
                                {
                                    order.Product_ID = order.Parent_Product_ID;
                                }
                            }
                            order.Order_ID = o.Oid.ToString();
                            order.Mall_PID = o.NumIid.ToString();
                            order.ImageUrl = "";
                            if (!string.IsNullOrEmpty(o.PicPath))
                            {
                                order.ImageUrl = o.PicPath;
                            }

                            sale.Orders.Add(order);
                        }
                    }

                    sale.HasRefound = containRefound;

                    if (onlyReFound)
                    {
                        if (containRefound)
                        {
                            sales.Add(sale);
                        }
                    }
                    else
                    {
                        sales.Add(sale);
                    }
                }
            }
            return sales;
        }
    }
}
