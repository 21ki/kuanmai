//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KM.JXC.DBA
{
    using System;
    using System.Collections.Generic;
    
    public partial class Back_Stock_Detail
    {
        public int Back_Stock_ID { get; set; }
        public int Product_ID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public int Parent_Product_ID { get; set; }
        public Nullable<int> StoreHouse_ID { get; set; }
    }
}
