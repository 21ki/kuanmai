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
    
    public partial class Shop
    {
        public long Shop_ID { get; set; }
        public string Name { get; set; }
        public long User_ID { get; set; }
        public Nullable<long> Main_Shop_ID { get; set; }
        public string Description { get; set; }
        public Nullable<long> Mall_Type_ID { get; set; }
        public string Mall_Shop_ID { get; set; }
    }
}
