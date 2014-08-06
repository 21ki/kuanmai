jQuery.extend({  
    GetDateByStr: function (str) {
        if ($.trim(str) == '') {
            return null;
        }
        var val = Date.parse(str);
        var newDate = new Date(val);
        return newDate;
    },
    GetUnixTime: function (str) {
        if ($.trim(str) == '') {
            return 0;
        }
        var val = Date.parse(str);
        var newDate = new Date(val);
        return newDate.getTime()/1000;
    },
    IsDecimal: function (str, digits) {
        if ($.trim(str) == "") {
            return false;
        }

        if (str.indexOf('.') < 0) {
            if (isNaN(str)) {
                return false;
            } else {
                return true;
            }
        }

        var strs = str.split('.');

        if (strs.length > 2) {
            return false;
        }

        if (isNaN(strs[0]) || isNaN(strs[1])) {
            return false;
        }

        if (digits && digits > 0) {
            if (strs[1].length > digits) {
                return false;
            }
        }

        return true;
    },
    IsNumber: function (str) {
        if ($.trim(str)=="") {
            return false;
        }

        if (isNaN(str)) {
            return false;
        }

        if (str.indexOf(".") > -1) {
            return false;
        }

        if (str.length>1) {
            if (str.substr(0, 1) == 0 || str.substr(0, 1) == "0") {
                return false;
            } else {
                return true;
            }
        }

        return true;
    }
});



//Compare two dates object
Date.prototype.GTE = function (date) {
    var date1 = this;
    var year1 = date1.getFullYear();
    var month1 = date1.getMonth() + 1;
    var day1 = date1.getDate();

    var year = date.getFullYear();
    var month = date.getMonth() + 1;
    var day = date.getDate();

    if (year1 < year) {
        return false;
    } else if (year1 == year) {
        if (month1 < month) {
            return false;
        } else if (month1 == month) {
            if (day1 < day) {
                return false;
            } else if (day1 == day) {
                if (date1.getHours() < date.getHours()) {
                    return false;
                } else if (date1.getHours() == date.getHours()) {
                    if (date1.getMinutes() < date.getMinutes()) {
                        return false;
                    } else if (date1.getMinutes() == date.getMinutes()) {
                        return false;
                    } else {
                        return true;
                    }
                } else {
                    return true;
                }
            } else {
                return true;
            }
        } else {
            return true;
        }
    } else {
        return true;
    }
}


function GetJsonData(data, rowIndex, dataIndx) {
    var rowData = data[rowIndex];
    var cellData = "";
    var obj;
    if (dataIndx.indexOf('.') > -1) {
        var indxs = dataIndx.split('.');
        for (var i = 0; i < indxs.length; i++) {
            if (i == 0) {
                obj = rowData[indxs[i]];
            } else {
                obj = obj[indxs[i]];
            }

            if (i == indxs.length - 1) {
                cellData = obj;
            }
        }
    } else {
        cellData = rowData[dataIndx];
    }
    return cellData;
}

function distanceOfTimeInWords(timestampInPage) {
    var timestampNow = Math.round(new Date().getTime() / 1000);
    var dif = timestampNow - timestampInPage;
    var outPut = "";
    
    //if (dif <= 15) outPut = "刚才";
    //else if (dif < 60) outPut = dif + "秒前";
    //else if (dif < 3600) outPut = Math.round(dif / 60) + "分钟前";
    //else if (dif < 86400) outPut = Math.round(dif / 3600) + "小时前";
    //else {
    //    var date = new Date(timestampInPage * 1000);
    //    var hours = date.getHours();
    //    var minutes = date.getMinutes();
    //    if (hours < 10) hours = "0" + hours;
    //    if (minutes < 10) minutes = "0" + minutes;

    //    outPut = date.getMonth() + 1 + "-" + (date.getDate()) + " " + hours + ":" + minutes;
    //}

    var date = new Date(timestampInPage * 1000);
    var hours = date.getHours();
    var minutes = date.getMinutes();
    if (hours < 10) hours = "0" + hours;
    if (minutes < 10) minutes = "0" + minutes;

    outPut = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + (date.getDate()) + " " + hours + ":" + minutes;
    return outPut;
}

