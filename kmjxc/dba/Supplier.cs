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
    
    public partial class Supplier
    {
        public int Supplier_ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string PostalCode { get; set; }
        public int Province_ID { get; set; }
        public int City_ID { get; set; }
        public long Create_Time { get; set; }
        public int User_ID { get; set; }
        public string Contact_Person { get; set; }
        public int Shop_ID { get; set; }
        public Nullable<bool> Enabled { get; set; }
        public string Remark { get; set; }
        public long Modified { get; set; }
        public int Modified_By { get; set; }
    }
}
