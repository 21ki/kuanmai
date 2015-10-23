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
            return list;
        }

        public static List<DictionaryTemplate> GetChargeTypeList()
        {
            List<DictionaryTemplate> list = new List<DictionaryTemplate>();
            list.Add(new DictionaryTemplate() { Id = 0, Value = "前台直冲" });
            list.Add(new DictionaryTemplate() { Id = 1, Value = "代理商充值" });
            list.Add(new DictionaryTemplate() { Id = 2, Value = "后台直冲" });
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
