var mLocatorApi = "http://v.showji.com/Locating/showji.com20150416273007.aspx";
function GetMobileLocationBack(obj) {    
    if (GetMobileLocationBack.CallBack && typeof (GetMobileLocationBack.CallBack) == "function") {
        var location = null;
        if (obj && obj != null && obj != 'undefined' && obj["QueryResult"]=="True") {
            location = { 'mobile': obj["Mobile"], 'sp': obj["TO"], 'province': obj["Province"], 'city': obj["City"] };
        }       
        GetMobileLocationBack.CallBack(location);
    }
}
function GetMobileLocation(mobileNumber, callback) {
    var oHead = document.getElementsByTagName('head')[0];
    var oTar = document.getElementById("remotejs");
    if (oTar) oHead.removeChild(oTar);
    var oScript = document.createElement('script');
    GetMobileLocationBack.CallBack = callback;
    oScript.type = "text/javascript";
    oScript.id = "remotejs";
    oScript.src = mLocatorApi + "?m=" + escape(mobileNumber) + "&output=json&callback=GetMobileLocationBack&timestamp=" + new Date().getTime();
    oHead.appendChild(oScript);
}


