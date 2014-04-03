function KMJXCBase() {
    this.AjaxCall = function (url,params,callback) {
        $.post(
            url,
            params,
            function (response) {
                if (callback && typeof (callback) == 'function') {
                    callback(response);
                }
            }
        );
    }

    this.LeftMenuClick = function (obj, category, prefix) {
        $('$' + category + prefix).show();
        $(obj).parent().find("div[id$='" + prefix + "']").each(function (index, item) {
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