using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BAdminRole
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public List<BAdminAction> Actions { get; set; }
        public BShop Shop { get; set; }
    }

    public class BAdminAction
    {
        public int ID { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public BAdminCategory Category { get; set; }
        
    }

    public class BAdminCategory
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Created { get; set; }
    }

    public class BAdminCategoryAction
    {
        public BAdminCategory Category { get; set; }
        public List<BAdminAction> Actions { get; set; }
    }
}
