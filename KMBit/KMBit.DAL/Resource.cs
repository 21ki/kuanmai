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
    
    public partial class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Province_Id { get; set; }
        public Nullable<int> City_Id { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
    }
}
