using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBit.Beans
{
    public class ReportTemplate
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public float CostAmount { get; set; }
        public float SalesAmount { get; set; }
        public float Revenue { get; set; }
    }

    public class ChartReport
    {
        public List<ReportTemplate> ResourceReport { get; set; }
        public List<ReportTemplate> UserReport { get; set; }
    }
}
