﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class KuanMaiEntities : DbContext
    {
        public KuanMaiEntities()
            : base("name=KuanMaiEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Access_Token> Access_Token { get; set; }
        public DbSet<Admin_Action> Admin_Action { get; set; }
        public DbSet<Admin_Role_Action> Admin_Role_Action { get; set; }
        public DbSet<Admin_Super> Admin_Super { get; set; }
        public DbSet<Admin_User_Action> Admin_User_Action { get; set; }
        public DbSet<Admin_User_Role> Admin_User_Role { get; set; }
        public DbSet<Back_Sale> Back_Sale { get; set; }
        public DbSet<Back_Sale_Detail> Back_Sale_Detail { get; set; }
        public DbSet<Back_Stock_Detail> Back_Stock_Detail { get; set; }
        public DbSet<Buy_Detail> Buy_Detail { get; set; }
        public DbSet<Buy_Order> Buy_Order { get; set; }
        public DbSet<Buy_Order_Detail> Buy_Order_Detail { get; set; }
        public DbSet<Common_District> Common_District { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Customer_Shop> Customer_Shop { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<Enter_Stock> Enter_Stock { get; set; }
        public DbSet<Enter_Stock_Detail> Enter_Stock_Detail { get; set; }
        public DbSet<Express> Express { get; set; }
        public DbSet<Express_Shop> Express_Shop { get; set; }
        public DbSet<Image> Image { get; set; }
        public DbSet<Leave_Stock> Leave_Stock { get; set; }
        public DbSet<Mall_Product> Mall_Product { get; set; }
        public DbSet<Mall_Product_Sku> Mall_Product_Sku { get; set; }
        public DbSet<Mall_Type> Mall_Type { get; set; }
        public DbSet<Open_Key> Open_Key { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Product_Spec> Product_Spec { get; set; }
        public DbSet<Product_Spec_Value> Product_Spec_Value { get; set; }
        public DbSet<Product_Specifications> Product_Specifications { get; set; }
        public DbSet<Product_Supplier> Product_Supplier { get; set; }
        public DbSet<Product_Unit> Product_Unit { get; set; }
        public DbSet<Sale> Sale { get; set; }
        public DbSet<Sale_Detail> Sale_Detail { get; set; }
        public DbSet<Sale_SyncTime> Sale_SyncTime { get; set; }
        public DbSet<Shop> Shop { get; set; }
        public DbSet<Shop_Child_Request> Shop_Child_Request { get; set; }
        public DbSet<Shop_User> Shop_User { get; set; }
        public DbSet<Stock_Waste> Stock_Waste { get; set; }
        public DbSet<Store_House> Store_House { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Supplier_Shop> Supplier_Shop { get; set; }
        public DbSet<SyncWithMall> SyncWithMall { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Buy> Buy { get; set; }
        public DbSet<Back_Stock> Back_Stock { get; set; }
        public DbSet<Stock_Pile> Stock_Pile { get; set; }
        public DbSet<Product_Class> Product_Class { get; set; }
        public DbSet<Admin_Role> Admin_Role { get; set; }
        public DbSet<Express_Fee> Express_Fee { get; set; }
        public DbSet<Bug> Bug { get; set; }
        public DbSet<Bug_Feature> Bug_Feature { get; set; }
        public DbSet<Bug_Status> Bug_Status { get; set; }
        public DbSet<Bug_Response> Bug_Response { get; set; }
        public DbSet<Corp_Info> Corp_Info { get; set; }
        public DbSet<User_Action_Log> User_Action_Log { get; set; }
        public DbSet<User_Action> User_Action { get; set; }
        public DbSet<Buy_Price_Detail> Buy_Price_Detail { get; set; }
        public DbSet<Buy_Price> Buy_Price { get; set; }
        public DbSet<Stock_Batch> Stock_Batch { get; set; }
        public DbSet<Leave_Stock_Detail> Leave_Stock_Detail { get; set; }
        public DbSet<Admin_Category> Admin_Category { get; set; }
    }
}
