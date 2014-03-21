using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.Common.KMException
{ 
    public enum ExceptionLevel
    {
        SYSTEM,
        USER,
        DEBUG,
        ERROR,
    }

    public class KMJXCException:Exception
    {
        private string message;

        public ExceptionLevel Level = ExceptionLevel.USER;

        public KMJXCException(string message)
            : base(message)
        {
            this.message=message;
        }

        public KMJXCException(string message,Exception ex)
            : base(message,ex)
        {
            this.message = message;
        }

        public KMJXCException(string message, ExceptionLevel level)
            : base(message)
        {
            this.message = message;
            this.Level = level;
        }

        public override string ToString()
        {
            return this.message;
        }
    }
}
