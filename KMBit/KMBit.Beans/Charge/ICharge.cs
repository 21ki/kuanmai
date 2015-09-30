using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans.Charge
{
    public enum ChargeResultStatus
    {
        SUCCEED,
        NO_ENOUGH_AMOUNT,
        USER_DISABLED,
        ROUTE_DISABLED,
        RESOURCE_DISABLED,
        TAOCAN_DISABLED,
        NO_ENOUGH_CREDIT
    }
    public interface ICharge
    {
        ChargeResultStatus Charge(int agentId,int routeId, int resourceTaocanId,string phoneNumber);
    }
}