function KMJXCBase() {
    this.AjaxCall = function (url, params, callback) {        
        
        $.post(
            url,
            params,
            function (response) {                
                if (callback && typeof (callback) == 'function') {
                    callback(response);
                }
            },
            'json'
        );
    }

    this.LeftMenuClick = function (obj, category, prefix) {       
        $('#' + category + prefix).show();       
        $(obj).parent().parent().find("div[id$='" + prefix + "']").each(function (index, item) {
            var id = $(item).attr('id');            
            if (id != (category + prefix)) {
                $(item).hide();
            }
        });
    }

    this.GetJsonCellData = function (rowData,dataIndx) { 
        var cellData = "";
        var obj;
        alert(dataIndx);
        if (dataIndx.indexOf('.') > -1) {
            var indxs = dataIndx.split('.');
            for (var i = 0; i < indxs.length; i++) {               
                if (i == 0) {
                    obj = rowData[indxs[i]];
                } else {
                    obj = obj[indxs[i]];
                }

                if (i == indxs.length - 1) {
                    cellData = obj;
                }
            }
        } else {
            cellData = rowData[dataIndx];
        }
        return cellData;
    }

    this.GetDateTime = function (timestampInPage,noTime) {
        if (timestampInPage==null || timestampInPage == 0 || timestampInPage == "") {
            return "";
        }
        var timestampNow = Math.round(new Date().getTime() / 1000);
        var dif = timestampNow - timestampInPage;
        var outPut = "";

        //if (dif <= 15) outPut = "刚才";
        //else if (dif < 60) outPut = dif + "秒前";
        //else if (dif < 3600) outPut = Math.round(dif / 60) + "分钟前";
        //else if (dif < 86400) outPut = Math.round(dif / 3600) + "小时前";
        //else {
        //    var date = new Date(timestampInPage * 1000);
        //    var hours = date.getHours();
        //    var minutes = date.getMinutes();
        //    if (hours < 10) hours = "0" + hours;
        //    if (minutes < 10) minutes = "0" + minutes;

        //    outPut = date.getMonth() + 1 + "-" + (date.getDate()) + " " + hours + ":" + minutes;
        //}

        var date = new Date(timestampInPage * 1000);
        var hours = date.getHours();
        var minutes = date.getMinutes();
        if (hours < 10) hours = "0" + hours;
        if (minutes < 10) minutes = "0" + minutes;

        if (noTime && noTime==true) {
            outPut = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + (date.getDate());
        }
        else {
            outPut = date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + (date.getDate()) + " " + hours + ":" + minutes;
        }
        
        return outPut;
    }

    this.Test = function () {
        return "TEST";
    }

    this.GetAreas = function (params, callback) {
        return this.AjaxCall("/api/Common/GetAreas", params, callback);
    }
    this.GetAllExpresses = function (params, callback) {
        return this.AjaxCall("/api/Common/GetExpresses", params, callback);
    }
    this.GetUsers = function (params, callback) {
        return this.AjaxCall("/api/Users/GetUsers", params, callback);
    }
    this.GetMallTypes = function (params, callback) {
        return this.AjaxCall("/api/Common/GetMallTypes", params, callback);
    }

    this.GetTradeStatusForSyncTrade = function () {
        var status = [
                      { 'value': 'WAIT_BUYER_CONFIRM_GOODS', 'name': '已发货', 'selected': false },
                      { 'value': 'SELLER_CONSIGNED_PART', 'name': '已发货（部分发货）', 'selected': false },
                      { 'value': 'TRADE_CLOSED', 'name': '交易关闭（全部退货）', 'selected': false },
                      { 'value': 'TRADE_FINISHED', 'name': '交易完成', 'selected': false }
                      
        ];

        return status;
    }

    this.InitializeHour = function (obj,hour) {
        for (var i = 0; i <= 23; i++) {
            var value = i;
            var name = i;
            if (i < 10) {
                name = "0" + i.toString();
            }
            if (i == hour) {
                $(obj).append("<option selected value=\"" + value + "\">" + name + "</option>");
            } else {
                $(obj).append("<option value=\""+value+"\">"+name+"</option>");
            }
        }
    }

    this.InitializeMinute = function (obj, minute) {
        for (var i = 0; i <= 59; i++) {
            var value = i;
            var name = i;
            if (i < 10) {
                name = "0" + i.toString();
            }
            if (i == minute) {
                $(obj).append("<option selected value=\"" + value + "\">" + name + "</option>");
            } else {
                $(obj).append("<option value=\"" + value + "\">" + name + "</option>");
            }
        }
    }

    this.Pager = function (json,selClass,pageClass) {
        if (!json || typeof(json)!=='object' || !json.total || !json.page || !json.pageSize || !json.fun) {
            return "";
        }

        var total = json.total;
        var page = json.page;
        var pageSize = json.pageSize;
        var totalPage = 0;
        var ajaxFun = json.fun;
        
        if (total < pageSize) {
            return "";
        }

        if (total % pageSize == 0) {
            totalPage = total / pageSize;
        } else {
            totalPage = total / pageSize + 1;
        }
        var buffer = [];
        
        for (var i = 1; i <= totalPage; i++) {

            if (i != page) {
                buffer.push("<a class=\"" + pageClass + "\" href=\"javascript:void(0)\" onclick=\"" + ajaxFun + "(" + i + ")\">" + i + "</a>");
            } else {
                buffer.push("<span class=\"" + selClass + "\">" + i + "</span>");
            }
        }

        return buffer.join("");
    }

    this.SetTheme = function (params, callback) {
        return this.AjaxCall("/Home/SetTheme", params, callback);
    }

    this.GetCorpInfo = function (params, callback) {
        return this.AjaxCall("/api/Common/GetCorpInfo", params, callback);
    }

    function getProducts(page,dialog) {
       
    }

    this.SearchProductsDialog = function (close, ok) {       
        var $div = $("<div></div>").appendTo($("body"));
        $div.dialog({
            width: 550,           
            resizable: false,
            title: "选择产品",
            modal: true,
            open: function () {
                $('#ulProductList').html("");
                getProducts(1, this);
            },
            close: function () { },
        });
        $div.dialog("open");
    }
}

