using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
namespace KMBit.BL.Charge
{
    public class ChargeResult
    {
       public ChargeStatus Status { get; set; }
       public string Message { get; set; }
    }
    public enum ChargeStatus
    {
        PENDIND,
        ONPROGRESS,
        SUCCEED,
        FAILED
    }
    public interface ICharge
    {
        ChargeResult Charge(ChargeOrder order);
        void CallBack(List<WebRequestParameters> data);
    }
}
