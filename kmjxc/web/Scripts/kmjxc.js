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
        }
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