KMJXCUserManager.prototype = new KMJXCBase();
function KMJXCUserManager() {
    
}

KMJXCProductManager.prototype = new KMJXCBase();
function KMJXCProductManager() {
    this.CreateCategory = function (postData, callback) {       
        var _this = this;
        _this.AjaxCall("/api/Categories/Add", postData, callback);
    }

    /*
    Parameters:
    parent_id
    */
    this.GetCategories = function (postData, callback) {
        var _this = this;
        _this.AjaxCall("/api/Categories/GetCategories/", postData, callback);
    }
    this.PullOnLineShopCategories = function (callback) {
        this.AjaxCall("/api/Categories/GetOnlineCategories/", "", callback);
    }
    this.GetProperties = function (params, callback) {
        this.AjaxCall("/api/Categories/GetProperties/", "", callback);
    }
    this.GetProperties2 = function (params, callback) {
        this.AjaxCall("/api/Categories/GetPropertiesT", params, callback);
    }
    this.CreateProperty = function (postData, callback) {
        this.AjaxCall("/api/Categories/CreateProperty/", postData, callback);
    }
    this.CategoryChange = function (obj, childContainer) {
        var _this = this;
        var pID = $(obj).val();
        
        if (pID > 0) {
            _this.GetCategories({ 'parent_id': pID }, function (response) {

                if (response != null && response != "" && typeof (response) == 'object') {

                    $('#' + childContainer).html("");
                    $('#' + childContainer).html("<option value=\"0\">--选择--</option>");
                    $(response.data).each(function (index, item) {
                        $('#' + childContainer).append("<option value=\"" + item.ID + "\">" + item.Name + "</option>");
                    });

                    $('#' + childContainer).show();
                } else {
                    $('#' + childContainer).hide();
                }
            });
        } else {
            $('#' + childContainer).html("");
            $('#' + childContainer).hide();
        }
    }

    this.AddNewPropValue = function (params, callback) {
        this.AjaxCall("/api/Categories/AddNewPropValue/", params, callback);
    }
    this.GetProperty = function (params, callback) {       
        this.AjaxCall("/api/Categories/GetProperty", params, callback);
    }
    this.DisableCategory = function (params, callback) {
        this.AjaxCall("/api/Categories/DisableCategory", params, callback);
    }

    this.GetPropertyValues = function (params, callback) {
        this.AjaxCall("/api/Categories/GetPropertyValues", params, callback);
    }
    this.DeleteImage = function (params, callback) {
        this.AjaxCall("/Image/Delete", params, callback);
    }
    this.CreateProduct = function (params, callback) {
        this.AjaxCall("/api/Products/Create", params, callback);
    }
    this.UpdateProduct = function (params, callback) {
        this.AjaxCall("/api/Products/UpdateProduct", params, callback);
    }

    /*
    Parameters:
    cid -- category
    keyword -- product keyword
    suppliers -- suppliers, multiple values contact with ","
    product_ids -- product id join with ","
    paging -- 0 or 1
    include_prop -- 0 or 1
    */
    this.SearchProducts = function (params, callback) {
        this.AjaxCall("/api/Products/SearchProducts", params, callback);
    }

    /*
    Parameters:
    mall_id:On sale product number
    product_id:local product id
    */
    this.GetProductFullInfo = function (params, callback) {
        this.AjaxCall("/api/Products/GetFullInfo", params, callback);
    }

    /*
    product_id:required
    */
    this.GetProductProperties = function (params, callback) {
        this.AjaxCall("/api/Products/GetProductProperties", params, callback);
    }

    /*
    category - category id:must
    products - product id join with "," : must
    */
    this.BatchEditCategory = function (params, callback) {
        this.AjaxCall("/api/Products/BatchEditCategory", params, callback);
    }

    /*
    price_details - required field json string
    desc - optional string
    title - required string
    */
    this.CreateBuyPrice = function (params, callback) {
       
        this.AjaxCall("/api/Products/CreateBuyPrice", params, callback);
    }
    /*
    category_id:required;
    props:required multiple property id join with ","
    */
    this.BatchUpdatePropertiesCategory = function (params, callback) {

        this.AjaxCall("/api/Categories/BatchUpdatePropertiesCategory", params, callback);
    }
}

