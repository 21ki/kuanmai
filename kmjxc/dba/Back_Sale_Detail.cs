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
    
    public partial class Back_Sale_Detail
    {
        public int Back_Sale_ID { get; set; }
        public int Product_ID { get; set; }
        public string Order_ID { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public Nullable<long> Created { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Parent_Product_ID { get; set; }
        public double Refound { get; set; }
    }
}
