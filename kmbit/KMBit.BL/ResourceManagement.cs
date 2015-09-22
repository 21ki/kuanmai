﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.DAL;
using KMBit.Beans;
using KMBit.Util;

namespace KMBit.BL
{
    public class ResourceManagement:BaseManagement
    {
        public ResourceManagement(int userId):base(userId)
        {
            if (this.logger == null)
            {
                this.logger = log4net.LogManager.GetLogger(typeof(ResourceManagement));
            }
        }

        public bool CreateResource(Resource resource)
        {
            bool ret = false;

            if(CurrentLoginUser.Permission.CREATE_RESOURCE==0)
            {
                throw new KMBitException("没有权限创建资源");
            }

            if (resource == null)
            {
                logger.Error("resource is NULL");
                throw new KMBitException("资源输入参数不正确");
            }

            if (string.IsNullOrEmpty(resource.Name))
            {
                logger.Error("resource name cannot be empty");
                throw new KMBitException("资源名称不能为空");
            }
            resource.Enabled = true;
            using (chargebitEntities db = new chargebitEntities())
            {
                db.Resource.Add(resource);
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public bool UpdateResource(Resource resource)
        {
            if (CurrentLoginUser.Permission.UPDATE_RESOURCE == 0)
            {
                throw new KMBitException("没有权限更新资源");
            }
            bool ret = false;
            if (resource == null)
            {
                logger.Error("resource is NULL");
                throw new KMBitException("资源输入参数不正确");
            }

            if (string.IsNullOrEmpty(resource.Name))
            {
                logger.Error("resource name cannot be empty");
                throw new KMBitException("资源名称不能为空");
            }

            using (chargebitEntities db = new chargebitEntities())
            {
                if(resource.Id>0)
                {
                    db.Resource.Attach(resource);
                }
                db.SaveChanges();
                ret = true;
            }

            return ret;
        }

        public bool CreateResourceTaocan(Resource_taocan taocan)
        {
            bool ret = false;
            if (CurrentLoginUser.Permission.CREATE_RESOURCE_TAOCAN == 0)
            {
                throw new KMBitException("没有权限创建资源套餐");
            }

            if (taocan.Quantity <= 0)
            {
                throw new KMBitException("套餐容量不能为零");
            }

            taocan.Enabled = true;
            taocan.Created_time = DateTimeUtil.ConvertDateTimeToInt(DateTime.Now);
            taocan.Updated_time = taocan.Created_time;
            using (chargebitEntities db = new chargebitEntities())
            {
                Taocan ntaocan = (from t in db.Taocan where t.Sp_id == taocan.Sp_id && t.Quantity == taocan.Quantity select t).FirstOrDefault<Taocan>();
                Sp sp = (from s in db.Sp where s.Id == taocan.Sp_id select s).FirstOrDefault<Sp>();
                if (ntaocan == null)
                {
                    string taocanName = sp != null ? sp.Name + " " + taocan.Quantity.ToString() + "M" : "全网 " + taocan.Quantity.ToString() + "M";
                    ntaocan = new Taocan() { Created_time = taocan.Created_time, Description = taocanName, Name = taocanName, Sp_id = taocan.Sp_id, Quantity = taocan.Quantity, Updated_time = taocan.Updated_time };
                    db.Taocan.Add(ntaocan);
                    db.SaveChanges();
                }
                if (ntaocan.Id > 0)
                {
                    db.Resource_taocan.Add(taocan);
                    db.SaveChanges();
                    ret = true;
                }
                else
                {
                    throw new KMBitException("套餐创建失败");
                }

            }
            return ret;
        }
    }
}