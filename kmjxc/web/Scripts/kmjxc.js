jQuery.extend({  
    GetDateByStr: function (str) {
        if ($.trim(str) == '') {
            return null;
        }
        var val = Date.parse(str);
        var newDate = new Date(val);
        return newDate;
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
    this.GetUsers = function (params, callback) {
        return this.AjaxCall("/api/Users/GetUsers", params, callback);
    }
    this.GetTradeStatusForSyncTrade = function () {
        var status = [{ 'value': 'TRADE_NO_CREATE_PAY', 'name': '没有创建支付宝交易','selected': false },
                      { 'value': 'WAIT_BUYER_PAY', 'name': '等待买家付款', 'selected': false },
                      { 'value': 'WAIT_SELLER_SEND_GOODS', 'name': '等待卖家发货','selected':true },
                      { 'value': 'WAIT_BUYER_CONFIRM_GOODS', 'name': '等待买家确认收货','selected': false },
                      { 'value': 'TRADE_FINISHED', 'name': '交易成功', 'selected': false },
                      { 'value': 'TRADE_CLOSED', 'name': '交易关闭', 'selected': false }
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
    this.SearchProducts = function (params, callback) {
        this.AjaxCall("/api/Products/SearchProducts", params, callback);
    }
    this.GetProductFullInfo = function (params, callback) {
        this.AjaxCall("/api/Products/GetFullInfo", params, callback);
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
}

KMJXManager.prototype = new KMJXCBase();
function KMJXManager() {
    this.UserManager = new KMJXCUserManager();
    this.StockManager = new KMJXCStockManager();
}

var manager = new KMJXManager();

function ProductGrid(tableId) {
    
}


function MessageBox(message, time, width) {
    //check if the dom already contains the message box dialog
    box = Boxy.get($('#alertmsgbox'));
    if (box != undefined) {
        return;
    }

    var w = 200;
    if (width) {
        w = width;
    }
    new Boxy("<div id=\"alertmsgbox\" style=\"width:" + w + "px;text-align:center\">" + message + "</div>",
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
    var loading = "<div style=\"padding-left:50px;padding-right:50px;\" id=\"" + keyId + "\"><span style=\"height:20px;line-height:20px;vertical-align: middle;\"><img style=\"vertical-align: middle;\" src=\"\\Content\\images\\loading.gif\"/> " + text + "</span> </div>";
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