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
    
    public partial class Sale
    {
        public int Sale_ID { get; set; }
        public long Sale_Time { get; set; }
        public int Shop_ID { get; set; }
        public Nullable<int> User_ID { get; set; }
        public string Mall_Trade_ID { get; set; }
        public decimal Amount { get; set; }
        public string Express_Cop { get; set; }
        public int Province_ID { get; set; }
        public Nullable<short> Sale_Type { get; set; }
    }
}