KMJXCBuyManager.prototype = new KMJXCBase();
function KMJXCBuyManager() {
    this.CreateSupplier = function (params, callback) {
        this.AjaxCall("/api/Suppliers/Create", params, callback);
    }
    this.UpdateSupplier = function (params, callback) {
        this.AjaxCall("/api/Suppliers/Save", params, callback);
    }
    this.GetSuppliers = function (params, callback) {
        this.AjaxCall("/api/Suppliers/GetSuppliers", params, callback);
    }
    this.GetBuyOrders = function (params, callback) {
        this.AjaxCall("/api/Products/GetBuyOrders", params, callback);
    }
    this.CreateBuyOrder = function (params, callback) {
        this.AjaxCall("/api/Products/CreateBuyOrder", params, callback);
    }
    this.UpdateBuyOrder = function (params, callback) {
        this.AjaxCall("/api/Products/UpdateBuyOrder", params, callback);
    }
    this.VerifyOrder = function (params, callback) {
        this.AjaxCall("/api/Products/VerifyOrder", params, callback);
    }
    this.GetBuys = function (params, callback) {
        this.AjaxCall("/api/Products/GetBuys", params, callback);
    }
    this.GetBuyDetails = function (params, callback) {
        this.AjaxCall("/api/Products/GetBuyDetails", params, callback);
    }

    /*
    buy_price_id:required
    */
    this.GetBuyPriceFullInfo = function (params, callback) {
        this.AjaxCall("/api/Buy/GetBuyPriceFullInfo", params, callback);
    }

    /*
    price_details - required field json string
    desc - optional string
    title - required string
    */
    this.CreateBuyPrice = function (params, callback) {

        this.AjaxCall("/api/Buy/CreateBuyPrice", params, callback);
    }

    /*
      price_id:required
      price_details - required field json string
      desc - optional string
      title - required string
    */
    this.SaveBuyPrice = function (params, callback) {
        this.AjaxCall("/api/Buy/SaveBuyPrice", params, callback);
    }

    /*
    id:supplier id required
    products:multiple product id join with ","
    */
    this.UpdateSupplierProducts = function (params, callback) {
        this.AjaxCall("/api/Suppliers/UpdateSupplierProducts", params, callback);
    }

    /*
       id:supplier id required
       products:multiple product id join with ","
   */
    this.RemoveSupplierProducts = function (params, callback) {
        this.AjaxCall("/api/Suppliers/RemoveSupplierProducts", params, callback);
    }
}

