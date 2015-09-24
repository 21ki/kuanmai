using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public enum KMBitExceptionType {
         INFO,
         ERROR,
         WARN
    }

    public class KMBitException:Exception
    {
        public KMBitExceptionType Type { get; private set; }
        public KMBitException(KMBitExceptionType type= KMBitExceptionType.WARN)
        {
            this.Type = type;
        }

        public KMBitException(string message, KMBitExceptionType type = KMBitExceptionType.INFO) : base(message)
        {
            this.Type = type;
        }

        public KMBitException(string message, Exception inner, KMBitExceptionType type = KMBitExceptionType.INFO) : base(message, inner)
        {
            this.Type = type;
        }
    }
}
