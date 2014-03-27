using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Open.Interface
{
    public interface IOManager
    {
        Mall_Type MallType { get; set; }
        Access_Token Access_Token { get; set; }
        Open_Key Open_Key { get; set; }
    }
}
