using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
using KM.JXC.BL.Models;
namespace KM.JXC.BL.Open.Interface
{
    public interface IShopManager
    {
        Shop GetShop(BUser user);        
    }
}
