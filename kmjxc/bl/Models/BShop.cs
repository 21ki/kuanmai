﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class BShop
    {
        public int ID { get; set; }
        public string Mall_ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<BShop> Chindren { get; set; }
        public BShop Parent { get; set; }
        public Mall_Type Type { get; set; }
        public int Created { get; set; }
        public int Synced { get; set; }
    }
}