KMJXCStockManager.prototype = new KMJXCBase();
function KMJXCStockManager() {
    this.EnterStockFromBuy = function (params, callback) {
        this.AjaxCall("/api/Stock/EnterStockFromBuy", params, callback);
    }
    this.GetStoreHouses = function (params, callback) {
        this.AjaxCall("/api/Stock/GetStoreHouses", params, callback);
    }
    this.CreateStoreHouse = function (params, callback) {
        this.AjaxCall("/api/Stock/CreateStoreHouse", params, callback);
    }
    this.UpdateStoreHouse = function (params, callback) {
        this.AjaxCall("/api/Stock/UpdateStoreHouse", params, callback);
    }
    this.SearchEnterStock = function (params, callback) {
        this.AjaxCall("/api/Stock/SearchEnterStock", params, callback);
    }
    this.UpdateStockByEnter = function (params, callback) {
        this.AjaxCall("/api/Stock/UpdateEnterStockToProductStock", params, callback);
    }
    this.SearchProductWastage = function (params, callback) {
        this.AjaxCall("/api/Stock/SearchProductWastage", params, callback);
    }
    this.GetProductStockDetails = function (params, callback) {
        this.AjaxCall("/api/Stock/GetProductStockDetails", params, callback);
    }

    /*
    products -- parent id, support multiple ids, join with ","
    house -- store house id don't support multiple ids
    page -- des page
    pageSize
    paging -- 1/0
    */
    this.SearchStocks = function (params, callback) {
        this.AjaxCall("/api/Stock/SearchStocks", params, callback);
    }

    /*
    stocks -- json str [{\"product_id\":"xxx",\"quantity\":"xxx",\"store_house\":"xxx"}]
    */
    this.UpdateProductsStocks = function (params, callback) {
        this.AjaxCall("/api/Stock/UpdateProductsStocks", params, callback);
    }

    /*
    trade_id:required;
    order_id:required;
    mall_item_id:required:
    mall_sku_id:optional
    product:required;
    product_prop:optional, if mall_sku_id is given, it will be required,
    connect:optional 1/0 1 means true 0 means false
    */
    this.CreateLeaveStockForMallTrade = function (params, callback) {
        this.AjaxCall("/api/Stock/CreateLeaveStockForMallTrade", params, callback);
    }

    this.GetStockAnalysis = function (params, callback) {
        this.AjaxCall("/api/Stock/GetStockAnalysis", params, callback);
    }

    /*
    product_ids:required
    */
    this.GetProductsWastageDetail = function (params, callback) {
        this.AjaxCall("/api/Stock/GetProductsWastageDetail", params, callback);
    }

    /*
    wastages:required, data format is json [{"product_id":1000,"quantity":100}] which must be url encoded
    */
    this.UpdateProductsWastage = function (params, callback) {
        this.AjaxCall("/api/Stock/UpdateProductsWastage", params, callback);
    }
}

KMJXCSaleManager.prototype = new KMJXCBase();
function KMJXCSaleManager() {
    this.SyncMallTrade = function (params, callback) {
        this.AjaxCall("/api/Sale/SyncMallTrades", params, callback);
    }
    this.GetLastSyncTime = function (params, callback) {
        this.AjaxCall("/api/Sale/GetLastSyncTime", params, callback);
    }

    this.SyncMallTrades = function (params, callback) {
        this.AjaxCall("/api/Sale/SyncMallTrades", params, callback);
    }
    this.HandleBackSaleDetail = function (params, callback) {
        this.AjaxCall("/api/Sale/HandleBackSaleDetail", params, callback);
    }

    this.SearchTradeSyncLog = function (params, callback) {
        this.AjaxCall("/api/Sale/SearchTradeSyncLog", params, callback);
    }
}

