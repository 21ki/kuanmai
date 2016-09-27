var mLocatorApi = "http://v.showji.com/Locating/showji.com2016234999234.aspx";
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

function ReportGenerater() {
    this.render = function (data, grid, pie, isAdmin) {       
        $('#' + grid).html("");
        $('#'+pie).html("");
        var pieData = [];
        var table = $('<table class="table table-striped grid-table"></table>').appendTo($('#' + grid));

        if (!isAdmin) {
            $('<thead><tr><th class="grid-header" style="width:10%">套餐编号</th><th class="grid-header" style="width:20%">套餐名称</th><th class="grid-header" style="width:15%">销售额</th><th class="grid-header" style="width:10%">成本</th><th class="grid-header" style="width:10%">利润</th></tr></thead>').appendTo(table);
        } else {
            $('<thead><tr><th class="grid-header" style="width:20%">名称</th><th class="grid-header" style="width:15%">销售额</th><th class="grid-header" style="width:10%">成本</th><th class="grid-header" style="width:10%">利润</th></tr></thead>').appendTo(table);
        }
        var body = $('<tbody></tbody>').appendTo(table);
        var sTotal = 0;
        var cTotal = 0;
        var rTotal = 0;
        $(data).each(function (index, item) {
            var row = $('<tr class="grid-row"></tr>').appendTo(body);
            if (!isAdmin) {
                $('<td class="grid-cell"></td>').html(item.Id).appendTo(row);
            }
            $('<td class="grid-cell"></td>').html(item.Name).appendTo(row);
            $('<td class="grid-cell"></td>').html(parseFloat(item.SalesAmount).toFixed(2)).appendTo(row);
            $('<td class="grid-cell"></td>').html(parseFloat(item.CostAmount).toFixed(2)).appendTo(row);
            $('<td class="grid-cell"></td>').html(parseFloat(item.Revenue).toFixed(2)).appendTo(row);
            sTotal += parseFloat(item.SalesAmount);
            cTotal += parseFloat(item.CostAmount);
            rTotal += parseFloat(item.Revenue);
            pieData.push([item.Name, item.CostAmount]);
        });
        var trow = $('<tr class="grid-row"></tr>').appendTo(body);
        $('<td class="grid-cell"></td>').html("汇总").appendTo(trow);
        if (!isAdmin) {
            $('<td class="grid-cell"></td>').html("").appendTo(trow);
        }
        $('<td class="grid-cell"></td>').html(sTotal.toFixed(2)).appendTo(trow);
        $('<td class="grid-cell"></td>').html(cTotal.toFixed(2)).appendTo(trow);
        $('<td class="grid-cell"></td>').html(rTotal.toFixed(2)).appendTo(trow);

        var plot1 = jQuery.jqplot(pie, [pieData],
          {
              seriesDefaults: {
                  // Make this a pie chart.
                  renderer: jQuery.jqplot.PieRenderer,
                  rendererOptions: {
                      // Put data labels on the pie slices.
                      // By default, labels show the percentage of the slice.
                      showDataLabels: true
                  }
              },
              legend: { show: true, location: 'e' }
          }
        );
    }
}

function openModalDialog(url, data) {   
    $.get(
            url,
            data,
            function (res, status) {
                if (res != null && res != "undefined") {
                    var modal = $('<div class="modal fade" id="editRuoteModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel"></div>').appendTo($('body'));
                    var modalDiv = $('<div class="modal-dialog" role="document"></div>').appendTo($(modal));
                    var modalContent = $('<div class="modal-content"></div>').appendTo($(modalDiv));
                    var modalBody = $('<div class="modal-body"></div>').appendTo($(modalContent)).html(res);
                    $(modal).modal();
                }
            }
     );
}


