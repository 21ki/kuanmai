//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KMBit.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class Charge_history
    {
        public int Id { get; set; }
        public Nullable<int> User_id { get; set; }
        public int Resource_id { get; set; }
        public int Resource_taocan_id { get; set; }
        public string Phone_number { get; set; }
        public long Process_time { get; set; }
        public long Created_time { get; set; }
        public float Sale_price { get; set; }
        public float Purchase_price { get; set; }
        public Nullable<sbyte> Charge_type { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}