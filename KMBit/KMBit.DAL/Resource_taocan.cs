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
    
    public partial class Resource_taocan
    {
        public int Id { get; set; }
        public int Resource_id { get; set; }
        public int Sp_id { get; set; }
        public int Area_id { get; set; }
        public int Taocan_id { get; set; }
        public long Created_time { get; set; }
        public long Updated_time { get; set; }
        public float Sale_price { get; set; }
        public float Purchase_price { get; set; }
        public int Quantity { get; set; }
        public bool Enabled { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string Serial { get; set; }
        public float Resource_Discount { get; set; }
        public bool EnableDiscount { get; set; }
        public int City_id { get; set; }
        public int NumberProvinceId { get; set; }
        public int NumberCityId { get; set; }
    }
}
