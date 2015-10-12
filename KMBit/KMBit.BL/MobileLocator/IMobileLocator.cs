using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
namespace KMBit.BL.MobileLocator
{

    public interface IMobileLocator
    {
        BMobileLocation GetMobileLocation(string phone);
    }
}