KMJXCShopManager.prototype = new KMJXCBase();
function KMJXCShopManager() {
    this.SearchExpresses = function (params, callback) {
        this.AjaxCall("/api/Shop/SearchExpresses", params, callback);
    }
    this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Shop/SearchExpressFee", params, callback);
    }
    this.GetNonAddedExpresses = function (params, callback) {
        this.AjaxCall("/api/Shop/GetNonAddedExpresses", params, callback);
    }
    this.CreateShopExpress = function (params, callback) {
        this.AjaxCall("/api/Shop/CreateShopExpress", params, callback);
    }
    this.UpdateExpressFee = function (params, callback) {
        this.AjaxCall("/api/Shop/UpdateExpressFee", params, callback);
    }
    this.CreateExpressFees = function (params, callback) {
        this.AjaxCall("/api/Shop/CreateExpressFees", params, callback);
    }
    this.GetNonExpressedHouses = function (params, callback) {
        this.AjaxCall("/api/Shop/GetNonExpressedHouses", params, callback);
    }
    this.GetStoreHouses = function (params, callback) {
        this.AjaxCall("/api/Shop/GetStoreHouses", params, callback);
    }
    this.SetDefaultExpress = function (params, callback) {
        this.AjaxCall("/api/Shop/SetDefaultExpress", params, callback);
    }
    this.AddChildShop = function (params, callback) {
        this.AjaxCall("/api/Shop/AddChildShop", params, callback);
    }
    this.HandleAddChildRequest = function (params, callback) {
        this.AjaxCall("/api/Shop/HandleAddChildRequest", params, callback);
    }
    this.SearchShopUsers = function (params, callback) {
        this.AjaxCall("/api/Shop/SearchShopUsers", params, callback);
    }
    this.SearchShopUsers = function (params, callback) {
        this.AjaxCall("/api/Shop/SearchShopUsers", params, callback);
    }
    this.SyncMallSoldProducts = function (params, callback) {
        this.AjaxCall("/api/Shop/SyncMallSoldProducts", params, callback);
    }
    this.SearchOnSaleProducts = function (params, callback) {
        this.AjaxCall("/api/Shop/SearchOnSaleProducts", params, callback);
    }

    /*
    Parameters:
    mall_id:on sale product id
    product_id:local product id
    */
    this.MapMallProduct = function (params, callback) {
        this.AjaxCall("/api/Shop/MapMallProduct", params, callback);
    }

    /*
    Parameters:
    sku_id:on sale product sku id
    product_id:local sub product id
    */
    this.MapMallProductSku = function (params, callback) {
        this.AjaxCall("/api/Shop/MapMallProductSku", params, callback);
    }

    /*
    mall_products:the mall product ids join with ","
    map_product:1 or 0, 1 means map, 0 means not map
    */
    this.CreateProductsByMallProducts = function (params, callback) {
        this.AjaxCall("/api/Shop/CreateProductsByMallProducts", params, callback);
    }

    /*
    phone:optional,
    address:optional,
    contact:optional,
    email:optional,
    shop_id:optional, if it is 0, then use current user's shop
    */
    this.UpdateShopContactInfo = function (params, callback) {
        this.AjaxCall("/api/Shop/UpdateShopContactInfo", params, callback);
    }
}

KMJXCPermissionManager.prototype = new KMJXCBase();
function KMJXCPermissionManager() {
    this.GetAdminRoles = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/GetAdminRoles", params, callback);
    }
    this.GetAdminActions = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/GetAdminActions", params, callback);
    }

    /*
    user: user id required
    roles: multiple role id join with ","
    */
    this.UpdateUserRoles = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/UpdateUserRoles", params, callback);
    }
    this.UpdateRoleActions = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/UpdateRoleActions", params, callback);
    }
    this.CreateRole = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/CreateRole", params, callback);
    }
    this.SetAdminRoleStatus = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/SetAdminRoleStatus", params, callback);
    }
    this.GetUserAdminRoles = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/GetUserAdminRoles", params, callback);
    }

    /*
    role_i:required
    */
    this.GetRoleActions = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Permission/GetRoleActions", params, callback);
    }
}

