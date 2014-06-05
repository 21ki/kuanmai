using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web;
namespace KM.JXC.Web.Controllers.api
{
    public class BaseApiController : ApiController
    {
        public int[] ConvertToIntArrar(string str)
        {
            int[] ids = new int[0];

            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            string[] ss = str.Split(',');
            if (ss.Length > 0)
            {
                ids=new int[ss.Length];
                for (int i = 0; i < ss.Length; i++)
                {
                    int id = 0;
                    int.TryParse(ss[i],out id);
                    ids[i] = id;
                }
               
            }

            return ids;
        }
    }
}
