use kuanmai;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Access_Token` (
  `User_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Mall_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Access_Token` varchar(45) NOT NULL DEFAULT '',
  `Expirse_In` int(10) unsigned NOT NULL DEFAULT '0',
  `Refresh_Token` varchar(45) NOT NULL DEFAULT '',
  `RExpirse_In` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`User_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Back_Sale` (
  `Back_Sale_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Back_Date` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `StoreHouse_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Description` varchar(500) NOT NULL DEFAULT '',
  `Sale_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Back_Sale_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Back_Sale_Detail` (
  `Back_Sale_ID` int(10) unsigned NOT NULL,
  `Product_ID` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`Back_Sale_ID`,`Product_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Back_Stock` (
  `Back_Sock_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Back_Date` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `StoreHouse_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Description` varchar(200) NOT NULL DEFAULT '',
  `Back_Sale_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Back_Sock_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Back_Stock_Detail` (
  `Back_Stock_ID` int(10) unsigned NOT NULL,
  `Product_ID` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL DEFAULT '0',
  `Price` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`Back_Stock_ID`,`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Buy` (
  `Buy_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Come_Date` int(10) unsigned NOT NULL,
  `Shop_ID` int(10) unsigned NOT NULL,
  `User_ID` int(10) unsigned NOT NULL,
  `Create_Date` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Buy_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Buy_Detail` (
  `Buy_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Product_ID` int(10) unsigned NOT NULL,
  `Buy_Order_ID` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  `Create_Date` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Buy_ID`,`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Buy_Order` (
  `Buy_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Supplier_ID` int(10) unsigned NOT NULL,
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Write_Date` int(10) unsigned DEFAULT '0',
  `Insure_Date` int(10) unsigned DEFAULT '0',
  `End_Date` int(10) unsigned DEFAULT '0',
  `Create_Date` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Order_User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Buy_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Buy_Order_Detail` (
  `Buy_Order_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Product_ID` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  PRIMARY KEY (`Buy_Order_ID`,`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Common_District` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  `Level` tinyint(4) unsigned NOT NULL,
  `UPID` int(10) unsigned NOT NULL,
  `Order` tinyint(4) unsigned NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Customer` (
  `Customer_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Mall_Type_ID` int(10) unsigned NOT NULL,
  `Name` varchar(45) NOT NULL DEFAULT '',
  `Email` varchar(45) DEFAULT '',
  `Phone` varchar(45) DEFAULT '',
  `Address` varchar(200) DEFAULT '',
  `City_ID` int(10) unsigned DEFAULT '0',
  `Province_ID` int(10) unsigned DEFAULT '0',
  PRIMARY KEY (`Customer_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Customer_Shop` (
  `Customer_ID` int(10) unsigned NOT NULL,
  `Shop_ID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Customer_ID`,`Shop_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Employee` (
  `Employee_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Department` varchar(45) DEFAULT '',
  `Name` varchar(45) NOT NULL,
  `Duty` varchar(45) DEFAULT NULL,
  `Gendar` char(1) DEFAULT '0',
  `BirthDate` int(10) unsigned DEFAULT '0',
  `HireDate` int(10) unsigned DEFAULT '0',
  `MatureDate` int(10) unsigned DEFAULT '0',
  `IdentityCard` varchar(18) DEFAULT '',
  `Address` varchar(45) DEFAULT '',
  `Phone` varchar(15) DEFAULT '',
  `Email` varchar(45) DEFAULT '',
  `User_ID` int(10) unsigned DEFAULT NULL,
  PRIMARY KEY (`Employee_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Enter_Stock` (
  `Enter_Stock_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Enter_Date` int(10) unsigned NOT NULL,
  `Shop_ID` int(10) unsigned NOT NULL,
  `StoreHouse_ID` int(10) unsigned NOT NULL,
  `User_ID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Enter_Stock_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Enter_Stock_Detail` (
  `Enter_Stock_ID` int(10) unsigned NOT NULL,
  `Product_ID` int(10) unsigned NOT NULL,
  `Quantity` int(10) unsigned NOT NULL,
  `Price` decimal(10,2) NOT NULL,
  `Have_Invoice` tinyint(1) unsigned DEFAULT '0',
  `Invoice_Num` varchar(45) DEFAULT '',
  `Invoice_Amount` decimal(10,2) DEFAULT '0.00',
  PRIMARY KEY (`Enter_Stock_ID`,`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Leave_Stock` (
  `Leave_Stock_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Leave_Date` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL,
  `StoreHouse_ID` int(10) unsigned NOT NULL,
  `To_StoreHouse_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Sale_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Leave_Stock_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Mall_Type` (
  `Mall_Type_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL,
  `Create_Time` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Mall_Type_ID`) USING BTREE
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Product` (
  `Product_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Product_Class_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Name` varchar(100) NOT NULL DEFAULT '',
  `Price` decimal(10,0) NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Create_Time` int(10) unsigned NOT NULL DEFAULT '0',
  `Product_Unit_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Parent_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Product_Class` (
  `Product_Class_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) NOT NULL DEFAULT '',
  `Parent_ID` int(10) unsigned DEFAULT '0',
  `Order` tinyint(2) unsigned DEFAULT '0',
  `Create_Time` int(10) unsigned DEFAULT '0',
  `Create_User_ID` int(10) unsigned DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Product_Class_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Product_Supplier` (
  `Product_ID` int(10) unsigned NOT NULL,
  `Supplier_ID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Product_ID`,`Supplier_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Product_Unit` (
  `Product_Unit_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(45) NOT NULL DEFAULT '',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Description` varchar(100) DEFAULT '',
  `Shop_ID` int(10) unsigned DEFAULT '0',
  `Create_Time` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Product_Unit_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Sale` (
  `Sale_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Sale_Time` int(10) unsigned NOT NULL DEFAULT '0',
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned DEFAULT '0',
  `Mall_Trade_ID` varchar(30) DEFAULT '',
  `Amount` decimal(10,2) NOT NULL,
  `Express_Cop` varchar(20) NOT NULL DEFAULT '',
  `Province_ID` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Sale_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Sale_Detail` (
  `Sale_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Product_ID` int(10) unsigned NOT NULL,
  `Mall_Order_ID` varchar(30) DEFAULT '',
  `Quantity` int(10) unsigned NOT NULL DEFAULT '0',
  `Price` decimal(10,2) NOT NULL,
  `Discount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `Status` tinyint(1) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`Sale_ID`,`Product_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Shop` (
  `Shop_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL DEFAULT '',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Main_Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Description` varchar(500) NOT NULL DEFAULT '',
  `Mall_Type_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Mall_Shop_ID` varchar(50) NOT NULL,
  PRIMARY KEY (`Shop_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Shop_User` (
  `Shop_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Create_Time` int(10) unsigned DEFAULT NULL,
  `Description` varchar(200) DEFAULT '',
  PRIMARY KEY (`Shop_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Stock_Pile` (
  `StockPile_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Shop_ID` int(10) unsigned NOT NULL,
  `StockHouse_ID` int(10) unsigned NOT NULL,
  `Product_ID` int(10) unsigned NOT NULL,
  `First_Enter_Time` int(10) unsigned DEFAULT '0',
  `LastLeave_Time` int(10) unsigned DEFAULT '0',
  `Quantity` int(10) unsigned NOT NULL DEFAULT '0',
  `Price` decimal(10,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`StockPile_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Store_House` (
  `StoreHouse_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Address` varchar(200) DEFAULT '',
  `Phone` varchar(45) DEFAULT '',
  `Employee_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Create_Time` int(10) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`StoreHouse_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Supplier` (
  `Supplier_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Address` varchar(200) DEFAULT '',
  `Fax` varchar(20) DEFAULT '',
  `Phone` varchar(20) DEFAULT '',
  `PostalCode` varchar(10) DEFAULT '',
  `Province_ID` int(10) unsigned DEFAULT '0',
  `City_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Create_Time` int(10) unsigned NOT NULL DEFAULT '0',
  `User_ID` int(10) unsigned NOT NULL DEFAULT '0',
  `Contact_Person` varchar(20) DEFAULT '',
  PRIMARY KEY (`Supplier_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Supplier_Shop` (
  `Supplier_ID` int(10) unsigned NOT NULL,
  `Shop_ID` int(10) unsigned NOT NULL,
  PRIMARY KEY (`Supplier_ID`,`Shop_ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `User` (
  `User_ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Mall_ID` varchar(45) NOT NULL DEFAULT '',
  `Mall_Name` varchar(45) NOT NULL DEFAULT '',
  `Mall_Type` int(10) unsigned NOT NULL DEFAULT '0',
  `Name` varchar(45) NOT NULL DEFAULT '',
  `Password` varchar(45) NOT NULL DEFAULT '',
  PRIMARY KEY (`User_ID`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;