KMJXCReportManager.prototype = new KMJXCBase();
function KMJXCReportManager() {
    /*
       paging:optional,default is 1
       products:optional
       page:required
       pageSize:required
   */
    this.GetSalesReport = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Report/GetSalesReport", params, callback);
    }



    this.GetExcelSaleReport = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Report/GetExcelSaleReport", params, callback);
    }

    /*
    paging:optional,default is 1
    products:optional
    page:required
    pageSize:required
    */
    this.GetStockReport = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Report/GetStockReport", params, callback);
    }

    /*
      paging:optional,default is 1
      products:optional
      page:required
      pageSize:required
   */
    this.GetExcelStockReport = this.SearchExpressFee = function (params, callback) {
        this.AjaxCall("/api/Report/GetExcelStockReport", params, callback);
    }
}

KMJXCBugManager.prototype = new KMJXCBase();

function KMJXCBugManager() {
    /*
    feature
    title
    description
    */
    this.CreateBug = function (params, callback) {
        this.AjaxCall("/api/Bug/CreateBug", params, callback);
    }

    /*
   bug_id   
   description
   */
    this.CreateBugResponse = function (params, callback) {
        this.AjaxCall("/api/Bug/CreateBugResponse", params, callback);
    }

    /*
      page   
      pageSize
      status
      user
      feature
    */
    this.SearchBugs = function (params, callback) {
        this.AjaxCall("/api/Bug/SearchBugs", params, callback);
    }

    /*
     bug_id
    */
    this.GetBugFullInfo = function (params, callback) {
        this.AjaxCall("/api/Bug/GetBugFullInfo", params, callback);
    }
    this.GetBugStatuses = function (params, callback) {
        this.AjaxCall("/api/Bug/GetBugStatuses", params, callback);
    }
    this.GetBugFeatures = function (params, callback) {
        this.AjaxCall("/api/Bug/GetBugFeatures", params, callback);
    }
    /*
    bug_id: required
    status:required
    */
    this.UpdateStatus = function (params, callback) {
        this.AjaxCall("/api/Bug/UpdateStatus", params, callback);
    }
}

KMJXCSystemAdmin.prototype = new KMJXCBase();
function KMJXCSystemAdmin() {
    /*
    about,
    contact,
    help
    */
    this.SetCorpInfo = function (params, callback) {
        this.AjaxCall("/api/SystemAdmin/SetCorpInfo", params, callback);
    }

    /*
    password
    uid -- optional
    */
    this.VerifyPassword = function (params, callback) {
        this.AjaxCall("/api/SystemAdmin/VerifyPassword", params, callback);
    }

    /*
    password
    uid -- optional
    */
    this.UpdatePassword = function (params, callback) {
        this.AjaxCall("/api/SystemAdmin/UpdatePassword", params, callback);
    }
}

KMJXManager.prototype = new KMJXCBase();
function KMJXManager() {
    this.UserManager = new KMJXCUserManager();
    this.StockManager = new KMJXCStockManager();
}

var manager = new KMJXManager();

function ProductGrid(tableId) {
    
}

function MessageBox(message, time) {
    //check if the dom already contains the message box dialog
    box = Boxy.get($('#alertmsgbox'));
    if (box != undefined) {
        return;
    }

    new Boxy("<div id=\"alertmsgbox\" style=\"padding-left:20px;padding-right:20px;text-align:center\">" + message + "</div>",
              {
                  modal: false,
                  fixed: true,
                  cache: true,
                  draggable: true,
                  center: true,
                  unloadOnHide: true,
                  // x:50,  
                  //y:50,        
                  afterDrop: function () { },
                  afterShow: function () {
                      var a = setTimeout(function () {
                          var box = Boxy.get($('#alertmsgbox'));
                          box.hide();
                      }, time);
                  },
                  afterHide: function () { }
              });
}

