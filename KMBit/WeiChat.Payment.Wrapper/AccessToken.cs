using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeChat.Adapter
{
    public class AccessToken
    {
        public string Access_Token { get; set; }
        //In seconds
        public int ExpiresIn { get; set; }
        public DateTime ExpiresTime { get; set; }
        public string Refresh_Token { get; set; }
        public string OpenId { get; set; }
        public string Scope { get; set; }
    }
}
