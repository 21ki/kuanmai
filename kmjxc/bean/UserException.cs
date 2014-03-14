using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.Common.KMException
{
    public class UserException:Exception
    {
        public UserException() { 
            
        }

        public UserException(string message)
            : base(message)
        {
            
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
