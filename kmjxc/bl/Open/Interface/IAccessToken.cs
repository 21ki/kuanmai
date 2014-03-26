using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Open.Interface
{
    public interface IAccessToken
    {
        Access_Token RequestAccessToken(string code);
        Access_Token RefreshToken(Access_Token oldToken);       
    }
}
