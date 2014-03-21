﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KM.JXC.DBA;
namespace KM.JXC.BL.Models
{
    public class EnterStockDetail
    {
        public EnterStock EnterStock { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public bool Invoiced { get; set; }
        public int InvoiceNumber { get; set; }
        public double InvoiceAmount { get; set; }
    }
}