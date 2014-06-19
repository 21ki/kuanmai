using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models.Admin
{
    public class BBugResponse:BAdminBase
    {
        public string Description { get; set; }
        public BBug Bug { get; set; }
    }
}
