using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using KM.JXC.DBA;

namespace KM.JXC.Open
{
    public class BaseAccessToken
    {
        protected long Mall_Type_ID { get; set; }

        protected Open_Key OpenKey { get; set; }

        public BaseAccessToken(long mall_type_id) 
        {
            this.Mall_Type_ID = mall_type_id;           
            this.GetAppKey();
        }

        private void GetAppKey()
        {
            if (this.Mall_Type_ID <= 0) {
                throw new Exception("Mall Type ID is invalid for open API");
            }

            try
            {
                KuanMaiEntities db = new KuanMaiEntities();
                var openKey = db.Open_Key.Where(p => p.Mall_Type_ID == this.Mall_Type_ID);
                if (openKey != null)
                {
                    List<Open_Key> keys = openKey.ToList<Open_Key>();
                    if (keys.Count == 1)
                    {
                        this.OpenKey = keys[0];   
                    }
                    else
                    {
                        throw new Exception("More app key and secret pair for Mall Type ID:" + this.Mall_Type_ID);
                    }
                }
                else
                {
                    throw new Exception("Didn't find app key and secret for Mall Type ID:" + this.Mall_Type_ID);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Nothing to do
            }
        }

        public Access_Token GetLocAccessToken(long user_id) 
        {
            Access_Token token = null;

            KuanMaiEntities db = new KuanMaiEntities();

            var etoken = from p in db.Access_Token where p.Mall_Type_ID == this.Mall_Type_ID && p.User_ID == user_id select p;

            if (etoken != null)
            {
                token = etoken.ToList<Access_Token>()[0];
            }

            return token;
        }
    }
}
