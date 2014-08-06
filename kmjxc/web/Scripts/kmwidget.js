(function ($) {


    /*
    pop up a search products dialog
    */
    $.fn.SearchProductsDialog = function (options) {
        var pdtMgr = new KMJXCProductManager();
        var _selectedProducts = [];
        function getProducts(page, dialog) {
            if (dialog.checkAll) {
                dialog.checkAll.removeAttr("checked");
                dialog.checkAll.next("span").html("全选");
            }
            if (page <= 0) {
                page = 1;
            }
            if (!dialog || dialog==null || typeof dialog == 'undefined') {
                return;
            }
            dialog.products_list.hide();
            dialog.pager.html("").hide();           
            var productListPageSize = dialog.options.pageSize;
            var pid = dialog.parent_category.val();
            var cid = dialog.child_category.val();
            var pkey = dialog.keyword.val();
            var category = 0;
            category = pid;
            if (cid > 0) {
                category = cid;
            }
            dialog.loading.show();
            var $ul = dialog.products_list;
            $ul.html("");           
            pdtMgr.SearchProducts({ 'cid': category, 'keyword': pkey, 'pageSize': productListPageSize, 'page': page }, function (res) {
                dialog.loading.hide();
                if (res != null) {
                    if ($(res.data).size() > 0 && dialog.options.seltype == "checkbox") {
                        dialog.checkAll.removeAttr("disabled");
                    } else {
                        dialog.checkAll.attr("disabled",true);
                    }
                    var checkedCount = 0;
                    $(res.data).each(function (index, item) {
                        var $li = $("<li></li>").appendTo($ul);
                        var $radio = null;
                        if (dialog.options.seltype == "radio") {
                            $radio = $("<input value=\"" + item.ID + "\" type=\"radio\" name=\"opt_productsel\"/>").appendTo($li);
                        } else if (dialog.options.seltype == "checkbox") {
                            $radio = $("<input value=\"" + item.ID + "\" type=\"checkbox\"/>").appendTo($li);
                            var alSelected = false;
                            if (_selectedProducts.length > 0) {
                                $(_selectedProducts).each(function (j, sel) {
                                    if (sel.product_id == item.ID) {
                                        alSelected = true;
                                        return false;
                                    }
                                });
                            }
                            if (alSelected) {
                                checkedCount++;
                                $radio.attr("checked", true);
                            }
                            $radio.change(function (e) {
                                var tmp = [];
                                var checked = $(this).attr('checked');
                                var curId = $(this).attr('value');
                                var name = $(this).next("span").html();
                                var newJson = { product_id: curId, product_name: name }; 
                                var existed = false;
                                for (var i = 0; i < _selectedProducts.length; i++) {
                                    if (_selectedProducts[i].product_id == curId) {
                                        existed = true;
                                        if (checked == 'checked') {
                                            tmp.push(_selectedProducts[i]);
                                        }
                                    } else {
                                        tmp.push(_selectedProducts[i]);
                                    }
                                }
                                if (!existed) {
                                    if (checked == 'checked') {
                                        tmp.push(newJson);
                                    }
                                }
                                _selectedProducts = tmp;
                            });
                        }
                        $("<span>" + item.Title + "</span>").appendTo($li);
                    });
                    if (checkedCount == $(res.data).size() && checkedCount > 0 && dialog.options.seltype == "checkbox") {
                        dialog.checkAll.attr("checked", true);
                        dialog.checkAll.next("span").html("取消全选");
                    }
                    $ul.show();
                    var htmlPage = pdtMgr.Pager({ 'total': res.totalRecords, 'page': res.curPage, 'pageSize': res.pageSize, 'fun': 'SearchProducts' }, "spn", "pn");
                    if (htmlPage != '') {
                        var p = $(htmlPage);   
                        dialog.pager.append(p);
                        dialog.pager.find("a[class='pn']").each(function (i, num) {                            
                            var dpage = parseInt($(num).html());
                            $(num).removeAttr("href").removeAttr("onclick").click(function () {
                                getProducts(dpage, dialog);
                            });
                        });
                        dialog.pager.show();
                    } else {
                        dialog.pager.hide();
                    }
                }
                $(dialog).dialog("option", "position", { my: "center", at: "center", of: window });
            });
        }

        var that = this;
        var defaultOpts = { ok: function (selected) { }, cancel: function (selected) { },pageSize:25,seltype:'radio',selected:[] };
        var opts = $.extend(defaultOpts, options);
        _selectedProducts = opts.selected;
        this.options = opts;
        //render search area
        var $search = $("<div></div>").appendTo(that);
        var $searchRow1 = $("<div class=\"rowS\"></div>").appendTo($search);
        $("<label>产品类目:</label>").appendTo($searchRow1);
        var $pcate = $("<select class=\"W_inputsel\" style=\"margin-right:3px;\"></select>").appendTo($searchRow1);
        that.parent_category = $pcate;
        var $ccate = $("<select class=\"W_inputsel\"></select>").appendTo($searchRow1).hide();
        that.child_category = $ccate;
        $pcate.change(function (e) {
            that.child_category.empty().hide();
            var pid = $(this).val();
            if (pid == 0) {                
                return;
            }
            pdtMgr.GetCategories({ 'parent_id': pid }, function (res) {
                if (res != null) {
                    that.child_category.empty();
                    that.child_category.append("<option value=\"0\">--选择--</option>");
                    $(res.data).each(function (index, item) {
                        that.child_category.append("<option value=\"" + item.ID + "\">" + item.Name + "</option>");
                    });
                    that.child_category.show();
                }
            });
        });
        var $searchRow2 = $("<div class=\"rowS\"></div>").appendTo($search);
        $("<label>产品名称:</label>").appendTo($searchRow2);
        var $keyword = $("<input class=\"W_input\" id=\"\" type=\"text\" style=\"width:250px;\"/>").appendTo($searchRow2);
        that.keyword = $keyword;
        var $searchRow3 = $("<div class=\"rowS nolabel1\"></div>").appendTo($search);
        var $btnSearch = $("<span>搜索</span>").appendTo($searchRow3).button({ icons: { primary: "ui-icon-search" } }).click(function (e) {
            getProducts(1, that);
        });

        //loading area
        var $loading = $('<div class="rowS" style="display:none;"></div>').appendTo(that);
        $('<span style="height:20px;line-height:20px;vertical-align: middle;"><img style="vertical-align: middle;" src="/Content/images/loading.gif"/>正在搜索产品,请稍等...</span>').appendTo($loading);
        that.loading = $loading;

        //check all
        if (that.options.seltype == 'checkbox') {
            var $checkAllRow = $('<div class="row rowTool" style="border:0;padding-left:5px;margin-top:15px;"></div>').appendTo(that);
            var $chkAllBox = $('<input type="checkbox" disabled title="全选"/>').appendTo($checkAllRow).click(function (e) {
                var checked = $(this).attr("checked");
                if (checked == "checked") {                    
                    $(this).next("span").html("取消全选");
                    $(that.products_list).find("li").each(function (j, pdt) {
                        $(pdt).find("input[type='checkbox']").attr("checked", true);                       
                        var checked = $(pdt).find("input[type='checkbox']").attr('checked');
                        var curId = $(pdt).find("input[type='checkbox']").attr('value');
                        var name = $(pdt).find("input[type='checkbox']").next("span").html();
                        var newJson = { product_id: curId, product_name: name };
                        var existed = false;
                        for (var i = 0; i < _selectedProducts.length; i++) {
                            if (_selectedProducts[i].product_id == curId) {
                                existed = true;
                                break;
                            }
                        }                        
                        if (!existed) {
                            _selectedProducts.push(newJson);
                        }
                    });
                } else {
                    $(this).next("span").html("全选");
                    $(that.products_list).find("li").each(function (j, pdt) {
                        $(pdt).find("input[type='checkbox']").removeAttr("checked");
                        var tmp = [];
                        var checked = $(pdt).find("input[type='checkbox']").attr('checked');
                        var curId = $(pdt).find("input[type='checkbox']").attr('value');
                        var name = $(pdt).find("input[type='checkbox']").next("span").html();
                        var newJson = { product_id: curId, product_name: name };
                        var existed = false;
                        for (var i = 0; i < _selectedProducts.length; i++) {
                            if (_selectedProducts[i].product_id == curId) {
                                existed = true;
                                break;
                            } else {
                                tmp.push(_selectedProducts[i]);
                            }                           
                        }
                        _selectedProducts = tmp;
                    });
                }
            });
            that.checkAll = $chkAllBox;
            $('<span style="margin-left:5px;">全选<span>').appendTo($checkAllRow);
        }

        //products list area
        var $productsList = $('<ul class="ulchk" id="opt_products_list"></ul>').appendTo(that);
        that.products_list = $productsList;

        //pager area
        var $pager = $('<div class="s_title spage pager" id="opt_products_list_Pager" style="display:none;"></div>').appendTo(that);
        that.pager = $pager;
        $(that).dialog({
            width: 550,
            resizable: false,
            title: "选择产品",
            modal: true,
            autoOpen:false,
            open: function () {                
                $('#ulProductList').html("");
            },
            close: function () {
                $(this).html("");
                _selectedProducts = [];
            },
            buttons: {
                "确定": function (e) {
                    var products = [];
                    if (that.options.seltype == 'radio') {
                        var pid = $(that).find('input:radio[name="opt_productsel"]:checked').val();
                        if (pid != null && pid != "undefined" && parseInt(pid) > 0) {
                            var pName = $(that).find('input:radio[name="opt_productsel"]:checked').next("span").html();
                            jsonProduct = { product_id: pid, product_name: pName };
                            products.push(jsonProduct);
                            _selectedProducts = products;
                        }
                    }
                                       
                    that.options.ok(_selectedProducts);
                    $(this).dialog("close");
                },
                "取消": function (e) {
                    $(this).dialog("close");
                }
            }
        });

        pdtMgr.GetCategories({}, function (res) {
            if (res != null) {
                that.parent_category.append("<option value=\"0\">--选择--</option>");
                $(res.data).each(function (index, item) {
                    that.parent_category.append("<option value=\"" + item.ID + "\">" + item.Name + "</option>");
                });
                $(that).dialog("open");
            }
        });
    }
})(jQuery);