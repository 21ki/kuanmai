using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using KMBit.Beans;
using KMBit.DAL;
using KMBit.BL.Charge;
namespace KMBit.BL.Charge
{
    public class ChongBaCharge : HttpService, ICharge
    {
        public ChongBaCharge()
        {
        }

        public ChargeResult AgencyCharge(AgencyChargeOrder order)
        {
            throw new NotImplementedException();
        }

        public ChargeResult Charge(ChargeOrder order)
        {
            ChargeResult result = new ChargeResult();
            List<WebRequestParameters> parmeters = new List<WebRequestParameters>();
            bool succeed = false;
            
            try
            {
                SendRequest(parmeters, false, out succeed);
                if (!string.IsNullOrEmpty(this.Response))
                {
                    JObject jsonResult = JObject.Parse(this.Response);
                }
            }
            catch(Exception ex)
            {

            }
            return result;
        }
    }
}
