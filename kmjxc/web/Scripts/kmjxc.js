function distanceOfTimeInWords(timestampInPage) {
    var timestampNow = Math.round(new Date().getTime() / 1000);
    var dif = timestampNow - timestampInPage;
    var outPut = "";

    if (dif <= 15) outPut = "刚才";
    else if (dif < 60) outPut = dif + "秒前";
    else if (dif < 3600) outPut = Math.round(dif / 60) + "分钟前";
    else if (dif < 86400) outPut = Math.round(dif / 3600) + "小时前";
    else {
        var date = new Date(timestampInPage * 1000);
        var hours = date.getHours();
        var minutes = date.getMinutes();
        if (hours < 10) hours = "0" + hours;
        if (minutes < 10) minutes = "0" + minutes;

        outPut = date.getMonth() + 1 + "-" + (date.getDate() + 1) + " " + hours + ":" + minutes;
    }
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
                    $(response).each(function (index, item) {
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
        this.AjaxCall("/api/Categories/AddNewPropValue/", postData, callback);
    }
    this.GetProperty = function (params, callback) {
       
        this.AjaxCall("/api/Categories/GetProperty", params, callback);
    }
}

KMJXCBuyManager.prototype = new KMJXCBase();
function KMJXCBuyManager() {

}

KMJXCStockManager.prototype = new KMJXCBase();
function KMJXCStockManager() {

}

KMJXManager.prototype = new KMJXCBase();
function KMJXManager() {
    this.UserManager = new KMJXCUserManager();
    this.StockManager = new KMJXCStockManager();
}

var manager = new KMJXManager();

function PropertyGrid(tableId) {
    
}