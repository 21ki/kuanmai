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

using TB=Top.Api.Domain;
using Top.Tmc;
using Top.Api.Request;
using Top.Api.Response;

namespace KM.JXC.BL.Open.Interface
{
    public interface IOTradeManager
    {
        void SyncSingleTrade(string trade_id);
        List<BSale> IncrementSyncMallTrades(int lastSyncTime,int timeEnd, string status);
        List<BSale> SyncMallTrades(DateTime? sDate,DateTime? edate, string status);        
    }
}
