﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class chargebitEntities : DbContext
    {
        public chargebitEntities()
            : base("name=chargebitEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Agent_route> Agent_route { get; set; }
        public DbSet<Area> Area { get; set; }
        public DbSet<Charge_history> Charge_history { get; set; }
        public DbSet<Payment_history> Payment_history { get; set; }
        public DbSet<Resrouce_interface> Resrouce_interface { get; set; }
        public DbSet<Sp> Sp { get; set; }
        public DbSet<Taocan> Taocan { get; set; }
        public DbSet<User_type> User_type { get; set; }
        public DbSet<Admin_Actions> Admin_Actions { get; set; }
        public DbSet<Admin_Categories> Admin_Categories { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Admin_Users_Actions> Admin_Users_Actions { get; set; }
        public DbSet<Admin_Users> Admin_Users { get; set; }
        public DbSet<Resource> Resource { get; set; }
        public DbSet<Resource_taocan> Resource_taocan { get; set; }
    }
}
