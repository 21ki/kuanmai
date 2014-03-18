using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using KM.JXC.DBA;
using KM.JXC.Common.KMException;
using KM.JXC.Open.Interface;
using KM.JXC.Open.TaoBao;
namespace KM.JXC.BL
{
    public class StockManager
    {
        private User currentUser = null;

        public StockManager(User user)
        {
            if (user == null)
            {
                throw new KMJXCException("Must supplier user instance when trying to use StockManager");
            }

            this.currentUser = user;
        }

        /// <summary>
        /// Add new enter stock record
        /// </summary>
        /// <param name="stock">Instance of Enter_Stock object</param>
        /// <returns></returns>
        public bool AddEnterStock(Enter_Stock stock)
        {
            bool result = false;
            if (stock == null)
            {
                return result;
            }

            if (stock.Buy_ID <= 0) {
                throw new KMJXCException("入库单未包含任何采购单信息");
            }

            if (stock.Shop_ID <= 0)
            {
                throw new KMJXCException("入库单未包含店铺信息");
            }

            if (stock.StoreHouse_ID <= 0)
            {
                throw new KMJXCException("入库单未包含任何仓库信息");
            }

            if (stock.User_ID <= 0)
            {
                stock.User_ID = this.currentUser.User_ID;
            }

            using (KuanMaiEntities db = new KuanMaiEntities())
            {
                db.Enter_Stock.Add(stock);
                db.SaveChanges();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stock"></param>
        /// <returns></returns>
        public bool AddEnterStockDetail(List<Enter_Stock> details)
        {
            bool result = false;

            return result;
        }
    }
}
