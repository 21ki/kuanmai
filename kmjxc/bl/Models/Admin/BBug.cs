using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models.Admin
{
    public class BBug:BAdminBase
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public BBugFeature Feature { get; set; }
        public BBugStatus Status { get; set; }
        public BUser Resolved_By { get; set; }
        public List<BBugResponse> Responses { get; set; }
        public long Resolved { get; set; }
    }
}