function ShowProgress(keyId, callback, text) {
    if (!text || $.trim(text) == '') {
        text = "正在加载,请耐心等待..........";
    }
    var loading = "<div style=\"padding-left:50px;padding-right:50px;\" id=\"" + keyId + "\"><span style=\"height:20px;line-height:20px;vertical-align: middle;\"><img style=\"vertical-align: middle;\" src=\"/Content/images/loading.gif\"/> " + text + "</span> </div>";
    var call = function (res) { };
    if (callback) {
        call = callback;
    }
    return ShowBox({ "content": loading, "callback": call });

}

function ShowDialog(dlgKey, model, options, text) {
    var content = '';
    if (!text || $.trim(text) == '') {
        text = "正在加载,请耐心等待..........";
    }
    if (model == "ajax") {
        if (options.url) {
            var loading = "<div style=\"width:200px;\" id=\"" + dlgKey + "\"><span style=\"height:20px;line-height:20px;vertical-align: middle;\"><img style=\"vertical-align: middle;\" src=\"\\Content\\images\\loading.gif\"/> " + text + "</span> </div>";
            box = ShowBox({
                "content": loading, "title": options.title, "x": options.x, "y": options.y, "callback":
                function () {
                    $.post(
                            options.url,
                            { key: dlgKey },
                            function (data) {
                                if (is_json(data)) {
                                    Boxy.get($('#' + dlgKey)).hide();
                                    var json = eval("(" + data + ")");
                                    if (json.status == "failed") {
                                        if (json.login == 'no') {
                                            modal_login();
                                            return;
                                        }
                                    }
                                } else {
                                    if (data != '') {
                                        var box = Boxy.get($('#' + dlgKey));
                                        box.setContent(data);
                                        box.center();
                                    }

                                    if (options.callback) {
                                        options.callback();
                                    }
                                }
                            }
                            );
                }
            });

        }
    } else if (model == "html") {
        content = options.html;
        callback = function () { };
        if (options.callback) {
            callback = options.callback;
        }

        ShowBox({ "content": content, "title": options.title, "x": options.x, "y": options.y, "callback": callback() });
    }
}

function ShowBox(options) {

    var box;
    if (options && (options.x != '' || options.y != '')) {

        box = new Boxy(options.content, {
            title: options.title,
            closeText: "关闭",
            closeable: true,
            modal: true,
            fixed: true,
            //cache:true,    
            x: options.x,
            y: options.y,
            draggable: true,
            center: true,
            unloadOnHide: true,
            afterDrop: function () { },
            afterShow: function () {
                if (options.callback) {
                    options.callback();
                }
            },
            afterHide: function () { },
            behaviours: function () { }
        });
    } else {
        box = new Boxy(options.content, {
            title: options.title,
            closeText: "关闭",
            closeable: true,
            modal: true,
            fixed: true,
            cache: true,
            draggable: true,
            center: true,
            unloadOnHide: true,
            afterDrop: function () { },
            afterShow: function () {
                if (options.callback) {
                    options.callback();
                }
            },
            afterHide: function () { },
            behaviours: function () { }
        });
    }

    return box;
}

function VerifyExistingProps(props1, props) {
    var tmp = [];
    var tmp1 = [];
    var found = false;
    if (props1.length == 0 || props.length == 0) {
        return found;
    }
    for (var i = 0; i < props1.length; i++) {
        var prop1 = props1[i];
        var startIndex = i + 1;
        for (startIndex; startIndex < props1.length;) {
            tmp1.push(props1[startIndex]);
            startIndex++;
        }
        if (prop1 != null) {
            for (var j = 0; j < props.length; j++) {
                var prop2 = props[j];
                if (prop2 != null) {
                    if (prop1.pid == prop2.pid && prop1.vid == prop2.vid) {
                        found = true;
                    } else {
                        tmp.push(prop2);
                    }
                }
            }
        }

        if (found) {
            if (i < props1.length - 1) {
                return VerifyExistingProps(tmp1, tmp);
            }
        } else {
            break;
        }
    }

    return found;
}