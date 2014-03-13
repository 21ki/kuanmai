using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.Open.Interface;
using KM.JXC.DBA;
namespace KM.JXC.Open.TaoBao
{
    public class TaoBaoOpen:IOpen
    {
        public TaoBaoOpen()
        {
        }

        public Access_Token GetAccessToken(string code)
        {
            throw new NotImplementedException();
        }
    }
}
