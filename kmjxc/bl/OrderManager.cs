using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data;

using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Common.Util;
using KM.JXC.BL.Open.Interface;
using KM.JXC.BL.Open.TaoBao;
using KM.JXC.BL.Models;

namespace KM.JXC.BL
{
    public class OrderManager:BBaseManager
    {
        public OrderManager(BUser user,int shop_id)
            : base(user, shop_id)
        {
        }

        public OrderManager(BUser user)
            : base(user)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backSale"></param>
        /// <param name="details"></param>
        public void BackSale(BBackSale backSale)
        {           
            if (backSale == null)
            {
                throw new KMJXCException("");
            }

            if (backSale.Trade == null || backSale.Trade.ID <= 0)
            {
                throw new KMJXCException("");
            }

            List<BBackSaleDetail> details=backSale.Details;

            if (details == null)
            {
                throw new KMJXCException("");
            }           

            KuanMaiEntities db = new KuanMaiEntities();

            try
            {
                Back_Sale sale=(from s in db.Back_Sale where s.Sale_ID==backSale.Trade.ID select s).FirstOrDefault<Back_Sale>();
                if (sale == null)
                {
                    sale = new Back_Sale();
                    sale.Back_Date = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
                    sale.Back_Sale_ID = 0;
                    sale.Description = backSale.Description;
                    sale.Sale_ID = backSale.Trade.ID;
                    sale.Shop_ID = this.Shop.Shop_ID;
                    sale.StoreHouse_ID = backSale.StoreHouse.StoreHouse_ID;
                    sale.User_ID = this.CurrentUser.ID;
                    db.Back_Sale.Add(sale);
                    db.SaveChanges();
                    if (sale.Back_Sale_ID > 0)
                    {
                        foreach (BBackSaleDetail detail in details)
                        {
                            if (detail.Product == null || detail.Product.ID <= 0)
                            {
                                continue;
                            }

                            Back_Sale_Detail d = new Back_Sale_Detail();
                            d.Back_Sale_ID = sale.Back_Sale_ID;
                            d.Created = (int)sale.Back_Date;
                            d.Description = detail.Description;
                            d.Price = decimal.Parse(detail.Price.ToString("0.00"));
                            d.Product_ID = detail.Product.ID;
                            d.Quantity = detail.Quantity;
                            d.Status = (short)detail.Status;
                            db.Back_Sale_Detail.Add(d);
                        }

                        db.SaveChanges();
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if(db!=null)
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<BBackSaleDetail> GetBackDetails(int backSaleId)
        {
            List<BBackSaleDetail> details = new List<BBackSaleDetail>();

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                details = (from bsd in db.Back_Sale_Detail
                           where bsd.Back_Sale_ID == backSaleId
                           select new BBackSaleDetail
                           {
                               Created = (int)bsd.Created,
                               Description = bsd.Description,
                               Price = double.Parse(bsd.Price.ToString("0.00")),
                               Quantity = bsd.Quantity,
                               Product = (from p in db.Product
                                          where bsd.Product_ID == p.Product_ID
                                          select new BProduct
                                           {
                                               ID = p.Product_ID,
                                               Title = p.Name,
                                               Description = p.Description,
                                               Category = new BCategory() { ID = p.Product_Class_ID },
                                               Code = p.Code,
                                               CreateTime = p.Create_Time,
                                               Price = p.Price,
                                               Quantity = (from sp in db.Stock_Pile where sp.Product_ID == p.Product_ID select sp.Quantity).FirstOrDefault<int>()
                                           }).FirstOrDefault<BProduct>()
                           }).ToList<BBackSaleDetail>();
            }

            return details;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public List<BBackSale> GetBackSales(int user_id,int category_id,string keyword, int pageIndex, int pageSize, int total)
        {
            List<BBackSale> sales = new List<BBackSale>();

            return sales;
        }
    }
}
