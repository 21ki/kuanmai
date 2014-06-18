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
        public ExceptionLevel Level = ExceptionLevel.USER;

        public KMJXCException(string message)
            : base(message)
        {
        }

        public KMJXCException(string message,Exception ex)
            : base(message,ex)
        {
        }

        public KMJXCException(string message, ExceptionLevel level)
            : base(message)
        {
            this.Level = level;
        }

        public override string ToString()
        {
            return this.Message;
        }
    }

    public class KMJXCMallException : KMJXCException
    {
        private string errorCode = null;

        public string ErrorCode
        {
            get { return errorCode; }
            protected set { errorCode = value; }
        }
        public KMJXCMallException(string code,string message)
            : base(message)
        {
        }
    }

    public class KMJXCTaobaoException : KMJXCMallException
    {
        public KMJXCTaobaoException(string code, string message)
            : base(code,message)
        {
        }

        private void ParseCode()
        {
            if (this.ErrorCode == null)
            {
                return;
            }

            switch (this.ErrorCode)
            {
                case "1":
                    break;
                default:
                    break;
            }
        }
    }
}
