using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBit.Beans;
namespace KMBit.BL
{
    public static class StaticDictionary
    {
        public static List<DictionaryTemplate> GetChargeStatusList()
        {
            List<DictionaryTemplate> list = new List<DictionaryTemplate>();
            list.Add(new DictionaryTemplate() { Id=10, Value="待充值" });
            list.Add(new DictionaryTemplate() { Id = 1, Value = "充值中" });
            list.Add(new DictionaryTemplate() { Id = 2, Value = "成功" });
            list.Add(new DictionaryTemplate() { Id = 3, Value = "失败" });
            list.Add(new DictionaryTemplate() { Id = 11, Value = "未支付" });
            return list;
        }

        public static List<DictionaryTemplate> GetChargeTypeList()
        {
            List<DictionaryTemplate> list = new List<DictionaryTemplate>();
            list.Add(new DictionaryTemplate() { Id = 0, Value = "前台直充" });
            list.Add(new DictionaryTemplate() { Id = 1, Value = "代理商直充" });
            list.Add(new DictionaryTemplate() { Id = 2, Value = "后台直充" });
            list.Add(new DictionaryTemplate() { Id = 3, Value = "扫码充值" });
            list.Add(new DictionaryTemplate() { Id = 4, Value = "卡密充值" });
            return list;
        }

        public static List<DictionaryTemplate> GetTranfserTypeList()
        {
            List<DictionaryTemplate> list = new List<DictionaryTemplate>();
            list.Add(new DictionaryTemplate() { Id = 1, Value = "支付宝" });
            list.Add(new DictionaryTemplate() { Id = 2, Value = "网银" });
            return list;
        }
    }
}
