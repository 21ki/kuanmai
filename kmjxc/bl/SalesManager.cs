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
    public class SalesManager:BBaseManager
    {
        public SalesManager(BUser user, int shop_id, Permission permission)
            : base(user, shop_id,permission)
        {
        }

        public SalesManager(BUser user, Permission permission)
            : base(user,permission)
        {
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
